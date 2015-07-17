using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Configuration;

using hmrcclasses;
using CharitiesOnline.Builders;

namespace CharitiesOnline
{
    public static class DataTableRepaymentPopulater
    {
        private static DataTable _giftAidDonations;
        private static DataTable _otherIncome;
        private static readonly List<string> RequiredColumnNames = new List<string>(new string[] { "Fore", "Sur", "House", "Postcode","Date","Total" });

        public static DataTable GiftAidDonations
        {
            get
            {
                return _giftAidDonations;
            }
            set
            {
                string[] MissingColumns = GetMissingColumns(value, RequiredColumnNames);

                if(MissingColumns.Length > 0)
                {
                    string Message = "One or more Required Columns is missing from the input datatable. List of missing columns: \n";
                    
                    foreach(var s in MissingColumns)
                    {
                        Message += s + "\n";
                    }

                    throw new ArgumentException(Message);
                }
                _giftAidDonations = value;
            }
        }

        public static DataTable OtherIncome
        {
            get
            {
                return _otherIncome;
            }
            set
            {
                _otherIncome = value;
            }
        }

        private static string[] GetMissingColumns(DataTable TableToCheck, List<string> RequiredColumnNames)
        {
            List<string> MissingColumns = new List<string>();

            string[] columnList;

            foreach(string colName in RequiredColumnNames)
            {                
                if(!TableToCheck.Columns.Contains(colName))
                {
                    MissingColumns.Add(colName);                    
                }
            }

            columnList = MissingColumns.ToArray();

            return columnList;
        }

        public static hmrcclasses.R68ClaimRepayment CreateRepayments()
        {
            R68ClaimRepaymentGAD[] GADs = new R68ClaimRepaymentGAD[GiftAidDonations.Rows.Count];

            for (int i = 0; i < GiftAidDonations.Rows.Count; i++)
            {
                if (GiftAidDonations.Columns.Contains("Type"))
                {
                    if (GiftAidDonations.Rows[i]["Type"].ToString().ToUpper() == "AGG")
                    {
                        R68ClaimRepaymentGADCreator aggDonationCreator = new R68ClaimRepaymentGADCreator(new AggDonationR68ClaimRepaymentGADBuilder());
                        aggDonationCreator.SetInputRow(GiftAidDonations.Rows[i]);
                        aggDonationCreator.CreateR68ClaimRepaymentGAD();
                        GADs[i] = aggDonationCreator.GetR68ClaimRepaymentGAD();
                    }
                    if (GiftAidDonations.Rows[i]["Type"].ToString().ToUpper() == "GAD")
                    {
                        R68ClaimRepaymentGADCreator donorCreator = new R68ClaimRepaymentGADCreator(new DonorR68ClaimRepaymentGADBuilder());
                        donorCreator.SetInputRow(GiftAidDonations.Rows[i]);
                        donorCreator.CreateR68ClaimRepaymentGAD();
                        GADs[i] = donorCreator.GetR68ClaimRepaymentGAD();
                    }
                }
                else
                {
                    R68ClaimRepaymentGADCreator donorCreator = new R68ClaimRepaymentGADCreator(new DonorR68ClaimRepaymentGADBuilder());
                    donorCreator.SetInputRow(GiftAidDonations.Rows[i]);
                    donorCreator.CreateR68ClaimRepaymentGAD();
                    GADs[i] = donorCreator.GetR68ClaimRepaymentGAD();
                }
            }

            var repayment = new R68ClaimRepayment();
            repayment.GAD = GADs;
            repayment.EarliestGAdate = Convert.ToDateTime(GiftAidDonations.Compute("min(Date)", string.Empty));
            repayment.EarliestGAdateSpecified = true;

            return repayment;
        }

        public static hmrcclasses.R68ClaimRepaymentOtherInc[] CreateOtherIncome()
        {
            hmrcclasses.R68ClaimRepaymentOtherInc[] OtherIncs = new R68ClaimRepaymentOtherInc[OtherIncome.Rows.Count];

            for (int i = 0; i < OtherIncome.Rows.Count; i++)
            {
                R68ClaimRepaymentOtherIncomeCreator otherIncCreator = new R68ClaimRepaymentOtherIncomeCreator(new DefaultR68ClaimRepaymentOtherIncomeBuilder());
                otherIncCreator.SetInputRow(OtherIncome.Rows[i]);
                otherIncCreator.CreateR68ClaiMRepaymentOtherInc();
                OtherIncs[i] = otherIncCreator.GetR68ClaimRepaymentOtherInc();
            }

            return OtherIncs;
        }
    }
}
