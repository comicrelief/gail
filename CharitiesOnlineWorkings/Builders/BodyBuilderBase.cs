using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using hmrcclasses;

namespace CharitiesOnlineWorkings.Builders
{
    public abstract class BodyBuilderBase
    {
        private GovTalkMessageBody _body;

        public GovTalkMessageBody Body
        {
            get
            {
                return _body;
            }
        }

        public void InitialiseBody()
        {
            _body = new GovTalkMessageBody();
        }

        public abstract void BuildBody();
        public abstract void AddBodyElements();
    }

    public class SubmitRequestBodyBuilder : BodyBuilderBase
    {
        public void CreateBody()
        {
            InitialiseBody();
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

            IRenvelopeCreator irEnvelopeCreator = new IRenvelopeCreator(new DefaultIRenvelopeBuilder());
            irEnvelopeCreator.CreateIRenvelope();

            hmrcclasses.IRenvelope ire = irEnvelopeCreator.GetIRenvelope();
            
            XmlElement xe = Helpers.SerializeIREnvelope(ire);
            XmlElement[] XmlElementIRenvelope = new XmlElement[1];
            XmlElementIRenvelope[0] = xe;

            Body.Any = XmlElementIRenvelope;

            Console.WriteLine("Built a SubmitRequestBody");
            
            // throw new NotImplementedException();
        }

        public override void AddBodyElements()
        {
            Console.WriteLine("Added some Elements to a SubmitRequest Body");

            // throw new NotImplementedException();
        }
    }

    public class SubmitRequestCompressedBodyBuilder : BodyBuilderBase
    {
        public void CreateBody()
        {
            InitialiseBody();
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

            IRenvelopeCreator irEnvelopeCreator = new IRenvelopeCreator(new CompressedIRenvelopeBuilder());
            irEnvelopeCreator.CreateIRenvelope();

            hmrcclasses.IRenvelope ire = irEnvelopeCreator.GetIRenvelope();

            XmlElement xe = Helpers.SerializeIREnvelope(ire);
            XmlElement[] XmlElementIRenvelope = new XmlElement[1];
            XmlElementIRenvelope[0] = xe;

            Body.Any = XmlElementIRenvelope;

            Console.WriteLine("Built a SubmitRequestBody");

            // throw new NotImplementedException();
        }

        public override void AddBodyElements() { }                   
       
    }

    public class EmptyBodyBuilder : BodyBuilderBase
    {
        public void CreateBody()
        {
            InitialiseBody();
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
        public BodyCreator(BodyBuilderBase bodyBuilder)
        {
            _bodyBuilder = bodyBuilder;
        }

        public void CreateBody()
        {
            _bodyBuilder.InitialiseBody();
            _bodyBuilder.BuildBody();
            _bodyBuilder.AddBodyElements();

        }

        public GovTalkMessageBody GetBody()
        {
            return _bodyBuilder.Body;
        }
    }
}
