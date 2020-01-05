using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Dispatcher.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace Dispatcher
{
    public static class ProcessQueueMessage
    {
        static private CloudStorageAccount storageAccount;

        [FunctionName("ProcessQueueMessage")]
        public static async Task Run([QueueTrigger("central", Connection = "StorageConnection")] string queueItem, ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {queueItem}");
            var item = JsonConvert.DeserializeObject<CentralMessage>(queueItem);
            var storageConnection = Environment.GetEnvironmentVariable("StorageConnection");
            storageAccount = CloudStorageAccount.Parse(storageConnection);

            var blobUrl = await ConvertTextToVoiceAndUploadToBlob(item.Message, log);

            var queueClient = storageAccount.CreateCloudQueueClient();

            foreach (var location in item.Locations)
            {
                var queueReference = queueClient.GetQueueReference(location);
                await queueReference.AddMessageAsync(new CloudQueueMessage
                (
                    JsonConvert.SerializeObject(new AudioMessage()
                    {
                        VoiceURL = blobUrl
                    })
                ));
            }
        }

        private static async Task<string> ConvertTextToVoiceAndUploadToBlob(string messageToConvert, ILogger log)
        {
            try
            {
                var cognitiveServiceKey = Environment.GetEnvironmentVariable("CognitiveServiceKey");
                var cognitiveServiceRegion = Environment.GetEnvironmentVariable("CognitiveServiceRegion");
                var cognitiveServiceName = Environment.GetEnvironmentVariable("CognitiveServiceName");
                string accessToken = await GetAccessTokenAsync(
                    cognitiveServiceKey, 
                    $"https://{cognitiveServiceRegion}.api.cognitive.microsoft.com/sts/v1.0/issueToken");

                string host = $"https://{cognitiveServiceRegion}.tts.speech.microsoft.com/cognitiveservices/v1";

                string body = $@"<speak version='1.0' 
                    xmlns='https://www.w3.org/2001/10/synthesis' xml:lang='en-US'>
                    <voice xml:lang='en-US' xml:gender='Female' name='en-US-JessaRUS'>
                    {messageToConvert}</voice></speak>";

                using (var client = new HttpClient())
                {
                    using (var request = new HttpRequestMessage())
                    {
                        request.Method = HttpMethod.Post;
                        request.RequestUri = new Uri(host);
                        request.Content = new StringContent(body, Encoding.UTF8, "application/ssml+xml");
                        request.Headers.Add("Authorization", "Bearer " + accessToken);
                        request.Headers.Add("Connection", "Keep-Alive");
                        request.Headers.Add("User-Agent", cognitiveServiceName);
                        request.Headers.Add("X-Microsoft-OutputFormat", "riff-24khz-16bit-mono-pcm");
                        using (var response = await client.SendAsync(request).ConfigureAwait(false))
                        {
                            response.EnsureSuccessStatusCode();
                            var audioResult = await response.Content.ReadAsStreamAsync();
                            string fileName = Guid.NewGuid() + "-" + DateTime.Now.ToString("ddMMyyyymmss") + ".mp3";
                            var blobUrl = await SaveToBlob(audioResult, fileName);
                            return blobUrl;                        
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError("Error occured: " + ex.Message);
                return null;
            }
        }

        private static async Task<string> GetAccessTokenAsync(string _subscriptionKey, string _tokenFetchUri)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);
                UriBuilder uriBuilder = new UriBuilder(_tokenFetchUri);

                var result = await client.PostAsync(uriBuilder.Uri.AbsoluteUri, null).ConfigureAwait(false);
                return await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
        }

        private static async Task<string> SaveToBlob(Stream audio, string fileName)
        {
            var blobClient = storageAccount.CreateCloudBlobClient();
            var blockReference = blobClient.GetContainerReference("audio").GetBlockBlobReference(fileName);
            await blockReference.UploadFromStreamAsync(audio);
            string sasBlobToken;
  
                SharedAccessBlobPolicy adhocSAS = new SharedAccessBlobPolicy()
                {
                    // Add an hour expiry.
                    SharedAccessExpiryTime = DateTime.UtcNow.AddHours(1),
                    Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.Write | SharedAccessBlobPermissions.Create,
                };

                // Generate the shared access signature on the blob, setting the constraints directly on the signature.
                sasBlobToken = blockReference.GetSharedAccessSignature(adhocSAS);
            

            // Return the URI string for the container, including the SAS token.
            return blockReference.Uri + sasBlobToken;
        }
    }

}
