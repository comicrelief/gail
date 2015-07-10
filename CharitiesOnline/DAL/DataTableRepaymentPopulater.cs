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

        public static DataTable GiftAidDonations
        {
            get
            {
                return _giftAidDonations;
            }
            set
            {
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

        public static hmrcclasses.R68ClaimRepayment CreateRepayments()
        {
            R68ClaimRepaymentGAD[] GADs = new R68ClaimRepaymentGAD[GiftAidDonations.Rows.Count];

            // throw exception if no column named Type
            // or use Donor method if no column named Type exists

            for (int i = 0; i < GiftAidDonations.Rows.Count; i++)
            {
                if (GiftAidDonations.Columns.Contains("Type"))
                {
                    if (GiftAidDonations.Rows[i]["Type"].ToString() == "Agg")
                    {
                        R68ClaimRepaymentGADCreator aggDonationCreator = new R68ClaimRepaymentGADCreator(new AggDonationR68ClaimRepaymentGADBuilder());
                        aggDonationCreator.SetInputRow(GiftAidDonations.Rows[i]);
                        aggDonationCreator.CreateR68ClaimRepaymentGAD();
                        GADs[i] = aggDonationCreator.GetR68ClaimRepaymentGAD();
                    }
                    if (GiftAidDonations.Rows[i]["Type"].ToString() == "GAD")
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
