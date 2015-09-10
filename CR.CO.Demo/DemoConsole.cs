using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Data;
using System.Xml;
using System.Xml.Linq;

using hmrcclasses;
using CharitiesOnline.Helpers;
using CharitiesOnline.MessageBuilders;
using CharitiesOnline.Models;
using CharitiesOnline.MessageReadingStrategies;
using CharitiesOnline.MessageReadingStrategies.ErrorReader;

using CR.Infrastructure.Configuration;
using CR.Infrastructure.Logging;
using CR.Infrastructure.ContextProvider;

using CR.CO.Demo;

namespace CharitiesOnline
{
    /// <summary>
    /// The DemoConsole is a demonstration console application showing examples of how to use the types and methods provided in the
    /// Comic Relief Charities Online library. It is not intended for production use, it is merely there for instruction, demonstration
    /// and user-testing purposes
    /// </summary>
    class DemoConsole
    {
        private static ILoggingService loggingService;
        private static IConfigurationRepository configurationRepository;
        static void Main(string[] args)
        {
            try
            {
                // The configurationRepository is intended to abstract the configurationManager type and allow
                // for different configuration options to be applied. For example, a DatabaseConfigurationRepository could be provided
                // if the requirement is to take reference values from a database.
                configurationRepository = new ConfigFileConfigurationRepository();
                loggingService = new Log4NetLoggingService(configurationRepository, new ThreadContextService());

                GovTalkMessageFileName FileNamer = new GovTalkMessageFileName.FileNameBuilder()
                .AddLogger(loggingService)
                .AddMessageIntention("GatewaySubmission")
                .AddFilePath(@"C:\Temp\")
                .AddTimestamp(DateTime.Now.ToString("yyyyMMddHHmmss"))
                .AddEnvironment("production")
                .BuildFileName();

                string outputFilename = FileNamer.ToString();

                GovTalkMessageHelper helper = new GovTalkMessageHelper(configurationRepository, loggingService);

                ////// Optionally, set this to a valid filepath for a CSV that contains GiftAid data in an acceptable format.
                string csvFile = @"C:\Temp\testdata.csv";

                #region Testing
                //TestGovTalkMessageCreation(csvFile, outputFilename);

                //XmlDocument LiveXml = new XmlDocument();
                //LiveXml.PreserveWhitespace = true;
                //LiveXml.Load(outputFilename);              
                //XmlDocument LocalTestXml = new XmlDocument();
                //LocalTestXml.PreserveWhitespace = true;
                //LocalTestXml = helper.UpdateMessageForLocalTest(LiveXml);
                //LocalTestXml.Save(@"C:\Temp\Localsend.xml");
                #endregion Testing

                //// Create a GovTalkMessage and save the Xml to disk
                string submitMessageFilename = DemonstrateCreateSubmitRequest(loggingService, configurationRepository, csvFile);                                
                 
                XmlDocument submitMessageXml = new XmlDocument();

                //// It is important if the XML message is being loaded from disk to preserve whitespace, otherwise the IRmark will be out for non-compressed files
                submitMessageXml.PreserveWhitespace = true;
                submitMessageXml.Load(outputFilename);

                XmlDocument submitMessageReply = DemonstrateSendMessage(loggingService, submitMessageXml);

                DemonstrateReadMessage(loggingService, submitMessageReply);
            }
            catch (System.Net.WebException wex)
            { 
                loggingService.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, "Exception occured in connecting to remote machine", wex);

                //Console.WriteLine("Exception occured in connecting to remote machine");
                //Console.WriteLine(wex.InnerException.Message);
                //Console.WriteLine(wex.Message);
            }
            catch (Exception ex)
            {
                loggingService.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, "Something went wrong", ex);
                //Console.WriteLine(ex);
            }
            finally
            {
                Console.ReadKey();
            }
        }
        
