namespace Loupedeck.SpeedtestNetPlugin.Speedtest.Models
{
    using System;
    using Newtonsoft.Json;

    public partial class Latency
    {
        [JsonProperty("iqm")]
        public Double Iqm { get; set; }
    }
}
