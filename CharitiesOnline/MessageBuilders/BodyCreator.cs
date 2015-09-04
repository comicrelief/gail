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
    public class BodyCreator
    {
        private BodyBuilderBase _bodyBuilder;
        private ILoggingService _loggingService;
        public BodyCreator(BodyBuilderBase bodyBuilder, ILoggingService loggingService)
        {
            _bodyBuilder = bodyBuilder;
            _loggingService = loggingService;
        }

        public void CreateBody()
        {
            _bodyBuilder.InitialiseBody(_loggingService);
            _bodyBuilder.BuildBody();
            _bodyBuilder.AddBodyElements();

        }

        public GovTalkMessageBody GetBody()
        {
            return _bodyBuilder.Body;
        }
    }
}
