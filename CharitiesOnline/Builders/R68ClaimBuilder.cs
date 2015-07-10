using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;

using System.Configuration;

using hmrcclasses;

namespace CharitiesOnline.Builders
{
    public abstract class R68ClaimBuilderBase
    {
        private hmrcclasses.R68Claim _r68Claim;
        // datastore here, multiple ways of getting the base data in

        public hmrcclasses.R68Claim R68Claim
        {
            get
            {
                return _r68Claim;
            }
        }

        public void InitialiseR68Claim()
        {
            _r68Claim = new hmrcclasses.R68Claim();
        }

        public abstract void SetClaimDetails();
        //public void SetRepayment(DataTable InputDataTable)
        //{
        //    R68ClaimRepaymentGAD[] GADs = new R68ClaimRepaymentGAD[InputDataTable.Rows.Count];
        //    // throw exception if no column named Type
        //    for(int i = 0; i < InputDataTable.Rows.Count; i++)
        //    {
        //        if(InputDataTable.Rows[i]["Type"].ToString() == "")
        //        {
        //            R68ClaimRepaymentGADCreator aggDonationCreator = new R68ClaimRepaymentGADCreator(new AggDonationR68ClaimRepaymentGADBuilder());
        //            aggDonationCreator.CreateR68ClaimRepaymentGAD();
        //            GADs[i] = aggDonationCreator.GetR68ClaimRepaymentGAD();
        //        }
        //        if(InputDataTable.Rows[i]["Type"].ToString() == "")
        //        {
        //            R68ClaimRepaymentGADCreator donorCreator = new R68ClaimRepaymentGADCreator(new DonorR68ClaimRepaymentGADBuilder());
        //            donorCreator.CreateR68ClaimRepaymentGAD();
        //            GADs[i] = donorCreator.GetR68ClaimRepaymentGAD();
        //        }
        //    }

        //    var repayment = new R68ClaimRepayment();
        //    repayment.GAD = GADs;
        //    repayment.EarliestGAdate = Convert.ToDateTime(InputDataTable.Compute("min(Date)", string.Empty));
        //    repayment.EarliestGAdateSpecified = true;

        //    R68Claim.Repayment = repayment;
        //}
        public abstract void SetGASDS();
        public abstract void SetRepayment();
    }

    public class R68ClaimCreator
    {
        private R68ClaimBuilderBase _r68ClaimBuilder;

        public R68ClaimCreator(R68ClaimBuilderBase r68ClaimBuilder)
        {
            _r68ClaimBuilder = r68ClaimBuilder;
        }
        public void CreateR68Claim()
        {
            _r68ClaimBuilder.InitialiseR68Claim();
            _r68ClaimBuilder.SetClaimDetails();
            _r68ClaimBuilder.SetGASDS();
            _r68ClaimBuilder.SetRepayment();
        }

        public hmrcclasses.R68Claim GetR68Claim()
        {
            return _r68ClaimBuilder.R68Claim;
        }
    }

    public class DefaultR68ClaimBuilder : R68ClaimBuilderBase
    {
        public void CreateR68Claim()
        {
            InitialiseR68Claim();
            SetClaimDetails();
            SetGASDS();
        }
        public override void SetClaimDetails()
        {
            R68Claim.HMRCref = ReferenceDataManager.Settings["R68ClaimHMRCRef"];
            R68Claim.OrgName = ReferenceDataManager.Settings["R68ClaimOrgName"];

            var regulator = new R68ClaimRegulator();
            switch (ReferenceDataManager.Settings["R68RegulatorName"])
            {
                case "CCEW":
                    regulator.Item = R68ClaimRegulatorRegName.CCEW;
                    break;
                case "CCNI":
                    regulator.Item = R68ClaimRegulatorRegName.CCNI;
                    break;
                case "OSCR":
                    regulator.Item = R68ClaimRegulatorRegName.OSCR;
                    break;
            }
            regulator.RegNo = ReferenceDataManager.Settings["R68RegulatorNumber"];

            R68Claim.Regulator = regulator;

        }
        public override void SetGASDS()
        {
            var gasds = new R68ClaimGASDS();
            gasds.ConnectedCharities = ReferenceDataManager.Settings["GASDSConnectedCharities"] == "yes" ? r68_YesNoType.yes : r68_YesNoType.no;
            gasds.CommBldgs = ReferenceDataManager.Settings["GASDSCommBldgs"] == "yes" ? r68_YesNoType.yes : r68_YesNoType.no;

            R68Claim.GASDS = gasds;
        }
        public override void SetRepayment()
        {
            throw new NotImplementedException();
        }
    }

    public class RepaymentBuilder : DefaultR68ClaimBuilder
    {
        private DataTable _donationsDataTable;
        public DataTable DonationsDataTable
        {
            get
            {
                return _donationsDataTable;
            }
            set
            {
                _donationsDataTable = value;
            }
        }
        
        private DataTable _otherIncomeDataTable;
        public DataTable OtherIncomeDataTable
        {
            get
            {
                return _otherIncomeDataTable;
            }
            set
            {
                _otherIncomeDataTable = value;
            }
        }
        public override void SetRepayment()
        {
            R68Claim.Repayment = DataTableRepaymentPopulater.CreateRepayments();

            if (DataTableRepaymentPopulater.OtherIncome != null && DataTableRepaymentPopulater.OtherIncome.Rows.Count > 0)
            {
                R68Claim.Repayment.OtherInc = DataTableRepaymentPopulater.CreateOtherIncome();
            }            
        }
    }

    public class ConfigFileRepaymentBuilder : DefaultR68ClaimBuilder
    {
        public override void SetRepayment()
        {
            base.SetRepayment();
        }
    }

    // What about a DataTableRepaymentBuilder from a DataTableR68ClaimBuilder
    // public class DataTableRepaymentBuilder : DataTableR68ClaimBuilder 
    // ??
    

}
