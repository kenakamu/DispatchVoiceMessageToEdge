namespace PlayMessage
{
    using System;
    using System.Diagnostics;
    using System.Runtime.Loader;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Azure.Devices.Client.Transport.Mqtt;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Newtonsoft.Json;
    using Microsoft.WindowsAzure.Storage.RetryPolicies;

    class Program
    {
        static string storageConnectionString;
        static string queueName;
        static CloudQueue queue;

        static void Main(string[] args)
        {
            Init().Wait();

            // Wait until the app unloads or is cancelled
            var cts = new CancellationTokenSource();
            AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
            Console.CancelKeyPress += (sender, cpe) => cts.Cancel();
            WhenCancelled(cts.Token).Wait();
        }

        /// <summary>
        /// Handles cleanup operations when app is cancelled or unloads
        /// </summary>
        public static Task WhenCancelled(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            return tcs.Task;
        }

        /// <summary>
        /// Initializes the ModuleClient and sets up the callback to update desiredproperty changed
        /// </summary>
        static async Task Init()
        {
            MqttTransportSettings mqttSetting = new MqttTransportSettings(TransportType.Mqtt_Tcp_Only);
            ITransportSettings[] settings = { mqttSetting };

            // Open a connection to the Edge runtime
            ModuleClient ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
            await ioTHubModuleClient.OpenAsync();
            Console.WriteLine("IoT Hub module client initialized.");

            var moduleTwin = await ioTHubModuleClient.GetTwinAsync();
            Console.WriteLine(JsonConvert.SerializeObject(moduleTwin));
            await ioTHubModuleClient.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertiesUpdate, null);

            // Call OnDesiredPropertesUpdate to read the default value from config.
            await OnDesiredPropertiesUpdate(moduleTwin.Properties.Desired, ioTHubModuleClient);
            await DoWork();
        }

        private static async Task DoWork()
        {
            while (queue == null)
            {
                await Task.Delay(1000);
            }

            while (true)
            {
                var message = await queue.GetMessageAsync();

                if (message != null)
                {
                    try
                    {
                        var audioMessage = JsonConvert.DeserializeObject<AudioMessage>(message.AsString);
                        Console.WriteLine($"Voice message at {audioMessage.VoiceURL}");

                        var p = new Process()
                        {
                            StartInfo = new ProcessStartInfo()
                            {
                                FileName = "/usr/bin/mplayer",
                                Arguments = $"-msglevel all=1 -quiet -af volume=5 scaletempo=speed=tempo -speed 1.0 {audioMessage.VoiceURL}"
                            }
                        };

                        var processResult = p.Start();
                        Console.WriteLine(processResult);
                        await queue.DeleteMessageAsync(message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        await queue.DeleteMessageAsync(message);
                    }
                }

                await Task.Delay(1000);
            }
        }

        private static void InitializeQueue()
        {
            if(string.IsNullOrEmpty(storageConnectionString) || string.IsNullOrEmpty(queueName))
            {
                return;
            }

            try
            {
                QueueRequestOptions interactiveRequestOption = new QueueRequestOptions()
                {
                    RetryPolicy = new ExponentialRetry(TimeSpan.FromMilliseconds(500), 3)
                };
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
                CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
                queueClient.DefaultRequestOptions = interactiveRequestOption;
                queue = queueClient.GetQueueReference(queueName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        static Task OnDesiredPropertiesUpdate(TwinCollection desiredProperties, object userContext)
        {
            try
            {
                Console.WriteLine("Desired property change: " + JsonConvert.SerializeObject(desiredProperties));
                
                if (desiredProperties.Contains("StorageConnectionString"))
                {
                    storageConnectionString = desiredProperties["StorageConnectionString"];
                }

                if (desiredProperties.Contains("QueueName"))
                {
                    queueName = desiredProperties["QueueName"];
                }

                if (!string.IsNullOrEmpty(storageConnectionString) && !string.IsNullOrEmpty(queueName))
                {
                    InitializeQueue();
                }
            }
            catch (AggregateException ex)
            {
                foreach (Exception exception in ex.InnerExceptions)
                {
                    Console.WriteLine();
                    Console.WriteLine("Error when receiving desired property: {0}", exception);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Error when receiving desired property: {0}", ex.Message);
            }
            return Task.CompletedTask;
        }      
    }
}
