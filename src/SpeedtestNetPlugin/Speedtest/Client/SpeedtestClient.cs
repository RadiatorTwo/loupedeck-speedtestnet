namespace Loupedeck.SpeedTestNetPlugin.Speedtest.Client
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Loupedeck.SpeedTestNetPlugin.Speedtest.Models;
    using Loupedeck.SpeedTestNetPlugin.Speedtest.Extensions;

    public class SpeedtestClient : ISpeedtestClient
    {
        public const String ServersUrl = "http://www.speedtest.net/speedtest-servers.php";
        public const String Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public const Int32 MaxUploadSize = 6;
        public static readonly Int32[] DownloadSizes = { 1500, 2000, 3000, 3500, 4000 };

        public TestStage CurrentStage { get; set; } = TestStage.Stopped;
        public SpeedUnit SpeedUnit { get; private set; } = SpeedUnit.Kbps;

        public event EventHandler<TestStage> StageChanged;
        public event EventHandler<ProgressInfo> ProgressChanged;

        public async Task<SpeedTestResult> TestSpeedAsync(SpeedUnit speedUnit,
            Int32 parallelTasks = 8,
            Boolean testLatency = true,
            Boolean testDownload = true,
            Boolean testUpload = true)
        {
            if (this.CurrentStage != TestStage.Stopped)
            {
                throw new InvalidOperationException("Speedtest already running");
            }
            this.SpeedUnit = speedUnit;

            try
            {
                var server = await this.GetBestServerByLatencyAsync() ?? throw new InvalidOperationException("No server was found");

                var latency = testLatency ? await this.TestServerLatencyAsync(server) : -1;
                var downloadSpeed = testDownload ? await this.TestDownloadSpeedAsync(server, parallelTasks) : -1;
                var uploadSpeed = testUpload ? await this.TestUploadSpeedAsync(server, parallelTasks) : -1;

                return new SpeedTestResult(speedUnit, downloadSpeed, uploadSpeed, latency, true);
            }
            finally
            {
                this.SetStage(TestStage.Stopped);
            }
        }

        public async Task<SpeedTestResult> TestLatencyAsync()
        {
            if (this.CurrentStage != TestStage.Stopped)
            {
                throw new InvalidOperationException("Speedtest already running");
            }

            try
            {
                var server = await this.GetBestServerByLatencyAsync() ?? throw new InvalidOperationException("No server was found");

                //var latency = await this.TestServerLatencyAsync(server);
             
                return new SpeedTestResult(0, 0, 0, server.Latency, true);
            }
            finally
            {
                this.SetStage(TestStage.Stopped);
            }
        }

        public SpeedTestResult TestLatency()
        {
            if (this.CurrentStage != TestStage.Stopped)
            {
                throw new InvalidOperationException("Speedtest already running");
            }

            try
            {
                var server = this.GetBestServerByLatency() ?? throw new InvalidOperationException("No server was found");

                return new SpeedTestResult(0, 0, 0, server.Latency, true);
            }
            finally
            {
                this.SetStage(TestStage.Stopped);
            }
        }

        private async Task<Server> GetBestServerByLatencyAsync()
        {
            var servers = await this.FetchServersAsync();
            var serverLatency = new Dictionary<Server, Int32>();

            foreach (var server in servers)
            {
                try
                {
                    var latency = await this.TestServerLatencyAsync(server);
                    server.Latency = latency;
                    serverLatency.Add(server, latency);
                }
                catch { }
            }

            return serverLatency.OrderBy(x => x.Value).Select(x => x.Key).FirstOrDefault();
        }

        private Server GetBestServerByLatency()
        {
            var servers = this.FetchServers();
            var serverLatency = new Dictionary<Server, Int32>();

            foreach (var server in servers)
            {
                try
                {
                    var latency = this.TestServerLatency(server);
                    server.Latency = latency;
                    serverLatency.Add(server, latency);
                }
                catch { }
            }

            return serverLatency.OrderBy(x => x.Value).Select(x => x.Key).FirstOrDefault();
        }

        private async Task<Server[]> FetchServersAsync()
        {
            using (var httpClient = GetHttpClient())
            {
                var serversXml = await httpClient.GetStringAsync(ServersUrl);
                return serversXml.DeserializeFromXml<ServerList>().Servers ?? Array.Empty<Server>();
            }
        }

        private Server[] FetchServers()
        {
            using (var httpClient = GetHttpClient())
            {
                var serversXml = httpClient.GetStringAsync(ServersUrl).GetAwaiter().GetResult();
                return serversXml.DeserializeFromXml<ServerList>().Servers ?? Array.Empty<Server>();
            }
        }

        private async Task<Int32> TestServerLatencyAsync(Server server, Int32 tests = 4)
        {
            this.SetStage(TestStage.Latency);

            if (String.IsNullOrWhiteSpace(server.Url))
            {
                throw new NullReferenceException("Server url was null");
            }

            var latencyUrl = GetBaseUrl(server.Url).Append("latency.txt");

            var stopwatch = new Stopwatch();

            using (var httpClient = GetHttpClient())
            {
                var test = 1;
                do
                {
                    stopwatch.Start();
                    var testString = await httpClient.GetStringAsync(latencyUrl);
                    stopwatch.Stop();

                    if (!testString.StartsWith("test=test"))
                    {
                        throw new InvalidOperationException("Server returned incorrect test string for latency.txt");
                    }
                    test++;
                } while (test < tests);

                return (Int32)stopwatch.ElapsedMilliseconds / tests;
            }
        }

        private Int32 TestServerLatency(Server server, Int32 tests = 4)
        {
            this.SetStage(TestStage.Latency);

            if (String.IsNullOrWhiteSpace(server.Url))
            {
                throw new NullReferenceException("Server url was null");
            }

            var latencyUrl = GetBaseUrl(server.Url).Append("latency.txt");

            var stopwatch = new Stopwatch();

            using (var httpClient = GetHttpClient())
            {
                var test = 1;
                do
                {
                    stopwatch.Start();
                    var testString = httpClient.GetStringAsync(latencyUrl).GetAwaiter().GetResult();
                    stopwatch.Stop();

                    if (!testString.StartsWith("test=test"))
                    {
                        throw new InvalidOperationException("Server returned incorrect test string for latency.txt");
                    }
                    test++;
                } while (test < tests);

                return (Int32)stopwatch.ElapsedMilliseconds / tests;
            }
        }

        private async Task<Double> TestUploadSpeedAsync(Server server, Int32 parallelUploads)
        {
            this.SetStage(TestStage.Upload);

            if (String.IsNullOrWhiteSpace(server.Url))
            {
                throw new NullReferenceException("Server url was null");
            }

            var testData = GenerateUploadData();

            return await this.TestSpeedAsync(testData, async (client, uploadData) =>
            {
                using (var content = new ByteArrayContent(uploadData))
                {
                    await client.PostAsync(server.Url, content).ConfigureAwait(false);
                    return uploadData.Length;
                }
            }, parallelUploads);
        }

        private async Task<Double> TestDownloadSpeedAsync(Server server, Int32 parallelDownloads)
        {
            this.SetStage(TestStage.Download);

            if (String.IsNullOrWhiteSpace(server.Url))
            {
                throw new NullReferenceException("Server url was null");
            }

            var testData = this.GenerateDownloadUrls(server.Url);

            return await this.TestSpeedAsync(testData, async (client, url) =>
            {
                var data = await client.GetStringAsync(url).ConfigureAwait(false);
                return data.Length;
            }, parallelDownloads);
        }

        private async Task<Double> TestSpeedAsync<T>(IEnumerable<T> testData,
            Func<HttpClient, T, Task<Int32>> doWork,
            Int32 parallelTasks)
        {
            var timer = new Stopwatch();
            var throttler = new SemaphoreSlim(parallelTasks);
            var progressTimer = new System.Timers.Timer(1000);

            timer.Start();
            Int64 totalBytesProcessed = 0;

            progressTimer.Elapsed += (sender, e) =>
            {
                var progressInfo = new ProgressInfo
                {
                    BytesProcessed = totalBytesProcessed,
                    Speed = this.ConvertUnit(totalBytesProcessed * 8.0 / 1024.0 /
                                            ((Double)timer.ElapsedMilliseconds / 1000)),
                    TotalBytes = 0 // You might want to set this to a meaningful value
                };

                ProgressChanged?.Invoke(this, progressInfo);
            };

            progressTimer.Start();

            var downloadTasks = testData.Select(async data =>
            {
                await throttler.WaitAsync().ConfigureAwait(false);
                using (var httpClient = GetHttpClient())
                {
                    try
                    {
                        var size = await doWork(httpClient, data).ConfigureAwait(false);

                        Interlocked.Add(ref totalBytesProcessed, size);
                        
                        return size;
                    }
                    finally
                    {
                        throttler.Release();
                    }
                }
            }).ToArray();

            await Task.WhenAll(downloadTasks);
            timer.Stop();

            Double totalSize = downloadTasks.Sum(task => task.Result);
            return this.ConvertUnit(totalSize * 8 / 1024 / ((Double)timer.ElapsedMilliseconds / 1000));
        }

        private static IEnumerable<Byte[]> GenerateUploadData()
        {
            var random = new Random();
            var result = new List<Byte[]>();

            for (var sizeCounter = 1; sizeCounter < MaxUploadSize + 1; sizeCounter++)
            {
                var size = sizeCounter * 200 * 1024;
                var builder = new StringBuilder(size);

                for (var i = 0; i < size; ++i)
                {
                    builder.Append(Chars[random.Next(Chars.Length)]);
                }

                var bytes = Encoding.UTF8.GetBytes(builder.ToString());

                for (var i = 0; i < 10; i++)
                {
                    result.Add(bytes);
                }
            }

            return result;
        }

        private Double ConvertUnit(Double value)
        {
            switch (this.SpeedUnit)
            {
                case SpeedUnit.Kbps:
                    return Math.Round(value);
                case SpeedUnit.KBps:
                    return Math.Round(value / 8.0);
                case SpeedUnit.Mbps:
                    return Math.Round(value / 1024.0);
                case SpeedUnit.MBps:
                    return Math.Round(value / 8192.0);
                default:
                    throw new InvalidEnumArgumentException("Not a valid SpeedUnit");
            }
        }

        private void SetStage(TestStage newStage)
        {
            if (this.CurrentStage == newStage)
            {
                return;
            }

            this.CurrentStage = newStage;
            StageChanged?.Invoke(this, newStage);
        }

        private IEnumerable<String> GenerateDownloadUrls(String serverUrl)
        {
            var downloadUrl = GetBaseUrl(serverUrl).Append("random{0}x{0}.jpg?r={1}");
            foreach (var downloadSize in DownloadSizes)
            {
                for (var i = 0; i < 4; i++)
                {
                    yield return String.Format(downloadUrl, downloadSize, i);
                }
            }
        }

        private static String GetBaseUrl(String url) => new Uri(new Uri(url), ".").OriginalString;

        private static HttpClient GetHttpClient()
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/112.0.0.0 Safari/537.36");
            httpClient.DefaultRequestHeaders.Accept.ParseAdd("text/html, application/xhtml+xml, */*");
            httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            return httpClient;
        }
    }
}
