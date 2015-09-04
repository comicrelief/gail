using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using hmrcclasses;
using CharitiesOnline.MessageBuilders;
using CR.Infrastructure.Logging;

namespace CharitiesOnline
{
    /// <summary>
    /// A mechanism to get donations data in to a submission message. Static so that it can be called
    /// from within the message builder classes without being injected through constructors. To use, first initialise a logger via
    /// SetLogger then assign a datatable via the static setter.
    /// </summary>
    public static class DataTableRepaymentPopulater
    {
        private static DataTable _giftAidDonations;
        private static DataTable _otherIncome;
        private static readonly List<string> RequiredColumnNames = new List<string>(new string[] { "Fore", "Sur", "House", "Postcode","Date","Total" });
        private static ILoggingService _loggingService;

        public static void SetLogger(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }       

        public static DataTable GiftAidDonations
        {
            get
            {
                return _giftAidDonations;
            }
            set
            {
                if (_loggingService == null)
                    throw new ArgumentNullException("LoggingService");

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

        public static R68ClaimRepayment CreateRepayments()
        {

            if(GiftAidDonations != null)
            {
                R68ClaimRepaymentGAD[] GADs = new R68ClaimRepaymentGAD[GiftAidDonations.Rows.Count];

                _loggingService.LogInfo(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
                    , String.Concat("Processing ",GiftAidDonations.Rows.Count," donation records."));

                for (int i = 0; i < GiftAidDonations.Rows.Count; i++)
                {
                    if (GiftAidDonations.Columns.Contains("Type"))
                    {
                        if (GiftAidDonations.Rows[i]["Type"].ToString().ToUpper() == "AGG")
                        {
                            R68ClaimRepaymentGADCreator aggDonationCreator = new R68ClaimRepaymentGADCreator(new AggDonationR68ClaimRepaymentGADBuilder(_loggingService), _loggingService);
                            aggDonationCreator.SetInputRow(GiftAidDonations.Rows[i]);
                            aggDonationCreator.CreateR68ClaimRepaymentGAD();
                            GADs[i] = aggDonationCreator.GetR68ClaimRepaymentGAD();
                        }
                        if (GiftAidDonations.Rows[i]["Type"].ToString().ToUpper() == "GAD")
                        {
                            R68ClaimRepaymentGADCreator donorCreator = new R68ClaimRepaymentGADCreator(new DonorR68ClaimRepaymentGADBuilder(_loggingService), _loggingService);
                            donorCreator.SetInputRow(GiftAidDonations.Rows[i]);
                            donorCreator.CreateR68ClaimRepaymentGAD();
                            GADs[i] = donorCreator.GetR68ClaimRepaymentGAD();
                        }
                    }
                    else
                    {
                        R68ClaimRepaymentGADCreator donorCreator = new R68ClaimRepaymentGADCreator(new DonorR68ClaimRepaymentGADBuilder(_loggingService), _loggingService);
                        donorCreator.SetInputRow(GiftAidDonations.Rows[i]);
                        donorCreator.CreateR68ClaimRepaymentGAD();
                        GADs[i] = donorCreator.GetR68ClaimRepaymentGAD();
                    }
                }

                var repayment = new R68ClaimRepayment();
                repayment.GAD = GADs;
                repayment.EarliestGAdate = Convert.ToDateTime(GiftAidDonations.Compute("min(Date)", string.Empty));
                repayment.EarliestGAdateSpecified = true;

                _loggingService.LogInfo(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
                    , "ClaimRepayment object created.");

                return repayment;
            }
            _loggingService.LogWarning(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, "No records for GiftAid repayment");
            return null;
           
        }

        public static R68ClaimRepaymentOtherInc[] CreateOtherIncome()
        {
            R68ClaimRepaymentOtherInc[] OtherIncs = new R68ClaimRepaymentOtherInc[OtherIncome.Rows.Count];

            _loggingService.LogInfo(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
                , String.Concat("Processing ", OtherIncome.Rows.Count, " OtherIncome records."));

            for (int i = 0; i < OtherIncome.Rows.Count; i++)
            {
                R68ClaimRepaymentOtherIncomeCreator otherIncCreator = new R68ClaimRepaymentOtherIncomeCreator(new DefaultR68ClaimRepaymentOtherIncomeBuilder(_loggingService), _loggingService);
                otherIncCreator.SetInputRow(OtherIncome.Rows[i]);
                otherIncCreator.CreateR68ClaiMRepaymentOtherInc();
                OtherIncs[i] = otherIncCreator.GetR68ClaimRepaymentOtherInc();
            }

            return OtherIncs;
        }
    }
}
