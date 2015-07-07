using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

using hmrcclasses;

namespace CharitiesOnlineWorkings.Builders
{
    public abstract class IRenvelopeBuilderBase
    {
        private hmrcclasses.IRenvelope _irEnvelope;

        public hmrcclasses.IRenvelope IREnvelope
        {
            get
            {
                return _irEnvelope;
            }
        }

        public void InitialiseIRenvelope()
        {
            _irEnvelope = new hmrcclasses.IRenvelope();
        }

        public abstract void SetIRHeader();
        public abstract void SetR68();

        public XmlElement SerializeIREnvelope(hmrcclasses.IRenvelope ire)
        {
            // Always the same regardless of nature of IRenvelope, right?
            // Is this the right place for it - does the builder of IRenvelopes also serialize them?

            using (MemoryStream memStream = new MemoryStream())
            {
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add(String.Empty, "http://www.govtalk.gov.uk/taxation/charities/r68/1");

                var knownTypes = new Type[] { typeof(hmrcclasses.IRenvelope), typeof(R68Claim) };

                XmlSerializer serializer =
                    new XmlSerializer(typeof(hmrcclasses.IRenvelope), knownTypes);

                XmlTextWriter tw = new XmlTextWriter(memStream, UTF8Encoding.UTF8);

                XmlDocument doc = new XmlDocument();
                tw.Formatting = Formatting.Indented;
                tw.IndentChar = ' ';
                serializer.Serialize(tw, ire, ns);
                memStream.Seek(0, SeekOrigin.Begin);
                doc.Load(memStream);
                XmlElement returnVal = doc.DocumentElement;

                return returnVal;
            }
        }
    }

    public class IRenvelopeCreator
    {
        private IRenvelopeBuilderBase _irEnvelopeBuilder;

        public IRenvelopeCreator(IRenvelopeBuilderBase irEnvelopeBuilder)
        {
            _irEnvelopeBuilder = irEnvelopeBuilder;
        }

        public void CreateIRenvelope()
        {
            _irEnvelopeBuilder.InitialiseIRenvelope();
            _irEnvelopeBuilder.SetIRHeader();
            _irEnvelopeBuilder.SetR68();
        }

        public hmrcclasses.IRenvelope GetIRenvelope()
        {
            return _irEnvelopeBuilder.IREnvelope;
        }

        //public XmlElement SerializeIREnvelope(hmrcclasses.IRenvelope ire)
        //{
        //    return _irEnvelopeBuilder.SerializeIREnvelope(ire);
        //}
    }

    public class DefaultIRenvelopeBuilder : IRenvelopeBuilderBase
    {
        public void CreateIRenvelope()
        {
            InitialiseIRenvelope();
            SetIRHeader();
            SetR68();
        }

        public override void SetIRHeader()
        {
            IRHeaderCreator irHeaderCreator = new IRHeaderCreator(new DefaultIRHeaderBuilder());

            irHeaderCreator.CreateIRHeader();

            hmrcclasses.IRheader irheader = irHeaderCreator.GetIRHeader();

            IREnvelope.IRheader = irheader;
        }

        public override void SetR68()
        {
            R68Creator r68Creator = new R68Creator(new ClaimR68Builder());

            r68Creator.CreateR68();

            IREnvelope.R68 = r68Creator.GetR68();
        }
    }

    public class CompressedIRenvelopeBuilder : IRenvelopeBuilderBase
    {
        public void CreateIRenvelope()
        {
            InitialiseIRenvelope();
            SetIRHeader();
            SetR68();
        }

        public override void SetIRHeader()
        {
            IRHeaderCreator irHeaderCreator = new IRHeaderCreator(new DefaultIRHeaderBuilder());

            irHeaderCreator.CreateIRHeader();

            hmrcclasses.IRheader irheader = irHeaderCreator.GetIRHeader();

            IREnvelope.IRheader = irheader;
        }

        public override void SetR68()
        {
            R68Creator r68Creator = new R68Creator(new CompressedPartR68Builder());

            r68Creator.CreateR68();

            IREnvelope.R68 = r68Creator.GetR68();
        }
    }



}
