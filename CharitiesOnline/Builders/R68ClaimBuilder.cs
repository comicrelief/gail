using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using hmrcclasses;
using CR.Infrastructure.Logging;

namespace CharitiesOnline.Builders
{
    public abstract class R68ClaimBuilderBase
    {
        private R68Claim _r68Claim;
        private ILoggingService _loggingService;

        public R68Claim R68Claim
        {
            get
            {
                return _r68Claim;
            }
        }

        public void InitialiseR68Claim(ILoggingService loggingService)
        {
            _r68Claim = new R68Claim();
            _loggingService = loggingService;
        }

        public abstract void SetClaimDetails();

        public abstract void SetGASDS();
        public abstract void SetRepayment();
    }

    public class R68ClaimCreator
    {
        private R68ClaimBuilderBase _r68ClaimBuilder;
        private ILoggingService _loggingService;

        public R68ClaimCreator(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }
        public R68ClaimCreator(R68ClaimBuilderBase r68ClaimBuilder)
        {
            _r68ClaimBuilder = r68ClaimBuilder;
        }
        public void CreateR68Claim()
        {
            _r68ClaimBuilder.InitialiseR68Claim(_loggingService);
            _r68ClaimBuilder.SetClaimDetails();
            _r68ClaimBuilder.SetGASDS();
            _r68ClaimBuilder.SetRepayment();
        }

        public R68Claim GetR68Claim()
        {
            return _r68ClaimBuilder.R68Claim;
        }
    }

    public class DefaultR68ClaimBuilder : R68ClaimBuilderBase
    {
        private ILoggingService _loggingService;

        public DefaultR68ClaimBuilder(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }
        public void CreateR68Claim()
        {
            InitialiseR68Claim(_loggingService);
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
        private ILoggingService _loggingService;

        public RepaymentBuilder(ILoggingService loggingService) : base(loggingService)
        {
            _loggingService = loggingService;
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
        private ILoggingService _loggingService;
        public ConfigFileRepaymentBuilder(ILoggingService loggingService) : base(loggingService)
        {
            _loggingService = loggingService;
        }
        public override void SetRepayment()
        {
            base.SetRepayment();
        }
    }

    // What about a DataTableRepaymentBuilder from a DataTableR68ClaimBuilder
    // public class DataTableRepaymentBuilder : DataTableR68ClaimBuilder 
    // ??
    

}
