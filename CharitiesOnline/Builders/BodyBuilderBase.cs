using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using hmrcclasses;
using CR.Infrastructure.Logging;

namespace CharitiesOnline.Builders
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
            
            XmlElement xe = Helpers.SerializeIREnvelope(ire);
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

    public class SubmitRequestCompressedBodyBuilder : BodyBuilderBase
    {
        private ILoggingService _loggingService;
        public SubmitRequestCompressedBodyBuilder(ILoggingService loggingService)
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

            IRenvelopeCreator irEnvelopeCreator = new IRenvelopeCreator(new CompressedIRenvelopeBuilder(_loggingService), _loggingService);
            irEnvelopeCreator.CreateIRenvelope();

            hmrcclasses.IRenvelope ire = irEnvelopeCreator.GetIRenvelope();

            XmlElement xe = Helpers.SerializeIREnvelope(ire);
            XmlElement[] XmlElementIRenvelope = new XmlElement[1];
            XmlElementIRenvelope[0] = xe;

            Body.Any = XmlElementIRenvelope;

           _loggingService.LogInfo(this, "Built a SubmitRequestBody");

            // throw new NotImplementedException();
        }

        public override void AddBodyElements() { }                   
       
    }

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
