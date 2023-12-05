namespace Loupedeck.SpeedtestNetPlugin.Speedtest.Models
{
    using System;
    using Newtonsoft.Json;

    public partial class Upload
    {
        [JsonProperty("type")]
        public String Type { get; set; }

        [JsonProperty("timestamp")]
        public DateTimeOffset Timestamp { get; set; }

        [JsonProperty("upload")]
        public UploadClass UploadUpload { get; set; }
    }

    public partial class UploadClass
    {
        [JsonProperty("bandwidth")]
        public Int64 Bandwidth { get; set; }

        [JsonProperty("bytes")]
        public Int64 Bytes { get; set; }

        [JsonProperty("elapsed")]
        public Int64 Elapsed { get; set; }

        [JsonProperty("latency")]
        public Latency Latency { get; set; }

        [JsonProperty("progress")]
        public Double Progress { get; set; }
    }
}