        /// <summary>
        /// Shows how to create a Submit Request GovTalkMessage, the basic type for submitting a GiftAid claim.
        /// If the message is very large it can be compressed, or compression can be chosen at the start of the process.
        /// Password and IRmark are added after the message is created.
        /// </summary>
        /// <param name="loggingService"></param>
        static string DemonstrateCreateSubmitRequest(ILoggingService loggingService, IConfigurationRepository configurationRepository, string giftAidDataSourceCsvFile)
        {
            // Assign a reference data source
            ReferenceDataManager.SetSource(ReferenceDataManager.SourceTypes.ConfigFile);

            // Set logger for the repaymentPopulater
            DataTableRepaymentPopulater.SetLogger(loggingService);
            
            // Assign a source for the GiftAid repayments data
            // If a filepath has been passed in, the DataHelpers method will make a datatable from a CSV source 
            // with a valid set of columns. Otherwise, grab a datatable from a database or some other source
            // If the repaymentpopulater is not given a datatable, the submission message will have no repayments in it
            if (!string.IsNullOrEmpty(giftAidDataSourceCsvFile))
                DataTableRepaymentPopulater.GiftAidDonations = DataHelpers.GetDataTableFromCsv(giftAidDataSourceCsvFile, true);            
            
            GovTalkMessageCreator submitMessageCreator = new GovTalkMessageCreator(new SubmitRequestMessageBuilder(loggingService), loggingService);

            submitMessageCreator.CreateGovTalkMessage();

            GovTalkMessage submitMessage = submitMessageCreator.GetGovTalkMessage();

            // Set a password if not using the password hard-coded in the configuration source
            GovTalkMessageHelper helper = new GovTalkMessageHelper(configurationRepository, loggingService);
            helper.SetPassword(submitMessage, "weirdpassword");

            XmlDocument submitMessageXml = submitMessageCreator.SerializeGovTalkMessage();

            GovTalkMessageHelper gtmHelper = new GovTalkMessageHelper(configurationRepository, loggingService);

            XmlDocument irMarkedMessageXml = gtmHelper.SetIRmark(submitMessageXml);

            // If the message is too big, compress it
            
            // byte[] xmlDocumentSize = xd.XmlToBytes();

            // if (xmlDocumentSize.Length > 1000000)
            // {
            //    XmlDocument compressedVersion = submitMessageCreator.CompressClaim();
            //    outputXmlDocument = GovTalkMessageHelper.SetIRmark(compressedVersion);
            // }

            // Optionally, create a filename using this helper class

            string outputFilename;
            string tempDirectory = configurationRepository.GetConfigurationValue<string>("TempFolder");

            GovTalkMessageFileName FileNamer = new GovTalkMessageFileName.FileNameBuilder()
                .AddLogger(loggingService)
                .AddMessageIntention("GatewaySubmission")
                .AddFilePath(tempDirectory)
                .AddTimestamp(DateTime.Now.ToString("yyyyMMddHHmmss"))
                .AddEnvironment("local")
                .AddCustomNamePart("EmptyRepayment")
                .BuildFileName();
            
            outputFilename = FileNamer.ToString();

            irMarkedMessageXml.Save(outputFilename);

            return outputFilename;
        }

        
        /// <summary>
        /// Demonstrate using the MessageSendingService to send a GovTalkMessage to the Government Gateway and receive a reply.
        /// </summary>
        /// <param name="loggingService"></param>
        /// <param name="sendMessage"></param>
        static XmlDocument DemonstrateSendMessage(ILoggingService loggingService, XmlDocument sendMessage)
        {
            string uri = configurationRepository.GetConfigurationValue<string>("SendURILocal");

            // Create a client to send the file to the target gateway
            CharitiesOnline.MessageService.Client client = new MessageService.Client(loggingService);

            // Create an XmlDocument of the reply from the endpoint
            XmlDocument reply = client.SendRequest(sendMessage, uri);

            return reply;        
        }

