namespace Loupedeck.SpeedTestNetPlugin.Speedtest.Extensions
{
    using System;
    using System.IO;
    using System.Xml.Serialization;

    public static class StringExtensions
    {
        private const Int64 OneKb = 1024;
        private const Int64 OneMb = OneKb * 1024;
        private const Int64 OneGb = OneMb * 1024;
        private const Int64 OneTb = OneGb * 1024;

        public static T DeserializeFromXml<T>(this String data)
        {
            var xmlSerializer = new XmlSerializer(typeof(T));

            using (var reader = new StringReader(data))
            {
                return (T)xmlSerializer.Deserialize(reader);
            }
        }

        public static String Append(this String originalString, String value) => originalString + value;

        public static String ToPrettySize(this Double value, Int32 decimalPlaces = 2)
        {
            var asTb = Math.Round(value / OneTb, decimalPlaces);
            var asGb = Math.Round(value / OneGb, decimalPlaces);
            var asMb = Math.Round(value / OneMb, decimalPlaces);
            var asKb = Math.Round(value / OneKb, decimalPlaces);
            var chosenValue = asTb > 1 ? $"{asTb}TB"
                : asGb > 1 ? String.Format("{0}GB", asGb)
                : asMb > 1 ? String.Format("{0}MB", asMb)
                : asKb > 1 ? String.Format("{0}KB", asKb)
                : $"{Math.Round(value, decimalPlaces)}B";
            return chosenValue;
        }
    }
}
