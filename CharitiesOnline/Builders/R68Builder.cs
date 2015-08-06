using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

using hmrcclasses;
using CR.Infrastructure.Logging;
using CharitiesOnline.Helpers;

namespace CharitiesOnline.Builders
{
    public abstract class R68BuilderBase
    {
        private R68 _r68;
        private ILoggingService _loggingService;
        public R68 R68
        {
            get
            {
                return _r68;
            }
        }

        public void InitialiseR68(ILoggingService loggingService)
        {
            _r68 = new R68();
            _loggingService = loggingService;
        }

        public abstract void SetAuthOfficial();
        public abstract void SetDeclaration();
        public abstract void SetItems();
    }

    public class R68Creator
    {
        private R68BuilderBase _r68Builder;
        private ILoggingService _loggingService;

        public R68Creator(R68BuilderBase r68Builder, ILoggingService loggingService)
        {
            _r68Builder = r68Builder;
            _loggingService = loggingService;
        }

        public void CreateR68()
        {
            _r68Builder.InitialiseR68(_loggingService);
            _r68Builder.SetAuthOfficial();
            _r68Builder.SetDeclaration();
            _r68Builder.SetItems();
        }

        public R68 GetR68()
        {
            return _r68Builder.R68;
        }
    }

    public class ClaimR68Builder : R68BuilderBase
    {
        private ILoggingService _loggingService;

        public ClaimR68Builder(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }
        public void CreateR68()
        {
            InitialiseR68(_loggingService);
            SetAuthOfficial();
            SetDeclaration();
            SetItems();
        }

        public override void SetAuthOfficial()
        {
            R68AuthOfficial AuthOfficial = new R68AuthOfficial();
            r68_NameStructure AuthOfficalName = new r68_NameStructure();
            string[] OffForeName = new string[1];
            OffForeName[0] = ReferenceDataManager.Settings["R68AuthOfficialForeName"];
            AuthOfficalName.Fore = OffForeName;
            AuthOfficalName.Sur = ReferenceDataManager.Settings["R68AuthOfficialSurname"];

            R68AuthOfficialOffID AuthOfficialID = new R68AuthOfficialOffID();
            AuthOfficialID.Item = ReferenceDataManager.Settings["R68AuthOfficialPostcode"];

            AuthOfficial.OffName = AuthOfficalName;
            AuthOfficial.OffID = AuthOfficialID;
            AuthOfficial.Phone = ReferenceDataManager.Settings["R68AuthOfficialPhone"];

            R68.Item = AuthOfficial;
        }
        public override void SetDeclaration()
        {
            // @TODO Low priority make configurable
            R68.Declaration = r68_YesType.yes;
        }
        public override void SetItems()
        {
            RepaymentBuilder dtRepaymenBuilder = new RepaymentBuilder(_loggingService);

            R68ClaimCreator r68ClaimCreator = new R68ClaimCreator(dtRepaymenBuilder);

            r68ClaimCreator.CreateR68Claim();

            // one way ..

            R68Claim[] r68claim = new R68Claim[1];
            r68claim[0] = r68ClaimCreator.GetR68Claim();

            R68.Items = r68claim;
        }

    }

    // @TODO: Add CompressedR68Builder
    // Could put Authofficial & Declaration in default r68builder

    public class CompressedPartR68Builder: R68BuilderBase
    {
        private ILoggingService _loggingService;

        public CompressedPartR68Builder(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }
        public override void SetDeclaration()
        {
            // @TODO Low priority make configurable
            R68.Declaration = r68_YesType.yes;
        }

        public override void SetAuthOfficial()
        {
            R68AuthOfficial AuthOfficial = new R68AuthOfficial();
            r68_NameStructure AuthOfficalName = new r68_NameStructure();
            string[] OffForeName = new string[1];
            OffForeName[0] = ReferenceDataManager.Settings["R68AuthOfficialForeName"];
            AuthOfficalName.Fore = OffForeName;
            AuthOfficalName.Sur = ReferenceDataManager.Settings["R68AuthOfficialSurname"];

            R68AuthOfficialOffID AuthOfficialID = new R68AuthOfficialOffID();
            AuthOfficialID.Item = ReferenceDataManager.Settings["R68AuthOfficialPostcode"];

            AuthOfficial.OffName = AuthOfficalName;
            AuthOfficial.OffID = AuthOfficialID;
            AuthOfficial.Phone = ReferenceDataManager.Settings["R68AuthOfficialPhone"];

            R68.Item = AuthOfficial;
        }
        public override void SetItems()
        {
            RepaymentBuilder dtRepaymenBuilder = new RepaymentBuilder(_loggingService);

            R68ClaimCreator r68ClaimCreator = new R68ClaimCreator(dtRepaymenBuilder);

            r68ClaimCreator.CreateR68Claim();
            
            R68Claim[] r68claim = new R68Claim[1];                
            r68claim[0] = r68ClaimCreator.GetR68Claim();

            R68.Items = r68claim;

            // Serialize R68 to get XmlDocument with Claim element

            System.Xml.XmlDocument r68xmlDoc = XmlSerializationHelpers.SerializeItem(R68);

            // Then extract Claim itself

            System.Xml.XmlDocument claimXmlDoc = GovTalkMessageHelpers.GetClaim(r68xmlDoc);
            
            // Reset R68 Items

            R68.Items = null;

            // Gzip compress

            R68CompressedPart compressedPart = new R68CompressedPart();
            compressedPart.Type = R68CompressedPartType.gzip;          
            compressedPart.Value = CommonUtilityHelpers.CompressData(claimXmlDoc.OuterXml);

            R68CompressedPart[] compressedParts = new R68CompressedPart[1];
            compressedParts[0] = compressedPart;

            R68.Items = compressedParts;

        }
    }
}
