using System;
using System.Xml.Linq;
using System.Data;
using System.Xml;
using System.Linq;

using hmrcclasses;
using CharitiesOnline.Helpers;
using CR.Infrastructure.Logging;

namespace CharitiesOnline.Strategies
{
    public class ReadSubmitRequestStrategy : IMessageReadStrategy
    {
        // private IMessageWriter _messageType;

        private GovTalkMessage _message;
        private IRenvelope _body;
        private ILoggingService _loggingService;

        public ReadSubmitRequestStrategy(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        public bool IsMatch(XDocument inMessage)
        {
            XNamespace ns = "http://www.govtalk.gov.uk/CM/envelope";

            string qualifier = inMessage.Descendants(ns + "Qualifier").FirstOrDefault().Value;
            string function = inMessage.Descendants(ns + "Function").FirstOrDefault().Value;

            if (qualifier == "request" && function == "submit")
            {
                return true;
            }
                                       
            return false;
        }

        // read message into datatable of donations ?

        // what do we want to do when we read in a Submit Request - recreate the input datatables (donations & otherincome)
        // list of the reference values

        public void ReadMessage(XDocument inXD)
        {
            _message = XmlSerializationHelpers.DeserializeMessage(inXD.ToXmlDocument());

            XmlElement xmlElement = _message.Body.Any[0];

            XmlDocument bodyDoc = new XmlDocument();
            bodyDoc.LoadXml(xmlElement.OuterXml);

            _body = XmlSerializationHelpers.DeserializeIRenvelope(bodyDoc);

            _loggingService.LogInfo(this, "Message read. Message is SubmitRequest.");        
        }

        public T GetMessageResults<T>()
        {
            R68Claim r68claim = GetClaim(_body.R68.Items);

            if (typeof(T) == typeof(DataTable))
            {
                DataTable dt = GetDataTableGiftAidDonations(r68claim.Repayment.GAD);

                return (T)Convert.ChangeType(dt, typeof(T));
            }
            if (typeof(T) == typeof(DataSet))
            {
                // Get GiftAidDonations
                DataTable dataTableGiftAidDonations = GetDataTableGiftAidDonations(r68claim.Repayment.GAD);

                // Get OtherIncome
                DataTable dataTableOtherIncome = GetDataTableOtherInc(r68claim.Repayment.OtherInc);

                DataSet ds = new DataSet();
                ds.Tables.Add(dataTableGiftAidDonations);
                ds.Tables.Add(dataTableOtherIncome);

                return (T)Convert.ChangeType(ds, typeof(T));
            }
            if (typeof(T) == typeof(string))
            {
                string messageForUser = "This is a Submit Request Message.";

                return (T)Convert.ChangeType(messageForUser, typeof(T));
            }

            _loggingService.LogWarning(this, "No valid type specified for ReadSubmitRequest. Returning default (probably null).");

            return default(T); 
        }
        public GovTalkMessage Message()
        {
            return _message;
        }

        public T GetBody<T>()
        {
            //return (IBodyReturnType)_body;

            return (T)Convert.ChangeType(_body, typeof(T));
        }

        public string GetBodyType()
        {
            // return Type of _body
            return _body.GetType().ToString();
        }

        private R68Claim GetClaim(object[] R68Items)
        {
            if(R68Items[0] is R68CompressedPart)
            {
                R68CompressedPart compressedPart = (R68CompressedPart)_body.R68.Items[0];

                string decompressedData = CommonUtilityHelpers.DecompressData(compressedPart.Value);
                XmlDocument decompressedXml = new XmlDocument();
                decompressedXml.LoadXml(decompressedData);

                // decompressedXml.DocumentElement.SetAttribute("xmlns", "http://www.govtalk.gov.uk/taxation/charities/r68/2");

                XmlDocument r68ClaimXmlDoc = new XmlDocument();
                XmlElement r68root = r68ClaimXmlDoc.CreateElement("R68Claim");
                r68root.SetAttribute("xmlns", "http://www.govtalk.gov.uk/taxation/charities/r68/2");
                r68ClaimXmlDoc.AppendChild(r68root);
                r68root.InnerXml = decompressedXml.DocumentElement.InnerXml;

                return XmlSerializationHelpers.Deserialize<R68Claim>(r68ClaimXmlDoc.OuterXml, "R68Claim"); 
            }
            else
            {
                return (R68Claim)R68Items[0];
            }
        }

        private DataTable GetDataTableGiftAidDonations(R68ClaimRepaymentGAD[] giftAidDonations)
        {
            DataTable dt = DataHelpers.MakeRepaymentTable();     

            foreach (var gad in giftAidDonations)  //r68claim.Repayment.GAD
            {
                DataRow dr;
                dr = dt.NewRow();

                if (gad.Item is R68ClaimRepaymentGADDonor)
                {
                    R68ClaimRepaymentGADDonor donor = (R68ClaimRepaymentGADDonor)gad.Item;

                    dr["Fore"] = donor.Fore;
                    dr["Sur"] = donor.Sur;
                    dr["House"] = donor.House;
                    if (donor.Item is string)
                    {
                        dr["Postcode"] = donor.Item;
                    }
                    if (donor.Item is hmrcclasses.r68_YesType)
                    {
                        dr["Overseas"] = "yes";
                    }
                    dr["Type"] = "GAD";
                }
                else if (gad.Item is string)
                {
                    // agg donation
                    dr["Type"] = "AGG";
                    dr["Description"] = gad.Item;
                }

                dr["Total"] = gad.Total;
                dr["Date"] = gad.Date;

                //sponsorship flag // @TODO: TEST Sponsorship indicator
                if (gad.SponsoredSpecified)
                    dr["Sponsored"] = gad.Sponsored == r68_YesType.yes ? "Y" : "N";

                dt.Rows.Add(dr);
            }

            return dt;
        }

        private DataTable GetDataTableOtherInc(R68ClaimRepaymentOtherInc[] otherInc)
        {
            DataTable dt = DataHelpers.MakeOtherIncomeTable();
            
            foreach (var otherincome in otherInc)
            {
                DataRow dr = dt.NewRow();
                dr["Payer"] = otherincome.Payer;
                dr["OIDate"] = otherincome.OIDate;
                dr["Gross"] = otherincome.Gross;
                dr["Tax"] = otherincome.Tax;
            }

            return dt;
        }

        public string GetCorrelationId()
        {
            return String.Empty;
        }

        public bool HasErrors()
        {
            return false;
        }
    }
}
