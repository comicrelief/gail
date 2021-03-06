﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;

using hmrcclasses;
using CR.Infrastructure.Logging;

namespace CharitiesOnline.MessageBuilders
{
    public abstract class R68ClaimRepaymentOtherIncBuilderBase
    {
        private R68ClaimRepaymentOtherInc _r68ClaimRepaymentOtherInc;
        private DataRow _inputDataRow;
        private ILoggingService _loggingService;

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

        public R68ClaimRepaymentOtherInc R68ClaimRepayOtherInc
        {
            get
            {
                return _r68ClaimRepaymentOtherInc;
            }
        }

        public void InitialiseR68ClaimRepyamentOtherInc(ILoggingService loggingService)
        {
            _r68ClaimRepaymentOtherInc = new R68ClaimRepaymentOtherInc();
            _loggingService = loggingService;
        }

        public abstract void SetOtherIncome();
    }

    public class R68ClaimRepaymentOtherIncomeCreator
    {
        private R68ClaimRepaymentOtherIncBuilderBase _r68ClaimRepaymentOtherIncBuilder;
        private ILoggingService _loggingService;

        public R68ClaimRepaymentOtherIncomeCreator(R68ClaimRepaymentOtherIncBuilderBase r68ClaimRepaymentOtherIncBuilder, ILoggingService loggingService)
        {
            _r68ClaimRepaymentOtherIncBuilder = r68ClaimRepaymentOtherIncBuilder;
            _loggingService = loggingService;
        }
        public void CreateR68ClaiMRepaymentOtherInc()
        {
            _r68ClaimRepaymentOtherIncBuilder.InitialiseR68ClaimRepyamentOtherInc(_loggingService);
            _r68ClaimRepaymentOtherIncBuilder.SetOtherIncome();
        }

        public R68ClaimRepaymentOtherInc GetR68ClaimRepaymentOtherInc()
        {
            return _r68ClaimRepaymentOtherIncBuilder.R68ClaimRepayOtherInc;
        }

        public void SetInputRow(DataRow inputRow)
        {
            _r68ClaimRepaymentOtherIncBuilder.InputDataRow = inputRow;
        }
    }

    public class DefaultR68ClaimRepaymentOtherIncomeBuilder : R68ClaimRepaymentOtherIncBuilderBase
    {
        private ILoggingService _loggingService;

        public DefaultR68ClaimRepaymentOtherIncomeBuilder(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }
        public void CreateR68ClaimRepaymentOtherInc()
        {
            InitialiseR68ClaimRepyamentOtherInc(_loggingService);
            SetOtherIncome();
        }
        public override void SetOtherIncome()
        {

            R68ClaimRepayOtherInc.Payer = InputDataRow["Payer"].ToString();
            R68ClaimRepayOtherInc.OIDate = Convert.ToDateTime(InputDataRow["OIDate"]);
            R68ClaimRepayOtherInc.Gross = Convert.ToDecimal(InputDataRow["Gross"]);
            R68ClaimRepayOtherInc.Tax = Convert.ToDecimal(InputDataRow["Tax"]);            
        }
    }
        

}
