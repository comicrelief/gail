using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Data;

using System.Configuration;

using System.Xml;
using System.Xml.Linq;

using hmrcclasses;
using CharitiesOnlineWorkings.Builders;
using CharitiesOnlineWorkings.Strategies;

namespace CharitiesOnlineWorkings
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Started");
            // Console.ReadKey();
            try
            {
                TestSend();
            }
            catch(System.Net.WebException wex)
            {
                Console.WriteLine("Exception occured in connecting to remote machine");
                Console.WriteLine(wex.InnerException.Message);
                Console.WriteLine(wex.Message);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
                                    
            Console.ReadKey();                
        }

        static void TestSend()
        {
            string uri = ConfigurationManager.AppSettings.Get("SendURILocal");

            XmlDocument xd = new XmlDocument();
            xd.PreserveWhitespace = true;
            xd.Load(@"C:\Temp\TestCompressedGovTalkMsgWithIrMark_2015_07_07_12_32_12.xml");

            CharitiesOnlineWorkings.MessageService.Client client = new MessageService.Client();

            XmlDocument reply = client.SendRequest(xd, uri);

            Console.WriteLine(reply.InnerXml);
            

        }

        public static void TestReadMessage()
        {
            // vep200pc
            // XDocument xd = XDocument.Load(@"C:\Temp\Compressed Data Samples\GAValidSample_GZIP.xml");
            XDocument xd = XDocument.Load(@"C:\Temp\GAValidSample_GZIP.xml");

            ReadSubmitRequestStrategy read = new ReadSubmitRequestStrategy();
            read.IsMatch(xd);

            DataTable dt = read.ReadMessage<DataTable>(xd);
            
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

        public static void TestGovTalkMessageCreation()
        {
            DataTableRepaymentPopulater.GiftAidDonations = Helpers.GetDataTableFromCsv(@"C:\enterprise_tfs\GAVIN\CO\test_data\sample2.csv", true);

            ReferenceDataManager.SetSource(ReferenceDataManager.SourceTypes.ConfigFile);

            GovTalkMessageCreator submitMessageCreator = new GovTalkMessageCreator(new SubmitRequestMessageBuilder());
            submitMessageCreator.CreateGovTalkMessage();
            hmrcclasses.GovTalkMessage submitMessage = submitMessageCreator.GetGovTalkMessage();

            XmlDocument xd = submitMessageCreator.SerializeGovTalkMessage();           

            XmlDocument finalXd = Helpers.SetIRmark(xd);

            finalXd.Save(@"C:\Temp\testGovTalkMsgWithIrMark" + DateTime.Now.ToString("_yyyy_MM_dd_HH_mm_ss", System.Globalization.CultureInfo.InvariantCulture) + ".xml");

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
            DataTableRepaymentPopulater.GiftAidDonations = Helpers.GetDataTableFromCsv(@"C:\Temp\Donations.csv", true);
            DataTableRepaymentPopulater.OtherIncome = Helpers.GetDataTableFromCsv(@"C:\Temp\OtherInc.csv", true);

            ReferenceDataManager.SetSource(ReferenceDataManager.SourceTypes.ConfigFile);

            GovTalkMessageCreator submitMessageCreator = new GovTalkMessageCreator(new SubmitRequestMessageBuilder());
            submitMessageCreator.CreateGovTalkMessage();
            hmrcclasses.GovTalkMessage submitMessage = submitMessageCreator.GetGovTalkMessage();

            XmlDocument xd = submitMessageCreator.SerializeGovTalkMessage();

            XmlDocument finalXd = Helpers.SetIRmark(xd);

            finalXd.Save(@"C:\Temp\TestGovTalkMsgWithOtherIncome" + DateTime.Now.ToString("_yyyy_MM_dd_HH_mm_ss", System.Globalization.CultureInfo.InvariantCulture) + ".xml");

        }

        public static void TestGovTalkCompressedMessageCreation()
        {
            DataTableRepaymentPopulater.GiftAidDonations = Helpers.GetDataTableFromCsv(@"C:\enterprise_tfs\GAVIN\CO\test_data\sample2.csv", true);
            DataTableRepaymentPopulater.OtherIncome = Helpers.GetDataTableFromCsv(@"C:\Temp\OtherInc.csv", true);

            // C:\Temp\Donations.csv

            ReferenceDataManager.SetSource(ReferenceDataManager.SourceTypes.ConfigFile);

            GovTalkMessageCreator compressedSubmissionCreator = new GovTalkMessageCreator(new SubmitRequestCompressedMessageBuilder());
            compressedSubmissionCreator.CreateGovTalkMessage();

            XmlDocument xd = compressedSubmissionCreator.SerializeGovTalkMessage();

            XmlDocument finalXd = Helpers.SetIRmark(xd);

            finalXd.Save(@"C:\Temp\TestCompressedGovTalkMsgWithIrMark" + DateTime.Now.ToString("_yyyy_MM_dd_HH_mm_ss", System.Globalization.CultureInfo.InvariantCulture) + ".xml");

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

            GovTalkMessageCreator submitPollCreator = new GovTalkMessageCreator(new SubmitPollMesageBuilder());
            
            submitPollCreator.SetCorrelationId("52CB8A8AA58148859377830BAE5B99C9");
                        
            submitPollCreator.CreateGovTalkMessage();

            XmlDocument xd = submitPollCreator.SerializeGovTalkMessage();

            xd.Save(@"c:\Temp\pollsubmit-" + DateTime.Now.ToString("_yyyy_MM_dd_HH_mm_ss", System.Globalization.CultureInfo.InvariantCulture) + ".xml");
            
        }

        public static void TestDeleterequest()
        {
            ReferenceDataManager.SetSource(ReferenceDataManager.SourceTypes.ConfigFile);

            GovTalkMessageCreator deleteRequestCreator = new GovTalkMessageCreator(new DeleteRequestMessageBuilder());
            deleteRequestCreator.SetCorrelationId("hjcjc");
            deleteRequestCreator.CreateGovTalkMessage();

            XmlDocument xd = deleteRequestCreator.SerializeGovTalkMessage();
            xd.Save(@"C:\Temp\deleterequest-" + DateTime.Now.ToString("_yyyy_MM_dd_HH_mm_ss", System.Globalization.CultureInfo.InvariantCulture) + ".xml");
        }

        public static void TestListRequest()
        {
            ReferenceDataManager.SetSource(ReferenceDataManager.SourceTypes.ConfigFile);

            GovTalkMessageCreator listRequestCreator = new GovTalkMessageCreator(new ListRequestMessageBuilder());
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

            XmlDocument finalXd = Helpers.SetIRmark(xd);

            finalXd.Save(@"C:\Temp\testGovTalkMsgWithIrMark" + DateTime.Now.ToString("_yyyy_MM_dd_HH_mm_ss", System.Globalization.CultureInfo.InvariantCulture) + ".xml");
        }

        public static void TestDeserialize()
        {
            XmlDocument xd = new XmlDocument();
            xd.Load(@"C:\CharitiesOnline\CharitiesValidSamples_February_2015\Compressed Data Samples\GAValidSample_GZIP.xml");

            GovTalkMessage gtm = Helpers.DeserializeMessage(xd);

            XmlElement xelement = gtm.Body.Any[0];

            // XmlDocument bodyDoc = xelement.OwnerDocument;            
            XmlDocument bodyDoc = new XmlDocument();
            bodyDoc.LoadXml(xelement.OuterXml);

            IRenvelope ire = Helpers.DeserializeIRenvelope(bodyDoc);

            R68CompressedPart compressedPart = (R68CompressedPart)ire.R68.Items[0];

            string decompressed = Helpers.DecompressData(compressedPart.Value);
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
                Helpers.SerializeItem(claim);

            claimXml.Save(@"C:\Temp\R68Claim.xml");
        }

        public static void TestDeserializeClaim()
        {
            XmlDocument claimXml = new XmlDocument();
            claimXml.Load(@"C:\Temp\R68Claim.xml");

            R68Claim r68claim = Helpers.DeserializeR68Claim(claimXml);
                
                //Helpers.Deserialize<R68Claim>(claimXml.OuterXml, "R68Claim");

        }
    }
}
