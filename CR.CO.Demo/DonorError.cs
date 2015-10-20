using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CR.CO.Demo
{
    public class DonorError
    {
        public string Forename { get; set; }
        public string Surname { get; set; }
        public string Address1 { get; set; }
        public string Postcode { get; set; }
        public string DonationDate { get; set; }
        public string DonationAmount { get; set; }
        public string ErrorField { get; set; }

        public override string ToString()
        {
            return "FORENAME=" + Forename + "&SURNAME=" + Surname + "&HOUSE=" + Address1 + "&POSTCODE=" + Postcode + "&DONATIONDATE=" + DonationDate + "&DONATIONAMOUNT=" + DonationAmount;
        }

        public string GavinSearchQuery()
        {
            string QueryString = "MATCH_GAFIRSTNAME=" + System.Web.HttpUtility.UrlEncode(Forename) + "&MATCH_GALASTNAME=" + System.Web.HttpUtility.UrlEncode(Surname) + "&MATCH_postcode=" + System.Web.HttpUtility.UrlEncode(Postcode) + "&SPECIAL_GiftAidAmount=" + System.Web.HttpUtility.UrlEncode(DonationAmount);

            return QueryString;
        }

        public Dictionary<string, string> GavinSearchTerms()
        {
            //removed URL encoding as double-encoding was taking place upon form submit
            // Might need to create an encoded and a non-encoded version of this method

            Dictionary<string, string> ReturnList = new Dictionary<string, string>();
            //ReturnList.Add("MATCH_GAFIRSTNAME", System.Web.HttpUtility.UrlEncode(Forename));
            //ReturnList.Add("MATCH_GALASTNAME", System.Web.HttpUtility.UrlEncode(Surname));
            //ReturnList.Add("MATCH_postcode", System.Web.HttpUtility.UrlEncode(Postcode));
            ReturnList.Add("MATCH_GAFIRSTNAME", Forename);
            ReturnList.Add("MATCH_GALASTNAME", Surname);
            ReturnList.Add("MATCH_postcode", Postcode);
            ReturnList.Add("SPECIAL_GiftAidAmount", System.Web.HttpUtility.UrlEncode(DonationAmount));

            return ReturnList;
        }

    }
}
