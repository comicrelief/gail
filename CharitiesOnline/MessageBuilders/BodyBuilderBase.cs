using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using hmrcclasses;
using CR.Infrastructure.Logging;
using CharitiesOnline.Helpers;

namespace CharitiesOnline.MessageBuilders
{
    public abstract class BodyBuilderBase
    {
        private GovTalkMessageBody _body;
        private ILoggingService _loggingService;

        public GovTalkMessageBody Body
        {
            get
            {
                return _body;
            }
        }

        public void InitialiseBody(ILoggingService loggingService)
        {
            _body = new GovTalkMessageBody();
            _loggingService = loggingService;
        }

        public abstract void BuildBody();
        public abstract void AddBodyElements();
    }

    






}
