using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Rest;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureIoTFunction
{
    public static class BlobFunction
    {
        public static string FaceApiKey = "hidden";
        public static string BaseUri = "https://canadacentral.api.cognitive.microsoft.com/face/v1.0";

        [FunctionName("BlobFunction")]
        public static void Run([BlobTrigger("azureiot/{name}", Connection = "AzureWebJobsDashboard"), Disable]CloudBlockBlob myBlob, string name, TraceWriter log)
        {
            log.Info($"C# Blob trigger function Processed blob\n Name:{name} \n Uri: {myBlob.Uri} ");

            FaceClient client =
                new FaceClient(new ApiKeyServiceClientCredentials(FaceApiKey))
                {
                    BaseUri = new Uri(BaseUri)
                };


            string uri = myBlob.Uri.AbsoluteUri;

            log.Info($"Image Uri: {uri} ");
            try
            {
                HttpOperationResponse<IList<DetectedFace>> response =
                    client.Face.DetectWithUrlWithHttpMessagesAsync(
                    uri,
                    false,
                    false,
                    new List<FaceAttributeType>
                    {
                        FaceAttributeType.Age
                    }).Result;
                if (response.Body.Count > 0)
                {
                    Double age = response.Body[0].FaceAttributes.Age.GetValueOrDefault(0d);
                    log.Info($"Found face: Age is {age}");
                }
                else
                {
                    log.Info($"No Face found!");
                }
            }
            catch (Exception e)
            {
                log.Info($"{e.Message}");
            }
        }
    }
}
