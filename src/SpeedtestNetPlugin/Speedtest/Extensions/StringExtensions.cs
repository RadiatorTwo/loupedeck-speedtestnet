namespace Loupedeck.SpeedTestNetPlugin.Speedtest.Extensions
{
    using System;
    using System.IO;
    using System.Xml.Serialization;

    public static class StringExtensions
    {
        public static T DeserializeFromXml<T>(this String data)
        {
            var xmlSerializer = new XmlSerializer(typeof(T));

            using (var reader = new StringReader(data))
            {
                return (T)xmlSerializer.Deserialize(reader);
            }
        }

        public static String Append(this String originalString, String value) => originalString + value;
    }
}
