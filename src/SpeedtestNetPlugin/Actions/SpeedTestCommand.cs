namespace Loupedeck.SpeedTestNetPlugin
{
    using System;
    using System.Text;

    using Loupedeck.SpeedtestNetPlugin;
    using Loupedeck.SpeedtestNetPlugin.Speedtest.Models;
    using Loupedeck.SpeedTestNetPlugin.Speedtest.Client;
    using Loupedeck.SpeedTestNetPlugin.Speedtest.Extensions;

    // This class implements an example command that counts button presses.

    public class SpeedTestCommand : PluginDynamicCommand
    {
        readonly ISpeedtestClient SpeedtestClient = new SpeedtestClient();

        private Double _downloadSpeed = -1;
        private Double _uploadSpeed = -1;
        private Double _ping = -1;

        public SpeedTestCommand()
            : base(displayName: "Speed Test", description: "Executes a Speed Test", groupName: "Commands")
        {
        }

        protected override Boolean OnLoad()
        {
            this.SpeedtestClient.PingDone += this.SpeedtestClient_PingDone;
            this.SpeedtestClient.DownloadProgress += this.SpeedtestClient_DownloadProgress;
            this.SpeedtestClient.DownloadDone += this.SpeedtestClient_DownloadDone;
            this.SpeedtestClient.UploadProgress += this.SpeedtestClient_UploadProgress;
            this.SpeedtestClient.UploadDone += this.SpeedtestClient_UploadDone;

            return true;
        }

        private void SpeedtestClient_UploadDone(Object sender, Upload e)
        {
            this._uploadSpeed = e.UploadUpload.Bandwidth;
            this.ActionImageChanged();
        }

        private void SpeedtestClient_UploadProgress(Object sender, Upload e)
        {
            this._uploadSpeed = e.UploadUpload.Bandwidth;
            this.ActionImageChanged();
        }

        private void SpeedtestClient_DownloadDone(Object sender, Download e)
        {
            this._downloadSpeed = e.DownloadDownload.Bandwidth;
            this.ActionImageChanged();
        }

        private void SpeedtestClient_DownloadProgress(Object sender, Download e)
        {
            this._downloadSpeed = e.DownloadDownload.Bandwidth;
            this.ActionImageChanged();
        }

        private void SpeedtestClient_PingDone(Object sender, Ping e)
        {
            this._ping = e.PingPing.Latency;

            this.ActionImageChanged();
            System.Threading.Thread.Sleep(100);
        }

        protected override void RunCommand(String actionParameter)
        {
            PluginLog.Info("Started Speedtest");
            PluginLog.Info("Current folder: " + Environment.CurrentDirectory);
            try
            {
                this.SpeedtestClient.TestSpeed();
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, ex.Message);
            }

            this.ActionImageChanged();
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            var sb = new StringBuilder();
            var bmpBuilder = new BitmapBuilder(imageSize);

            sb.AppendLine($"Ping: {Math.Round(this._ping)} ms");
            sb.AppendLine($"↓: {(this._downloadSpeed <= -1 ? "N/A" : $"{this._downloadSpeed.ToPrettySize()}/s")}");
            sb.AppendLine($"↑: {(this._uploadSpeed <= -1 ? "N/A" : $"{this._uploadSpeed.ToPrettySize()}/s")}");

            bmpBuilder.DrawText(sb.ToString(), fontSize: 12);
            return bmpBuilder.ToImage();
        }
    }
}
