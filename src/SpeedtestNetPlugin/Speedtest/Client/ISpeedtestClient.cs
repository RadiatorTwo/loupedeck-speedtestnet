﻿namespace Loupedeck.SpeedTestNetPlugin.Speedtest.Client
{
    using System;
    using Loupedeck.SpeedtestNetPlugin.Speedtest.Models;

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
        event EventHandler<Ping> PingDone;
        event EventHandler<Download> DownloadProgress;
        event EventHandler<Download> DownloadDone;
        event EventHandler<Upload> UploadProgress;
        event EventHandler<Upload> UploadDone;

        void TestSpeed();
    }
}
