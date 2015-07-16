using System;
using System.Xml.Linq;
using System.Data;
using System.Xml;
using System.Linq;

using hmrcclasses;

namespace CharitiesOnline.Strategies
{
    public class ReadSubmitRequestStrategy : IMessageReadStrategy
    {
        // private IMessageWriter _messageType;

        private GovTalkMessage _message;
        private IRenvelope _body;

        public bool IsMatch(XDocument inMessage)
        {
            XNamespace ns = "http://www.govtalk.gov.uk/CM/envelope";

            string qualifier = inMessage.Descendants(ns + "Qualifier").FirstOrDefault().Value;
            string function = inMessage.Descendants(ns + "Function").FirstOrDefault().Value;

            if (qualifier == "request" && function == "submit")
            {
                _message = Helpers.DeserializeMessage(inMessage.ToXmlDocument());
                
                XmlElement xmlElement = _message.Body.Any[0];
                
                XmlDocument bodyDoc = new XmlDocument();
                bodyDoc.LoadXml(xmlElement.OuterXml);
                
                _body = Helpers.DeserializeIRenvelope(bodyDoc);

                return true;
            }
                                       
            return false;
        }

        // read message into datatable of donations ?

        // what do we want to do when we read in a Submit Request - recreate the input datatables (donations & otherincome)
        // list of the reference values

        public T ReadMessage<T>(XDocument inXD)
        {
            if (typeof(T) == typeof(DataTable))
            {
                DataTable dt = Helpers.MakeRepaymentTable();                              

                // deal with compression first then load uncompressed repayment & otherinc 

                R68Claim r68claim;

                if(_body.R68.Items[0] is R68CompressedPart)
                {
                    R68CompressedPart compressedPart = (R68CompressedPart)_body.R68.Items[0];

                    string decompressedData = Helpers.DecompressData(compressedPart.Value);
                    XmlDocument decompressedXml = new XmlDocument();              
                    decompressedXml.LoadXml(decompressedData);

                    // decompressedXml.DocumentElement.SetAttribute("xmlns", "http://www.govtalk.gov.uk/taxation/charities/r68/2");

                    XmlDocument r68ClaimXmlDoc = new XmlDocument();
                    XmlElement r68root = r68ClaimXmlDoc.CreateElement("R68Claim");
                    r68root.SetAttribute("xmlns", "http://www.govtalk.gov.uk/taxation/charities/r68/2");
                    r68ClaimXmlDoc.AppendChild(r68root);
                    r68root.InnerXml = decompressedXml.DocumentElement.InnerXml;
                    
                    r68claim = Helpers.Deserialize <R68Claim> (r68ClaimXmlDoc.OuterXml, "R68Claim");                                                  
                }
                else
                {
                    r68claim = (R68Claim)_body.R68.Items[0];
                }

                #region GiftAidDonors

                foreach (var gad in r68claim.Repayment.GAD)
                {
                    // @TODO: Sponsorship indicator

                    DataRow dr;
                    dr = dt.NewRow();

                    if (gad.Item is hmrcclasses.R68ClaimRepaymentGADDonor)
                    {
                        hmrcclasses.R68ClaimRepaymentGADDonor donor = (hmrcclasses.R68ClaimRepaymentGADDonor)gad.Item;

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

                    dt.Rows.Add(dr);
                }
                #endregion GiftAidDonors

                foreach(var otherInc in r68claim.Repayment.OtherInc)
                {

                }

                return (T)Convert.ChangeType(dt, typeof(T));
            }
            else
            {
                // log this

                return default(T);
            }                            
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

    }
}
