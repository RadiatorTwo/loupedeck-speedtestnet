namespace Loupedeck.SpeedTestNetPlugin.Speedtest.Models
{
    using System.Xml.Serialization;

    [XmlRoot("settings")]
    public class ServerList
    {
        [XmlArray("servers")]
        [XmlArrayItem("server")]
        public Server[] Servers { get; set; }
    }
}