        /// <summary>
        /// Demonstrate using the message reader strategies to get results from a message reply
        /// </summary>
        /// <param name="loggingService"></param>
        /// <param name="messageToRead"></param>
        static void DemonstrateReadMessage(ILoggingService loggingService, XmlDocument messageToRead)
        {
            // Set up a message reading strategy
            IMessageReader messageReader = new DefaultMessageReader(loggingService, configurationRepository, messageToRead.ToXDocument());
            messageReader.ReadMessage();

            // We don't know what we've got back from the Gateway, but all replies are GovTalkMessages
            if(messageReader.HasErrors())
            {
                //There are errors in the results file so we can deal with them

                // Get a DataTable of the results and have a look at that
                DataTable errorTable = messageReader.GetMessageResults<DataTable>();
                // Or set up an error return strategy and do something with that
                IErrorReturnCalculator errorCalculator = new DefaultErrorReturnCalculator();
                GovTalkMessageGovTalkDetailsError error = messageReader.GetMessageResults<GovTalkMessageGovTalkDetailsError>();
                
                Console.WriteLine(errorCalculator.CalculateErrorReturn(error));

                if(error.Number == "3001")
                {
                    ErrorResponse errResponse = messageReader.GetMessageResults<ErrorResponse>();                    
                }
            }
            else
            {
                // It's either an acknowledgement so we need to get the poll interval and URL, or a response.
                string[] results = messageReader.GetMessageResults<string[]>();

                foreach(var result in results)
                {
                    Console.WriteLine(result);
                }

                if(messageReader.GetQualifier() == "response")
                {
                    DataTable responseTable = messageReader.GetMessageResults<DataTable>();

                    LocalHelp.ConsolePrintDataTable(responseTable);
                }
            }

            GovTalkMessageFileName ReplyNamer = new GovTalkMessageFileName.FileNameBuilder()
            .AddLogger(loggingService)
            .AddConfigurationRepository(configurationRepository)
            .AddMessageIntention("ReplyMessage")
            .AddCorrelationId(messageReader.GetCorrelationId())
            .AddFilePath(@"C:\Temp\")
            .BuildFileName();

            string replyFileName = ReplyNamer.ToString();

            messageToRead.Save(replyFileName);
        }

        public static void DemonstrateLocalProcess()
        {
            // Set up the logging
            IConfigurationRepository configurationRepository = new ConfigFileConfigurationRepository();
            ILoggingService loggingService = new Log4NetLoggingService(configurationRepository, new ThreadContextService());

            DataTableRepaymentPopulater.SetLogger(loggingService);

            // Create a file of donations records
            DataTableRepaymentPopulater.GiftAidDonations = DataHelpers.GetDataTableFromCsv(@"C:\Temp\Donations.csv", true);

            // Set up app.config as a source for the reference data
            ReferenceDataManager.SetSource(ReferenceDataManager.SourceTypes.ConfigFile);

            // Build a GovTalkMessage
            GovTalkMessageCreator submitMessageCreator = new GovTalkMessageCreator(new SubmitRequestMessageBuilder(loggingService), loggingService);
            submitMessageCreator.CreateGovTalkMessage();

            // Get the GovTalkMessage that has been built
            GovTalkMessage submitMessage = submitMessageCreator.GetGovTalkMessage();

            // Serialize the GovTalkMessage to an XmlDocument
            XmlDocument xd = submitMessageCreator.SerializeGovTalkMessage();

            // Set the IRmark for the GovTalkMessage XmlDocument
            GovTalkMessageHelper gtmHelper = new GovTalkMessageHelper(configurationRepository, loggingService);
            XmlDocument finalXd = gtmHelper.SetIRmark(xd);

            // Set the URI to send the file to
            string uri = configurationRepository.GetConfigurationValue<string>("SendURILocal");

            // Create a client to send the file to the target gateway
            CharitiesOnline.MessageService.Client client = new MessageService.Client(loggingService);

            // Create an XmlDocument of the reply from the endpoint
            XmlDocument reply = client.SendRequest(xd, uri);

            // Set up a message reading strategy
            IMessageReader _messageReader = new DefaultMessageReader(loggingService,configurationRepository,reply.ToXDocument());
            _messageReader.ReadMessage();

            string[] results = _messageReader.GetMessageResults<string[]>();

            //int correlationIdIndex = Array.IndexOf(results, "CorrelationId");
            int correlationIdPosition = Array.FindIndex(results, element => element.StartsWith("CorrelationId"));
            if (correlationIdPosition < 0)
                throw new ArgumentNullException("CorrelationId");

            int qualifierPosition = Array.FindIndex(results, element => element.StartsWith("Qualifier"));

            if (qualifierPosition < 0)
                throw new ArgumentNullException("Qualifier");

            Console.WriteLine(string.Join("\n", results));

            #region old
            // This bit, bunch of if-thens, should be covered by the reader strategy ...
            //string bodytype = _messageReader.GetBodyType(reply.ToXDocument());                        

            //if(bodytype == null)
            //{
            //    //acknowledgment
            //    Console.WriteLine("CorrelationId is {0}",_messageReader.ReadMessage<string>(reply.ToXDocument()));
            //}
            //else if(bodytype == "hmrcclasses.SuccessResponse")
            //{
            //    //success
            //    string[] success = _messageReader.ReadMessage<string[]>(reply.ToXDocument());
            //    Console.WriteLine(string.Join("\n", success));
            //}
            //else if(bodytype == "hmrcclasses.ErrorResponse")
            //{
            //    //error
            //    string[] error = _messageReader.ReadMessage<string[]>(reply.ToXDocument());
            //    Console.WriteLine(string.Join("\n", error));
            //}

            #endregion old

            // @TODO Need a method in the reader for generating a good filepath for messages
            // Made a utility to do it
            // Need to get correlationId

            GovTalkMessageFileName fileNamer = new GovTalkMessageFileName.FileNameBuilder()
            .AddLogger(loggingService)
            .AddFilePath(configurationRepository.GetConfigurationValue<string>("TempFolder"))
            .AddEnvironment("local")
            .AddMessageIntention("reply")
            .AddCorrelationId(results[correlationIdPosition].Substring(results[correlationIdPosition].IndexOf("::") + 2))
            .AddMessageQualifier(results[qualifierPosition].Substring(results[qualifierPosition].IndexOf("::") + 2)) //could check for < 0 here and pass empty string
            .BuildFileName();

            string filename = fileNamer.ToString();

            reply.Save(filename);

            // reply.Save(@"C:\Temp\localreply.xml");

        }

        #region TestingAndDevelopment

        static void TestReadErrors(IConfigurationRepository configurationRepository, ILoggingService loggingService)
        {

            XDocument errorDoc = XDocument.Load(@"C:\Temp\test_reply__error_20150825151729.xml");

            IMessageReader reader = new DefaultMessageReader(loggingService, configurationRepository, errorDoc);

            reader.ReadMessage();

            DataTable errorTable = reader.GetMessageResults<DataTable>();

            foreach (DataColumn column in errorTable.Columns)
                Console.Write("\t{0}", column.ColumnName);

            Console.WriteLine("");
            foreach (DataRow row in errorTable.Rows)
            {
                foreach (DataColumn column in errorTable.Columns)
                    Console.Write("\t{0}", row[column]);
                Console.WriteLine("");
            }

            errorTable.Clear();

            errorDoc = XDocument.Load(@"C:\Temp\RequestMessage_1422880486_File14393268203594585061_error_20150202123518_.xml");
            reader = new DefaultMessageReader(loggingService, configurationRepository, errorDoc);
            reader.ReadMessage();

            errorTable = reader.GetMessageResults<DataTable>();

            Console.WriteLine("");
            foreach (DataRow row in errorTable.Rows)
            {

                foreach (DataColumn column in errorTable.Columns)
                    Console.Write("\t{0}", row[column]);
                Console.WriteLine("");
            }

            Console.ReadKey();
        }

        static void TestReadListResponse()
        {
            IConfigurationRepository configurationRepository = new ConfigFileConfigurationRepository();
            loggingService = new Log4NetLoggingService(configurationRepository, new ThreadContextService());

            LogProviderContext.Current = loggingService;

            string filename = @"C:\Temp\LocalDataRequestMessage_2014_10_16_10_33_50.xml";

            XmlDocument listResponse = new XmlDocument();
            listResponse.Load(filename);

            IMessageReader reader = new DefaultMessageReader(loggingService, configurationRepository, listResponse.ToXDocument());
            reader.ReadMessage();

            string[] results = reader.GetMessageResults<string[]>();

            string qualifier = reader.GetQualifier();
            string function = reader.GetFunction();

            DataTable listResults = reader.GetMessageResults<DataTable>();

            GovTalkMessage message = reader.Message();

            Console.WriteLine("Message from {0}", message.Header.MessageDetails.ResponseEndPoint.Value.ToString());
        }

        public static void TestFileNaming(ILoggingService loggingService, IConfigurationRepository configurationRepository)
        {

            GovTalkMessageFileName filename = (new GovTalkMessageFileName.FileNameBuilder()
            .AddLogger(loggingService)
            .AddFilePath(@"")
            .AddEnvironment("Test")
            .AddMessageIntention("RequestMessage")
            .AddTimestamp(DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss", System.Globalization.CultureInfo.InvariantCulture))
            .AddCustomNamePart("File" + 1)
            .BuildFileName()
            );

            Console.WriteLine(filename.ToString());

            Console.WriteLine(filename.Environment);

            filename.Environment = "production_";

            Console.WriteLine(filename.ToString());

            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            int FileSequence = 0;

            GovTalkMessageFileName FileNamer = new GovTalkMessageFileName.FileNameBuilder()
               .AddConfigurationRepository(configurationRepository)
               .AddLogger(loggingService)
               .AddEnvironment("localtest")
               .AddFilePath(@"C:\Temp\")
               .AddMessageIntention("SubmitRequest")
               .AddTimestamp(timestamp)
               .AddCustomNamePart(String.Concat("152",(FileSequence > 0 ? "_" + FileSequence.ToString() : "")))
            .BuildFileName();

            Console.WriteLine(FileNamer.ToString());


            Console.ReadKey();
        }

        public static void TestDeserializeSuccessResponse(ILoggingService loggingService)
        {
            GovTalkMessageFileName FileNamer = new GovTalkMessageFileName.FileNameBuilder()
                .AddLogger(loggingService)
                .AddFilePath(@"C:\Temp\")
                .AddEnvironment("live")
                .AddMessageIntention("PollMessage")
                .AddTimestamp("2015_02_02_12_41")
                .AddCorrelationId("1853DE80F71CEF4C07B57CD5BDA969D577")
                .AddMessageQualifier("response")
                .AddCustomNamePart("20150202124118")
                .BuildFileName();

            string filename = FileNamer.ToString();

            XmlDocument successMessage = new XmlDocument();
            successMessage.Load(filename);

            GovTalkMessage success = XmlSerializationHelpers.DeserializeMessage(successMessage);

            XmlDocument successXml = new XmlDocument();

            successXml.LoadXml(success.Body.Any[0].OuterXml);

            SuccessResponse successResp = XmlSerializationHelpers.DeserializeSuccessResponse(successXml);

            loggingService.LogInfo(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, successResp.Message[0].Value);
        }

        public static void TestReadSuccessResponse()
        {
            IConfigurationRepository configurationRepository = new ConfigFileConfigurationRepository();
            ILoggingService loggingService = new Log4NetLoggingService(configurationRepository, new ThreadContextService());

            XmlDocument successResponse = new XmlDocument();
            successResponse.Load(@"C:\Temp\success_response_78503626913182048.xml");

            ReadResponseStrategy reader = new ReadResponseStrategy(loggingService);
            if (reader.IsMatch(successResponse.ToXDocument()))
            {
                SuccessResponse success = reader.GetBody<SuccessResponse>();
            }
        }

        public static void TestReadMessages(IMessageReader messageReader)
        {
            IMessageReader _messageReader = messageReader;
            _messageReader.ReadMessage();

            XmlDocument messageXML = new XmlDocument();
            messageXML.Load(@"C:\Temp\RequestMessage_1423572802_File187E1E8A7F16147A6B87962E07933B406_acknowledgement_20150210125836_.xml");

            string[] results = _messageReader.GetMessageResults<string[]>();

            foreach (var s in results)
            {
                Console.WriteLine(s);
            }

            // GovTalkMessage message = _messageReader.Message(messageXML.ToXDocument());

            string bodytype = _messageReader.GetBodyType();

            Console.WriteLine(bodytype);

            ErrorResponse err = _messageReader.GetBody<ErrorResponse>();

        }

        static XmlDocument TestSend(string filename = "")
        {
            IConfigurationRepository configurationRepository = new ConfigFileConfigurationRepository();
            ILoggingService loggingService = new Log4NetLoggingService(configurationRepository, new ThreadContextService());

            string uri = configurationRepository.GetConfigurationValue<string>("SendURIDev");

            XmlDocument xd = new XmlDocument();
            xd.PreserveWhitespace = true;

            if(filename == "")
            {
                filename = @"C:\Temp\test_GatewaySubmission_20150818163247_EmptyRepayment.xml";
            }            

            xd.Load(filename);

            CharitiesOnline.MessageService.Client client = new MessageService.Client(loggingService);

            XmlDocument reply = client.SendRequest(xd, uri);

            Console.WriteLine(reply.InnerXml);

            return reply;
            
        }

        public static void TestReadMessage()
        {
            IConfigurationRepository configurationRepository = new ConfigFileConfigurationRepository();
            ILoggingService loggingService = new Log4NetLoggingService(configurationRepository, new ThreadContextService());

            // vep200pc
            // XDocument xd = XDocument.Load(@"C:\Temp\Compressed Data Samples\GAValidSample_GZIP.xml");
            XDocument xd = XDocument.Load(@"C:\Temp\GAValidSample_GZIP.xml");

            ReadSubmitRequestStrategy read = new ReadSubmitRequestStrategy(loggingService);
            read.IsMatch(xd);

            DataTable dt = read.GetMessageResults<DataTable>();

        }

        public static void TestReferenceData()
        {

            ReferenceDataManager.SetSource(ReferenceDataManager.SourceTypes.ConfigFile);

            //DataTable keyValueDt = Helpers.GetDataTableFromCsv(@"C:\Temp\Key-Value.csv", true);

            //ReferenceDataManager.SetDataTable(keyValueDt, ReferenceDataManager.DatabaseType.KeyValue);

            //ReferenceDataManager.SetSource(ReferenceDataManager.SourceTypes.DatabaseKeyValue);

            //foreach(string k in ReferenceDataManager.Settings)
            //{
            //    Console.WriteLine("Key {0} is {1}", k, ReferenceDataManager.Settings[k]);
            //}

            //Console.WriteLine("=======================");
            //Console.WriteLine("=======================");

            //DataTable defaultHeadersDt = Helpers.GetDataTableFromCsv(@"C:\Temp\DefaultHeader.csv", true);

            //ReferenceDataManager.SetDataTable(defaultHeadersDt, ReferenceDataManager.DatabaseType.DefaultHeader);
            //ReferenceDataManager.SetSource(ReferenceDataManager.SourceTypes.DatabaseDefaultHeader);

            //foreach (string k in ReferenceDataManager.Settings)
            //{
            //    Console.WriteLine("Key {0} is {1}", k, ReferenceDataManager.Settings[k]);
            //}



            // Console.WriteLine(ReferenceDataManager.Settings["R68AuthOfficialSurname"]);

            // Console.ReadKey();
        }

        public static void TestGovTalkMessageCreation(string SourceDataFileName, string Filename = "")
        {
            IConfigurationRepository configurationRepository = new ConfigFileConfigurationRepository();
            ILoggingService loggingService = new Log4NetLoggingService(configurationRepository, new ThreadContextService());

            ReferenceDataManager.SetSource(ReferenceDataManager.SourceTypes.ConfigFile);
            ReferenceDataManager.governmentGatewayEnvironment = GovernmentGatewayEnvironment.productiongateway;

            DataTableRepaymentPopulater.SetLogger(loggingService);

            if (!string.IsNullOrEmpty(SourceDataFileName))
                DataTableRepaymentPopulater.GiftAidDonations = DataHelpers.GetDataTableFromCsv(@SourceDataFileName, true);

            GovTalkMessageCreator submitMessageCreator = new GovTalkMessageCreator(new SubmitRequestMessageBuilder(loggingService), loggingService);

            submitMessageCreator.CreateGovTalkMessage();

            GovTalkMessage submitMessage = submitMessageCreator.GetGovTalkMessage();

            GovTalkMessageHelper helper = new GovTalkMessageHelper(configurationRepository, loggingService);
            helper.SetPassword(submitMessage, "testing1");

            XmlDocument xd = submitMessageCreator.SerializeGovTalkMessage();
            xd.PreserveWhitespace = true;

            xd = helper.AddPassword(xd.ToXDocument(), "xdocpassword", "clear").ToXmlDocument();          

            byte[] xmlDocumentSize = xd.XmlToBytes();

            Console.WriteLine("The document is {0} bytes big.", xmlDocumentSize.Length);

            XmlDocument outputXmlDocument = new XmlDocument();
            outputXmlDocument.PreserveWhitespace = true;

            //if (xmlDocumentSize.Length > 1000000)
            //{
            //    XmlDocument compressedVersion = submitMessageCreator.CompressClaim();
            //    outputXmlDocument = GovTalkMessageHelper.SetIRmark(compressedVersion);
            //}
            //else
            //{

            GovTalkMessageHelper gtmHelper = new GovTalkMessageHelper(configurationRepository, loggingService);
            outputXmlDocument = gtmHelper.SetIRmark(xd);
            //}

            string filename;

            if (Filename == "")
            {
                GovTalkMessageFileName FileNamer = new GovTalkMessageFileName.FileNameBuilder()
                .AddLogger(loggingService)
                .AddMessageIntention("GatewaySubmission")
                .AddFilePath(@"C:\Temp\")
                .AddTimestamp(DateTime.Now.ToString("yyyyMMddHHmmss"))
                .AddEnvironment(ReferenceDataManager.governmentGatewayEnvironment.ToString())
                .BuildFileName();

                filename = FileNamer.ToString();
            }
            else
            {
                filename = Filename;
            }
            
            outputXmlDocument.Save(filename);

            #region old
            //BodyCreator bodyCreator = new BodyCreator(new SubmitRequestBodyBuilder());
            //bodyCreator.CreateBody();
            //GovTalkMessageBody body = bodyCreator.GetBody();

            //BodyCreator pollBodyCreator = new BodyCreator(new SubmitPollBodyBuilder());
            //pollBodyCreator.CreateBody();
            //GovTalkMessageBody pollBody = pollBodyCreator.GetBody();
            #endregion old
        }

        public static void TestOtherIncome()
        {
            DataTableRepaymentPopulater.GiftAidDonations = DataHelpers.GetDataTableFromCsv(@"C:\Temp\Donations.csv", true);
            DataTableRepaymentPopulater.OtherIncome = DataHelpers.GetDataTableFromCsv(@"C:\Temp\OtherInc.csv", true);

            ReferenceDataManager.SetSource(ReferenceDataManager.SourceTypes.ConfigFile);

            IConfigurationRepository configurationRepository = new ConfigFileConfigurationRepository();
            ILoggingService loggingService = new Log4NetLoggingService(configurationRepository, new ThreadContextService());

            GovTalkMessageCreator submitMessageCreator = new GovTalkMessageCreator(new SubmitRequestMessageBuilder(loggingService), loggingService);
            submitMessageCreator.CreateGovTalkMessage();
            hmrcclasses.GovTalkMessage submitMessage = submitMessageCreator.GetGovTalkMessage();

            XmlDocument xd = submitMessageCreator.SerializeGovTalkMessage();
            
            GovTalkMessageHelper gtmHelper = new GovTalkMessageHelper(configurationRepository, loggingService);
            XmlDocument finalXd = gtmHelper.SetIRmark(xd);

            GovTalkMessageFileName filename = (new GovTalkMessageFileName.FileNameBuilder()
            .AddFilePath(@"C:\Temp\")
            .AddEnvironment("Test")
            .AddMessageIntention("GovTalkMsgWithOtherIncome")
            .AddTimestamp(DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss", System.Globalization.CultureInfo.InvariantCulture))
            .BuildFileName()
            );

            finalXd.Save(filename.ToString());

        }

        public static void TestGovTalkCompressedMessageCreation()
        {
            DataTableRepaymentPopulater.GiftAidDonations = DataHelpers.GetDataTableFromCsv(@"C:\enterprise_tfs\GAVIN\CO\test_data\sample2.csv", true);
            DataTableRepaymentPopulater.OtherIncome = DataHelpers.GetDataTableFromCsv(@"C:\Temp\OtherInc.csv", true);

            // C:\Temp\Donations.csv

            ReferenceDataManager.SetSource(ReferenceDataManager.SourceTypes.ConfigFile);

            IConfigurationRepository configurationRepository = new ConfigFileConfigurationRepository();
            ILoggingService loggingService = new Log4NetLoggingService(configurationRepository, new ThreadContextService());

            GovTalkMessageCreator compressedSubmissionCreator = new GovTalkMessageCreator(new SubmitRequestCompressedMessageBuilder(loggingService), loggingService);
            compressedSubmissionCreator.CreateGovTalkMessage();

            XmlDocument xd = compressedSubmissionCreator.SerializeGovTalkMessage();

            GovTalkMessageHelper gtmHelper = new GovTalkMessageHelper(configurationRepository, loggingService);
            XmlDocument finalXd = gtmHelper.SetIRmark(xd);

            GovTalkMessageFileName filename = (new GovTalkMessageFileName.FileNameBuilder()
            .AddFilePath(@"C:\Temp\")
            .AddEnvironment("Test")
            .AddMessageIntention("CompressedGovTalkMessage")
            .AddTimestamp(DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss", System.Globalization.CultureInfo.InvariantCulture))
            .AddCustomNamePart("File" + 1)
            .BuildFileName()
            );

            finalXd.Save(filename.ToString());

        }

        public static void TestGovTalkDetails()
        {
            TestReferenceData();
            GovTalkDetailsCreator govTalkDetailsCreator = new GovTalkDetailsCreator(new SubmitRequestGovTalkDetailsBuilder());
            govTalkDetailsCreator.CreateGovTalkDetails();

            hmrcclasses.GovTalkMessageGovTalkDetails govTalkDetails = govTalkDetailsCreator.GetGovTalkDetails();

            XmlElement xml = govTalkDetailsCreator.SerializeGovTalkDetails(govTalkDetails);

            Console.WriteLine(xml.InnerXml.ToString());

        }

        public static void TestPollrequest()
        {
            ReferenceDataManager.SetSource(ReferenceDataManager.SourceTypes.ConfigFile);

            IConfigurationRepository configurationRepository = new ConfigFileConfigurationRepository();
            ILoggingService loggingService = new Log4NetLoggingService(configurationRepository, new ThreadContextService());

            GovTalkMessageCreator submitPollCreator = new GovTalkMessageCreator(new SubmitPollMesageBuilder(loggingService), loggingService);

            submitPollCreator.SetCorrelationId("52CB8A8AA58148859377830BAE5B99C9");

            submitPollCreator.CreateGovTalkMessage();

            XmlDocument xd = submitPollCreator.SerializeGovTalkMessage();

            xd.Save(@"c:\Temp\pollsubmit-" + DateTime.Now.ToString("_yyyy_MM_dd_HH_mm_ss", System.Globalization.CultureInfo.InvariantCulture) + ".xml");

        }

        public static void TestDeleterequest()
        {
            ReferenceDataManager.SetSource(ReferenceDataManager.SourceTypes.ConfigFile);
            IConfigurationRepository configurationRepository = new ConfigFileConfigurationRepository();
            ILoggingService loggingService = new Log4NetLoggingService(configurationRepository, new ThreadContextService());

            GovTalkMessageCreator deleteRequestCreator = new GovTalkMessageCreator(new DeleteRequestMessageBuilder(loggingService), loggingService);
            deleteRequestCreator.SetCorrelationId("hjcjc");
            deleteRequestCreator.CreateGovTalkMessage();

            XmlDocument xd = deleteRequestCreator.SerializeGovTalkMessage();
            xd.Save(@"C:\Temp\deleterequest-" + DateTime.Now.ToString("_yyyy_MM_dd_HH_mm_ss", System.Globalization.CultureInfo.InvariantCulture) + ".xml");
        }

        public static void TestListRequest()
        {
            ReferenceDataManager.SetSource(ReferenceDataManager.SourceTypes.ConfigFile);

            IConfigurationRepository configurationRepository = new ConfigFileConfigurationRepository();
            ILoggingService loggingService = new Log4NetLoggingService(configurationRepository, new ThreadContextService());

            GovTalkMessageCreator listRequestCreator = new GovTalkMessageCreator(new ListRequestMessageBuilder(loggingService), loggingService);
            listRequestCreator.CreateGovTalkMessage();

            XmlDocument xd = listRequestCreator.SerializeGovTalkMessage();
            xd.Save(@"C:\Temp\listrequest-" + DateTime.Now.ToString("_yyyy_MM_dd_HH_mm_ss", System.Globalization.CultureInfo.InvariantCulture) + ".xml");

            // Helpers.SerializeToFile(listRequestCreator.GetGovTalkMessage(), @"C:\Temp\listrequest-" + DateTime.Now.ToString("_yyyy_MM_dd_HH_mm_ss", System.Globalization.CultureInfo.InvariantCulture) + ".xml");

        }

        public static void HackIRMark()
        {
            XmlDocument xd = new XmlDocument();
            xd.PreserveWhitespace = true;
            xd.Load(@"C:\Temp\TestGovTalkMsgWithOtherIncome_2015_06_30_11_32_29.xml");

            GovTalkMessageHelper gtmHelper = new GovTalkMessageHelper(configurationRepository, loggingService);
            XmlDocument finalXd = gtmHelper.SetIRmark(xd);

            finalXd.Save(@"C:\Temp\testGovTalkMsgWithIrMark" + DateTime.Now.ToString("_yyyy_MM_dd_HH_mm_ss", System.Globalization.CultureInfo.InvariantCulture) + ".xml");
        }

        public static void TestDeserialize()
        {
            XmlDocument xd = new XmlDocument();
            xd.Load(@"C:\CharitiesOnline\CharitiesValidSamples_February_2015\Compressed Data Samples\GAValidSample_GZIP.xml");

            GovTalkMessage gtm = XmlSerializationHelpers.DeserializeMessage(xd);

            XmlElement xelement = gtm.Body.Any[0];

            // XmlDocument bodyDoc = xelement.OwnerDocument;            
            XmlDocument bodyDoc = new XmlDocument();
            bodyDoc.LoadXml(xelement.OuterXml);

            IRenvelope ire = XmlSerializationHelpers.DeserializeIRenvelope(bodyDoc);

            R68CompressedPart compressedPart = (R68CompressedPart)ire.R68.Items[0];

            string decompressed = CommonUtilityHelper.DecompressData(compressedPart.Value);
        }

        public static void TestSerialize()
        {
            R68ClaimGASDS gasds = new R68ClaimGASDS();
            gasds.ConnectedCharities = r68_YesNoType.no;
            gasds.CommBldgs = r68_YesNoType.no;

            R68ClaimRepaymentOtherInc otherinc = new R68ClaimRepaymentOtherInc();
            otherinc.Payer = "Peter Other";
            otherinc.Gross = 13.12M;
            otherinc.OIDate = Convert.ToDateTime("2014-10-31");
            otherinc.Tax = 2.62M;

            R68ClaimRepaymentOtherInc[] OtherIncs = new R68ClaimRepaymentOtherInc[1];
            OtherIncs[0] = otherinc;

            R68ClaimRepaymentGADDonor donor = new R68ClaimRepaymentGADDonor();
            donor.Fore = "Jane";
            donor.Sur = "Smith";
            donor.House = "1";
            donor.Item = "BA23 9CD";

            R68ClaimRepaymentGAD gad = new R68ClaimRepaymentGAD();
            gad.Item = donor;
            gad.TotalString = "12.00";
            gad.Date = Convert.ToDateTime("2014-10-03");

            XmlDocument xmlGad =
                XmlSerializationHelpers.SerializeItem(gad);

            byte[] bytes = Encoding.UTF8.GetBytes(xmlGad.OuterXml);



            xmlGad.Save(@"C:\Temp\GAD.xml");

            R68ClaimRepaymentGAD[] GADS = new R68ClaimRepaymentGAD[1];
            GADS[0] = gad;

            R68ClaimRepayment repayment = new R68ClaimRepayment();
            repayment.EarliestGAdateSpecified = true;
            repayment.EarliestGAdate = Convert.ToDateTime("2014-10-03");
            repayment.GAD = GADS;
            repayment.OtherInc = OtherIncs;

            R68ClaimRegulator regulator = new R68ClaimRegulator();
            regulator.Item = R68ClaimRegulatorRegName.CCEW;
            regulator.RegNo = "A1234";

            R68Claim claim = new R68Claim();
            claim.OrgName = "My Organisation";
            claim.HMRCref = "AA12345";
            claim.GASDS = gasds;
            claim.Regulator = regulator;
            claim.Repayment = repayment;

            XmlDocument claimXml =
                XmlSerializationHelpers.SerializeItem(claim);



            claimXml.Save(@"C:\Temp\R68Claim.xml");
        }

        public static void TestDeserializeClaim()
        {
            XmlDocument claimXml = new XmlDocument();
            claimXml.Load(@"C:\Temp\R68Claim.xml");

            R68Claim r68claim = XmlSerializationHelpers.DeserializeR68Claim(claimXml);

            //Helpers.Deserialize<R68Claim>(claimXml.OuterXml, "R68Claim");

        }

        #endregion TestingAndDevelopment
    }
}
