namespace Loupedeck.SpeedTestNetPlugin
{
    using System;

    using Loupedeck.SpeedTestNetPlugin.Speedtest.Client;
    using Loupedeck.SpeedTestNetPlugin.Speedtest.Models;

    // This class implements an example command that counts button presses.

    public class LatencyTestCommand : PluginDynamicCommand
    {
        readonly ISpeedtestClient SpeedtestClient = new SpeedtestClient();
        SpeedTestResult SpeedTestResult { get; set; }

        public LatencyTestCommand()
            : base(displayName: "Latency Test", description: "Executes a Latency Test", groupName: "Commands")
        {
        }

        protected override Boolean OnLoad()
        {
            this.SpeedTestResult = new SpeedTestResult(0, 0, 0, 0, false);
            return true;
        }

        protected override void RunCommand(String actionParameter)
        {
            this.SpeedtestClient.CurrentStage = TestStage.Prepare;
            this.ActionImageChanged();
            System.Threading.Thread.Sleep(100);
            this.SpeedtestClient.CurrentStage = TestStage.Stopped;

            this.SpeedTestResult = this.SpeedtestClient.TestLatency();
            this.ActionImageChanged();
        }

        // This method is called when Loupedeck needs to show the command on the console or the UI.
        protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize)
        {
            switch (this.SpeedtestClient.CurrentStage)
            {
                case TestStage.Prepare:
                    return "Testing...";
                case TestStage.Stopped:
                    return this.SpeedTestResult.HasResult ? $"Latency: {Environment.NewLine} {this.SpeedTestResult.Latency} ms" : "Latency Test";
                default:
                    return "Latency Test";
            }
        }
    }
}
