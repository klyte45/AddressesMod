using Klyte.Commons.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Klyte.Addresses.Xml
{
    [XmlRoot("adrConfig")]
    public class AdrConfigXml : DataExtensionBase<AdrConfigXml>
    {
        [XmlElement("global")]
        public AdrGlobalConfig GlobalConfig { get; set; } = new AdrGlobalConfig();

        [XmlArray("districts")]
        [XmlArrayItem("district")]
        public List<AdrDistrictConfig> DistrictConfigs
        {
            get => m_districtConfigs;

            set => m_districtConfigs = value;
        }

        [XmlIgnore]
        public override string SaveId => "K45_ADR_MAIN";      

        [XmlIgnore]
        private List<AdrDistrictConfig> m_districtConfigs = new List<AdrDistrictConfig>();

        public AdrDistrictConfig GetConfigForDistrict(ushort districtId)
        {
            if (m_districtConfigs.Where(x => x.Id == districtId).Count() == 0)
            {
                m_districtConfigs.Add(new AdrDistrictConfig
                {
                    Id = districtId
                });
            }
            return m_districtConfigs.Where(x => x.Id == districtId).FirstOrDefault();
        }
    }
}

