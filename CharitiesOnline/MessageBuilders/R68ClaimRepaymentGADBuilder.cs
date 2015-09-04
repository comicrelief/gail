using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;

using hmrcclasses;
using CR.Infrastructure.Logging;

namespace CharitiesOnline.MessageBuilders
{
    public abstract class R68ClaimRepaymentGADBuilderBase
    {
        private R68ClaimRepaymentGAD _r68ClaimRepaymentGAD;
        private ILoggingService _loggingService;
        private DataRow _inputDataRow;        

        public DataRow InputDataRow
        {
            get
            {
                return _inputDataRow;
            }
            set
            {
                _inputDataRow = value;
            }
        }

        public R68ClaimRepaymentGAD R68ClaimRepaymentGAD
        {
            get
            {
                return _r68ClaimRepaymentGAD;
            }
        }

        public void InitialiseR68ClaimRepaymentGAD(ILoggingService loggingService)
        {
            _r68ClaimRepaymentGAD = new R68ClaimRepaymentGAD();
            _loggingService = loggingService;
        }

        public abstract void SetDonationDetails();
        public abstract void SetDonation();
    }

    public class R68ClaimRepaymentGADCreator
    {
        private R68ClaimRepaymentGADBuilderBase _r68ClaimRepaymentGADBuilder;
        private ILoggingService _loggingService;

        public R68ClaimRepaymentGADCreator(R68ClaimRepaymentGADBuilderBase r68ClaimRepaymentGADBuilder, ILoggingService loggingService)
        {
            _r68ClaimRepaymentGADBuilder = r68ClaimRepaymentGADBuilder;
            _loggingService = loggingService;
        }

        public void CreateR68ClaimRepaymentGAD()
        {
            _r68ClaimRepaymentGADBuilder.InitialiseR68ClaimRepaymentGAD(_loggingService);
            _r68ClaimRepaymentGADBuilder.SetDonationDetails();
            _r68ClaimRepaymentGADBuilder.SetDonation();
        }

        public hmrcclasses.R68ClaimRepaymentGAD GetR68ClaimRepaymentGAD()
        {
            return _r68ClaimRepaymentGADBuilder.R68ClaimRepaymentGAD;
        }

        public void SetInputRow(DataRow inputRow)
        {           
            _r68ClaimRepaymentGADBuilder.InputDataRow = inputRow;
        }
    }

    public class DefaultR68ClaimRepaymentGADBuilder : R68ClaimRepaymentGADBuilderBase
    {
        private ILoggingService _loggingService;
        public DefaultR68ClaimRepaymentGADBuilder(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }
        public void CreateR68ClaimRepaymentGAD()
        {
            InitialiseR68ClaimRepaymentGAD(_loggingService);
            SetDonationDetails();
        }

        public override void SetDonationDetails()
        {
            DateTime donationDate;
            if (DateTime.TryParse(InputDataRow["Date"].ToString(), out donationDate))
            {
                R68ClaimRepaymentGAD.Date = donationDate;
            }
            else
            {
                throw new FormatException(String.Concat("Donation Date format incorrect for record ", InputDataRow.Table.Rows.IndexOf(InputDataRow) + 1));
            }
            
            Decimal donationAmount;
            if(Decimal.TryParse(InputDataRow["Total"].ToString(), out donationAmount))
            {
                R68ClaimRepaymentGAD.Total = donationAmount;
            }
            else
            {
                throw new FormatException(String.Concat("Donation Amount format incorrect for record ", InputDataRow.Table.Rows.IndexOf(InputDataRow) + 1));
            }

            R68ClaimRepaymentGAD.Total = Convert.ToDecimal(InputDataRow["Total"]);

            if (InputDataRow.Table.Columns.Contains("Sponsored"))
            {
                if (InputDataRow["Sponsored"].ToString().ToUpper() == "YES" || InputDataRow["Sponsored"].ToString().ToUpper() == "Y")
                {
                    R68ClaimRepaymentGAD.SponsoredSpecified = true;
                    R68ClaimRepaymentGAD.Sponsored = r68_YesType.yes;
                }
            }                                               
        }

        public override void SetDonation()
        {
            throw new NotImplementedException();
        }

    }

    public class DonorR68ClaimRepaymentGADBuilder : DefaultR68ClaimRepaymentGADBuilder
    {
        private ILoggingService _loggingService;

        public DonorR68ClaimRepaymentGADBuilder(ILoggingService loggingService) : base(loggingService)
        {
            _loggingService = loggingService;
        }
        public new void CreateR68ClaimRepaymentGAD()
        {
            SetDonation();
        }

        public override void SetDonation()
        {
            R68ClaimRepaymentGADDonor donor = new R68ClaimRepaymentGADDonor();

            if (InputDataRow.Table.Columns.Contains("Title") &&
                            InputDataRow["Title"].ToString().Length > 0)
                donor.Ttl = InputDataRow["Title"].ToString();
                donor.Fore = InputDataRow["Fore"].ToString();
                donor.Sur = InputDataRow["Sur"].ToString();
                donor.House = InputDataRow["house"].ToString().Length > 40 ? InputDataRow["house"].ToString().Substring(0, 40) : InputDataRow["house"].ToString();
            if (InputDataRow["Postcode"].ToString().Length > 0)
            {
                donor.Item = InputDataRow["Postcode"].ToString();
            }
            else
            {
                donor.Item = new r68_YesType();
            }

            R68ClaimRepaymentGAD.Item = donor;
        }
    }

    public class AggDonationR68ClaimRepaymentGADBuilder : DefaultR68ClaimRepaymentGADBuilder
    {
        private ILoggingService _loggingService;
        public AggDonationR68ClaimRepaymentGADBuilder(ILoggingService loggingService) : base(loggingService)
        {
            _loggingService = loggingService;
        }
        #region old
        //private DataRow _inputDataRow;
        //public new DataRow InputDataRow
        //{
        //    get
        //    {
        //        return _inputDataRow;
        //    }
        //    set
        //    {
        //        _inputDataRow = value;
        //    }
        //}
        #endregion old
        public new void CreateR68ClaimRepaymentGAD()
        {
            SetDonation();
        }

        public override void SetDonation()
        {
            R68ClaimRepaymentGAD.Item = InputDataRow["Description"].ToString();
            //base.SetAggDonation();
        }
    }
}
