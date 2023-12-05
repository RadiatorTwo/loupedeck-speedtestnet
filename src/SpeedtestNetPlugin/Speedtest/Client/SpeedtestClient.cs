namespace Loupedeck.SpeedTestNetPlugin.Speedtest.Client
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.IO;
    using Newtonsoft.Json;
    using Loupedeck.SpeedtestNetPlugin.Speedtest.Models;

    public class SpeedtestClient : ISpeedtestClient
    {
        public event EventHandler<Ping> PingDone;
        public event EventHandler<Download> DownloadProgress;
        public event EventHandler<Download> DownloadDone;
        public event EventHandler<Upload> UploadProgress;
        public event EventHandler<Upload> UploadDone;

        public String consoleAppPath;
        public const String consoleAppArguments = "-f jsonl --progress-update-interval=1000";

        private readonly List<Ping> Pings = new List<Ping>();

        private Boolean _running = false;

        public void TestSpeed()
        {
            if (this._running)
            {
                throw new InvalidOperationException("Speedtest already running");
            }

#if DEBUG
            this.consoleAppPath = Path.Combine("Speedtest", "speedtest.exe");
#else
            this.consoleAppPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Loupedeck", "Plugins", "SpeedtestNet", "win", "Speedtest", "speedtest.exe");
#endif
            this._running = true;

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = this.consoleAppPath,
                    Arguments = consoleAppArguments,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = new Process { StartInfo = psi })
                {
                    process.Start();

                    // Read the output in a separate thread
                    var outputReader = new Thread(() =>
                    {
                        using (StreamReader reader = process.StandardOutput)
                        {
                            while (!reader.EndOfStream)
                            {
                                var line = reader.ReadLine();

                                if (line.Contains("\"type\":\"ping\""))
                                {
                                    var ping = JsonConvert.DeserializeObject<Ping>(line);
                                    this.Pings.Add(ping);

                                    if (ping.PingPing.Progress >= 1.0)
                                    {
                                        var lowestPing = this.Pings.OrderBy((p) => p.PingPing.Latency).FirstOrDefault();
                                        PingDone?.Invoke(this, lowestPing);
                                    }
                                }
                                else if (line.Contains("\"type\":\"download\""))
                                {
                                    var download = JsonConvert.DeserializeObject<Download>(line);

                                    if (download.DownloadDownload.Progress >= 1.0)
                                    {
                                        DownloadDone?.Invoke(this, download);
                                    }
                                    else
                                    {
                                        DownloadProgress?.Invoke(this, download);
                                    }
                                }
                                else if (line.Contains("\"type\":\"upload\""))
                                {
                                    var upload = JsonConvert.DeserializeObject<Upload>(line);

                                    if (upload.UploadUpload.Progress >= 1.0)
                                    {
                                        UploadDone?.Invoke(this, upload);
                                    }
                                    else
                                    {
                                        UploadProgress?.Invoke(this, upload);
                                    }
                                }
                            }
                        }
                    });

                    outputReader.Start();

                    // Optionally, wait for the process to exit (if needed)
                    process.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                this._running = false;
            }
        }
    }
}
