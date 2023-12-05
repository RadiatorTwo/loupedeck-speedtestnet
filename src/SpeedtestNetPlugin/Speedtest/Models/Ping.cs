namespace Loupedeck.SpeedtestNetPlugin.Speedtest.Models
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class Ping
    {
        [JsonProperty("type")]
        public String Type { get; set; }

        [JsonProperty("timestamp")]
        public DateTimeOffset Timestamp { get; set; }

        [JsonProperty("ping")]
        public PingClass PingPing { get; set; }
    }

    public partial class PingClass
    {
        [JsonProperty("jitter")]
        public Int64 Jitter { get; set; }

        [JsonProperty("latency")]
        public Double Latency { get; set; }

        [JsonProperty("progress")]
        public Double Progress { get; set; }
    }
}
