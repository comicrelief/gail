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
    public class SubmitRequestBodyBuilder : BodyBuilderBase
    {
        private ILoggingService _loggingService;
        public SubmitRequestBodyBuilder(ILoggingService loggingService)
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
            // do the building logic for the Body of a SubmitRequest

            #region old
            //GovTalkMessageBody Body = new GovTalkMessageBody();
            //IR68 ir68 = new IR68(_localData);
            //IRenvelope ire = new IRenvelope();
            //ire = ir68.CreateIRBody();
            #endregion old

            IRenvelopeCreator irEnvelopeCreator = new IRenvelopeCreator(new DefaultIRenvelopeBuilder(_loggingService), _loggingService);
            irEnvelopeCreator.CreateIRenvelope();

            hmrcclasses.IRenvelope ire = irEnvelopeCreator.GetIRenvelope();

            XmlElement xe = XmlSerializationHelpers.SerializeIREnvelope(ire);
            XmlElement[] XmlElementIRenvelope = new XmlElement[1];
            XmlElementIRenvelope[0] = xe;

            Body.Any = XmlElementIRenvelope;

            _loggingService.LogInfo(this, "Built a SubmitRequestBody");

            // throw new NotImplementedException();
        }

        public override void AddBodyElements()
        {
            _loggingService.LogInfo(this, "Added some Elements to a SubmitRequest Body");

            // throw new NotImplementedException();
        }
    }
}
