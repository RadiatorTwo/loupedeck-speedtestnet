namespace Loupedeck.SpeedTestNetPlugin.Speedtest.Client
{
    using System;
    using System.Threading.Tasks;

    using Loupedeck.SpeedTestNetPlugin.Speedtest.Models;

    public enum SpeedUnit
    {
        Kbps,
        KBps,
        Mbps,
        MBps
    }

    public enum TestStage
    {
        Stopped,
        Prepare,
        Latency,
        Download,
        Upload
    }

    public interface ISpeedtestClient
    {
        TestStage CurrentStage { get; set; }
        SpeedUnit SpeedUnit { get; }
        event EventHandler<TestStage> StageChanged;
        event EventHandler<ProgressInfo> ProgressChanged;
        Task<SpeedTestResult> TestSpeedAsync(SpeedUnit speedUnit,
            Int32 parallelTasks = 8,
            Boolean testLatency = true,
            Boolean testDownload = true,
            Boolean testUpload = true);

        Task<SpeedTestResult> TestLatencyAsync();
        SpeedTestResult TestLatency();
    }
}
