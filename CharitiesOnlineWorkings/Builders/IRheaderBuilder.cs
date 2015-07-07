using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Only need this in the Config implementation
using System.Configuration;

using hmrcclasses;

namespace CharitiesOnlineWorkings.Builders
{
    public abstract class IRheaderBuilderBase
    {
        private hmrcclasses.IRheader _irHeader;

        public hmrcclasses.IRheader IRHeader
        {
            get
            {
                return _irHeader;
            }
        }

        public void InitialiseIRHeader()
        {
            _irHeader = new hmrcclasses.IRheader();
        }

        public abstract void SetKeys();
        public abstract void SetIRPeriodEnd();
        public abstract void SetIRCurrency();
        public abstract void SetIRSender();
        public abstract void SetIRMark();
    }

    public class IRHeaderCreator
    {
        private IRheaderBuilderBase _irHeaderBuilder;

        public IRHeaderCreator(IRheaderBuilderBase irHeaderBuilder)
        {
            _irHeaderBuilder = irHeaderBuilder;
        }

        public void CreateIRHeader()
        {
            _irHeaderBuilder.InitialiseIRHeader();
            _irHeaderBuilder.SetKeys();
            _irHeaderBuilder.SetIRCurrency();
            _irHeaderBuilder.SetIRPeriodEnd();
            _irHeaderBuilder.SetIRSender();
            _irHeaderBuilder.SetIRMark();
        }

        public hmrcclasses.IRheader GetIRHeader()
        {
            return _irHeaderBuilder.IRHeader;
        }
    }

    public class DefaultIRHeaderBuilder : IRheaderBuilderBase
    {
        // Use a config file to create the IR header

        public void CreateIRheader()
        {
            InitialiseIRHeader();
            SetKeys();
            SetIRPeriodEnd();
            SetIRCurrency();
            SetIRSender();
            SetIRMark();
        }

        public override void SetKeys()
        {
            //@TODO: Need to consider multiple keys as a possibility
            IRheaderKey key = new IRheaderKey(); //Factory, or no need because its a fixed dependency?

            key.Type = ReferenceDataManager.Settings["GovTalkDetailsKeyType"];
            key.Value = ReferenceDataManager.Settings["GovTalkDetailsKey"];

            IRheaderKey[] keys = new IRheaderKey[1];
            keys[0] = key;

            IRHeader.Keys = keys;
        }

        public override void SetIRPeriodEnd()
        {
            IRHeader.PeriodEnd = Convert.ToDateTime(ReferenceDataManager.Settings["IRheaderPeriodEnd"]);
        }

        public override void SetIRCurrency()
        {
            // low prioerity @TODO make this configurable

            IRHeader.DefaultCurrencySpecified = true;
            IRHeader.DefaultCurrency = IRheaderDefaultCurrency.GBP;
        }

        public override void SetIRSender()
        {
            switch (ReferenceDataManager.Settings["IRheaderSender"])
            {
                case "ActinginCapacity":
                    IRHeader.Sender = IRheaderSender.ActinginCapacity;
                    break;
                case "Agent":
                    IRHeader.Sender = IRheaderSender.Agent;
                    break;
                case "Bureau":
                    IRHeader.Sender = IRheaderSender.Bureau;
                    break;
                case "Company":
                    IRHeader.Sender = IRheaderSender.Company;
                    break;
                case "Employer":
                    IRHeader.Sender = IRheaderSender.Employer;
                    break;
                case "Government":
                    IRHeader.Sender = IRheaderSender.Government;
                    break;
                case "Individual":
                    IRHeader.Sender = IRheaderSender.Individual;
                    break;
                case "Other":
                    IRHeader.Sender = IRheaderSender.Other;
                    break;
                case "Partnership":
                    IRHeader.Sender = IRheaderSender.Partnership;
                    break;
                case "Trust":
                    IRHeader.Sender = IRheaderSender.Trust;
                    break;
            }
        }

        public override void SetIRMark()
        {
            IRheaderIRmark irmark = new IRheaderIRmark();
            irmark.Type = IRheaderIRmarkType.generic;
            irmark.Value = "";

            IRHeader.IRmark = irmark;
        }
    }

    //public class DbIRHeaderBuilder : IRheaderBuilderBase
    //{
    //    // Use a DB to create the IR header
    //}
}
