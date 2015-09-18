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
    public class EmptyBodyBuilder : BodyBuilderBase
    {
        private ILoggingService _loggingService;

        public EmptyBodyBuilder(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }
        public void CreateBody()
        {
            InitialiseBody(_loggingService);
            BuildBody();
        }

        public override void BuildBody()
        {
        }

        public override void AddBodyElements()
        {
        }
    }
}
