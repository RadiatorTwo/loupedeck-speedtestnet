namespace Loupedeck.SpeedTestNetPlugin.Speedtest.Models
{
    using System;

    public class ProgressInfo
    {
        public Int64 TotalBytes { get; set; }
        public Int64 BytesProcessed { get; set; }
        public Double Speed { get; set; }
    }
}
