using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;

using hmrcclasses;

namespace CharitiesOnline.Builders
{
    public abstract class R68ClaimRepaymentGADBuilderBase
    {
        private R68ClaimRepaymentGAD _r68ClaimRepaymentGAD;

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

        public void InitialiseR68ClaimRepaymentGAD()
        {
            _r68ClaimRepaymentGAD = new R68ClaimRepaymentGAD();
        }

        public abstract void SetDonationDetails();
        // public abstract void SetDonor();
        // public abstract void SetAggDonation();
        public abstract void SetDonation();
    }

    public class R68ClaimRepaymentGADCreator
    {
        private R68ClaimRepaymentGADBuilderBase _r68ClaimRepaymentGADBuilder;

        public R68ClaimRepaymentGADCreator(R68ClaimRepaymentGADBuilderBase r68ClaimRepaymentGADBuilder)
        {
            _r68ClaimRepaymentGADBuilder = r68ClaimRepaymentGADBuilder;
        }

        public void CreateR68ClaimRepaymentGAD()
        {
            _r68ClaimRepaymentGADBuilder.InitialiseR68ClaimRepaymentGAD();
            _r68ClaimRepaymentGADBuilder.SetDonationDetails();
            _r68ClaimRepaymentGADBuilder.SetDonation();
//Choice here depending on whether it's an Agg or a Donor
            //_r68ClaimRepaymentGADBuilder.SetDonor();
            //_r68ClaimRepaymentGADBuilder.SetAggDonation();
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
        public void CreateR68ClaimRepaymentGAD()
        {
            InitialiseR68ClaimRepaymentGAD();
            SetDonationDetails();
        }

        public override void SetDonationDetails()
        {
            try
            {
                R68ClaimRepaymentGAD.Date = Convert.ToDateTime(InputDataRow["Date"]);
                R68ClaimRepaymentGAD.Total = Convert.ToDecimal(InputDataRow["Total"]);
            }
            catch(ArgumentException argEx)
            {
                // Trying to catch error caused by row without correct name
                string msg = String.Format("The column named {0} does not exist in the input row");
                throw new ArgumentException(msg);
            }
            catch(FormatException fEx)
            {
                // Trying to catch value exceptions for the DateTime and Decimal conversions
            }                        
        }

        public override void SetDonation()
        {
            throw new NotImplementedException();
        }

        //public override void SetDonor()
        //{
        //    throw new NotImplementedException();
        //}
        //public override void SetAggDonation()
        //{
        //    throw new NotImplementedException();
        //}
    }

    public class DonorR68ClaimRepaymentGADBuilder : DefaultR68ClaimRepaymentGADBuilder
    {        
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
