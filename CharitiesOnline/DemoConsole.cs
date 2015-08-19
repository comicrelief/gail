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
using CharitiesOnline.Builders;
using CharitiesOnline.Strategies;

using CR.Infrastructure.Configuration;
using CR.Infrastructure.Logging;
using CR.Infrastructure.ContextProvider;

namespace CharitiesOnline
{
    class DemoConsole
    {
        private static ILoggingService loggingService;
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Started");

                IConfigurationRepository configurationRepository = new ConfigFileConfigurationRepository();
                loggingService = new Log4NetLoggingService(configurationRepository, new ThreadContextService());

                LogProviderContext.Current = loggingService;

                Type type = typeof(DemoConsole);
                    //System.Reflection.MethodBase.GetCurrentMethod().DeclaringType;

                loggingService.LogInfo(type, "Logging!");

                LogProviderContext.Current.LogInfo(type, "Logging from contextual log provider");

                // TestReadMessages(reader);

                // TestFileNaming();

                TestDeserializeSuccessResponse(loggingService);

                // TestSerialize();
                TestLocalProcess();
                // TestGovTalkMessageCreation("");
                // TestReadSuccessResponse();
                // IMessageReader reader = new DefaultMessageReader();
                // TestReadMessages(reader);
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

            Console.ReadKey();                
        }
         
        public static void TestFileNaming()
        {

            GovTalkMessageFileName filename = (new GovTalkMessageFileName.FileNameBuilder()
            .AddFilePath(@"")
            .AddEnvironment("Test")
            .AddMessageIntention("RequestMessage")
            .AddTimestamp(DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss", System.Globalization.CultureInfo.InvariantCulture))
            .AddCustomNamePart("File" + 1)
            .BuildFileName()
            );

            Console.WriteLine(filename.ToString());
        }

        public static void TestLocalProcess()
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
            GovTalkMessageCreator submitMessageCreator = new GovTalkMessageCreator(new SubmitRequestMessageBuilder(loggingService),loggingService);
            submitMessageCreator.CreateGovTalkMessage();
            
            // Get the GovTalkMessage that has been built
            GovTalkMessage submitMessage = submitMessageCreator.GetGovTalkMessage();

            // Serialize the GovTalkMessage to an XmlDocument
            XmlDocument xd = submitMessageCreator.SerializeGovTalkMessage();

            // Set the IRmark for the GovTalkMessage XmlDocument
            XmlDocument finalXd = GovTalkMessageHelpers.SetIRmark(xd);

            // Set the URI to send the file to
            string uri = configurationRepository.GetConfigurationValue<string>("SendURILocal");                                

            // Create a client to send the file to the target gateway
            CharitiesOnline.MessageService.Client client = new MessageService.Client(loggingService);

            // Create an XmlDocument of the reply from the endpoint
            XmlDocument reply = client.SendRequest(xd, uri);           

            // Set up a message reading strategy
            IMessageReader _messageReader = new DefaultMessageReader(loggingService, configurationRepository, reply.ToXDocument());

            string[] results = _messageReader.ReadMessage<string[]>();

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
            .AddCorrelationId(results[correlationIdPosition].Substring(results[correlationIdPosition].IndexOf("::")+2))
            .AddMessageQualifier(results[qualifierPosition].Substring(results[qualifierPosition].IndexOf("::")+2)) //could check for < 0 here and pass empty string
            .BuildFileName();

            string filename = fileNamer.ToString();

            reply.Save(filename);
            
            // reply.Save(@"C:\Temp\localreply.xml");
            
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

            XmlDocument messageXML = new XmlDocument();
            messageXML.Load(@"C:\Temp\RequestMessage_1423572802_File187E1E8A7F16147A6B87962E07933B406_acknowledgement_20150210125836_.xml");

            string[] results = _messageReader.ReadMessage<string[]>();

            foreach(var s in results)
            {
                Console.WriteLine(s);
            }

            // GovTalkMessage message = _messageReader.Message(messageXML.ToXDocument());

            string bodytype = _messageReader.GetBodyType();

            Console.WriteLine(bodytype);

            ErrorResponse err = _messageReader.GetBody<ErrorResponse>();

        }

        static void TestSend()
        {
            IConfigurationRepository configurationRepository = new ConfigFileConfigurationRepository();
            ILoggingService loggingService = new Log4NetLoggingService(configurationRepository, new ThreadContextService());

            string uri = configurationRepository.GetConfigurationValue<string>("SendURILocal");

            XmlDocument xd = new XmlDocument();
            xd.PreserveWhitespace = true;
            xd.Load(@"C:\Temp\TestCompressedGovTalkMsgWithIrMark_2015_07_07_12_32_12.xml");

            CharitiesOnline.MessageService.Client client = new MessageService.Client(loggingService);

            XmlDocument reply = client.SendRequest(xd, uri);

            Console.WriteLine(reply.InnerXml);            
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

        public static void TestGovTalkMessageCreation(string SourceDataFileName)
        {           
            ReferenceDataManager.SetSource(ReferenceDataManager.SourceTypes.ConfigFile);

            IConfigurationRepository configurationRepository = new ConfigFileConfigurationRepository();
            ILoggingService loggingService = new Log4NetLoggingService(configurationRepository, new ThreadContextService());

            DataTableRepaymentPopulater.SetLogger(loggingService);
            
            if(!string.IsNullOrEmpty(SourceDataFileName))
                DataTableRepaymentPopulater.GiftAidDonations = DataHelpers.GetDataTableFromCsv(@SourceDataFileName, true);

            GovTalkMessageCreator submitMessageCreator = new GovTalkMessageCreator(new SubmitRequestMessageBuilder(loggingService), loggingService);
            
            submitMessageCreator.CreateGovTalkMessage();

            hmrcclasses.GovTalkMessage submitMessage = submitMessageCreator.GetGovTalkMessage();

            XmlDocument xd = submitMessageCreator.SerializeGovTalkMessage();         
            
            byte[] xmlDocumentSize = xd.XmlToBytes();

            Console.WriteLine("The document is {0} bytes big.", xmlDocumentSize.Length);

            XmlDocument outputXmlDocument;

            if(xmlDocumentSize.Length > 1000000)
            {
                XmlDocument compressedVersion = submitMessageCreator.CompressClaim();
                outputXmlDocument = GovTalkMessageHelpers.SetIRmark(compressedVersion);
            }
            else
            {
                outputXmlDocument = GovTalkMessageHelpers.SetIRmark(xd);
            }                                  

            outputXmlDocument.Save(@"C:\Temp\testGovTalkMsgCompressedWithIrMark" + DateTime.Now.ToString("_yyyy_MM_dd_HH_mm_ss", System.Globalization.CultureInfo.InvariantCulture) + ".xml");

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

            XmlDocument finalXd = GovTalkMessageHelpers.SetIRmark(xd);

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

            XmlDocument finalXd = GovTalkMessageHelpers.SetIRmark(xd);

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

            XmlDocument finalXd = GovTalkMessageHelpers.SetIRmark(xd);

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

            string decompressed = CommonUtilityHelpers.DecompressData(compressedPart.Value);
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
    }
}
