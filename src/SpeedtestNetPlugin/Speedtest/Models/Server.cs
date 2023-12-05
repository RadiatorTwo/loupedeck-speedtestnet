namespace Loupedeck.SpeedTestNetPlugin.Speedtest.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Serialization;

    public class Server
    {
        [XmlAttribute("id")]
        public Int32 Id { get; set; }

        [XmlAttribute("name")]
        public String Name { get; set; }

        [XmlAttribute("country")]
        public String Country { get; set; }

        [XmlAttribute("sponsor")]
        public String Sponsor { get; set; }

        [XmlAttribute("host")]
        public String Host { get; set; }

        [XmlAttribute("url")]
        public String Url { get; set; }

        [XmlAttribute("lat")]
        public Double Latitude { get; set; }

        [XmlAttribute("lon")]
        public Double Longitude { get; set; }

        [XmlIgnore]
        public Int32 Latency { get; set; }
    }
}
