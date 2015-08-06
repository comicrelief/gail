using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

using hmrcclasses;
using CR.Infrastructure.Logging;

namespace CharitiesOnline.Builders
{
    public abstract class IRenvelopeBuilderBase
    {
        private hmrcclasses.IRenvelope _irEnvelope;
        private ILoggingService _loggingService;

        public hmrcclasses.IRenvelope IREnvelope
        {
            get
            {
                return _irEnvelope;
            }
        }

        public void InitialiseIRenvelope(ILoggingService loggingService)
        {
            _irEnvelope = new hmrcclasses.IRenvelope();
            _loggingService = loggingService;
        }

        public abstract void SetIRHeader();
        public abstract void SetR68();

        public XmlElement SerializeIREnvelope(IRenvelope irEnvelope)
        {
            // Always the same regardless of nature of IRenvelope, right?
            // Is this the right place for it - does the builder of IRenvelopes also serialize them?

            using (MemoryStream memStream = new MemoryStream())
            {
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add(String.Empty, "http://www.govtalk.gov.uk/taxation/charities/r68/1");

                var knownTypes = new Type[] { typeof(IRenvelope), typeof(R68Claim) };

                XmlSerializer serializer =
                    new XmlSerializer(typeof(IRenvelope), knownTypes);

                XmlTextWriter tw = new XmlTextWriter(memStream, UTF8Encoding.UTF8);

                XmlDocument doc = new XmlDocument();
                tw.Formatting = Formatting.Indented;
                tw.IndentChar = ' ';
                serializer.Serialize(tw, irEnvelope, ns);
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
        private ILoggingService _loggingService;

        public IRenvelopeCreator(IRenvelopeBuilderBase irEnvelopeBuilder, ILoggingService loggingService)
        {
            _irEnvelopeBuilder = irEnvelopeBuilder;
            _loggingService = loggingService;
        }

        public void CreateIRenvelope()
        {
            _irEnvelopeBuilder.InitialiseIRenvelope(_loggingService);
            _irEnvelopeBuilder.SetIRHeader();
            _irEnvelopeBuilder.SetR68();
        }

        public IRenvelope GetIRenvelope()
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
        private ILoggingService _loggingService;

        public DefaultIRenvelopeBuilder(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }
        public void CreateIRenvelope()
        {
            InitialiseIRenvelope(_loggingService);
            SetIRHeader();
            SetR68();
        }

        public override void SetIRHeader()
        {
            IRHeaderCreator irHeaderCreator = new IRHeaderCreator(new DefaultIRHeaderBuilder(_loggingService), _loggingService);

            irHeaderCreator.CreateIRHeader();

            hmrcclasses.IRheader irheader = irHeaderCreator.GetIRHeader();

            IREnvelope.IRheader = irheader;
        }

        public override void SetR68()
        {
            R68Creator r68Creator = new R68Creator(new ClaimR68Builder(_loggingService), _loggingService);

            r68Creator.CreateR68();

            _loggingService.LogInfo(this, "R68 Envelope created.");

            IREnvelope.R68 = r68Creator.GetR68();
        }
    }

    public class CompressedIRenvelopeBuilder : IRenvelopeBuilderBase
    {
        private ILoggingService _loggingService;

        public CompressedIRenvelopeBuilder(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }
        public void CreateIRenvelope()
        {
            InitialiseIRenvelope(_loggingService);
            SetIRHeader();
            SetR68();
        }

        public override void SetIRHeader()
        {
            IRHeaderCreator irHeaderCreator = new IRHeaderCreator(new DefaultIRHeaderBuilder(_loggingService), _loggingService);

            irHeaderCreator.CreateIRHeader();

            hmrcclasses.IRheader irheader = irHeaderCreator.GetIRHeader();

            IREnvelope.IRheader = irheader;
        }

        public override void SetR68()
        {
            R68Creator r68Creator = new R68Creator(new CompressedPartR68Builder(_loggingService), _loggingService);

            r68Creator.CreateR68();

            _loggingService.LogInfo(this, "Compressed R68 Envelope created.");

            IREnvelope.R68 = r68Creator.GetR68();
        }
    }



}
