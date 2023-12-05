namespace Loupedeck.SpeedTestNetPlugin.Speedtest.Models
{
    using System;
    using Loupedeck.SpeedTestNetPlugin.Speedtest.Client;

    public struct SpeedTestResult
    {
        public SpeedUnit SpeedUnit { get; set; }
        public Double DownloadSpeed { get; set; }
        public Double UploadSpeed { get; set; }
        public Int32 Latency { get; set; }
        public Boolean HasResult { get; set; }

        public SpeedTestResult(SpeedUnit speedUnit, Double downloadSpeed, Double uploadSpeed, Int32 latency, Boolean hasResult)
        {
            this.SpeedUnit = speedUnit;
            this.DownloadSpeed = downloadSpeed;
            this.UploadSpeed = uploadSpeed;
            this.Latency = latency;
            this.HasResult = hasResult;
        }
    }
}
