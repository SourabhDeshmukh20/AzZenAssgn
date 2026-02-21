using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace ZenAssignment.Func
{
    public class BlobTrigger
    {
        [FunctionName("BlobTrigger")]
        public void Run([BlobTrigger("azzenassignment/{name}", Connection = "StorageConnection")]Stream myBlob, string name, ILogger log, 
                        [Blob("azimagestorage/{name}-metadata.json", FileAccess.Write, Connection = "StorageConnection")] Stream outputBlob)
        {
            try
            {
                log.LogInformation($"Blob trigger function processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

                // Generate metadata
                var metadata = new Dictionary<string, object>
                {
                    { "FileName", name },
                    { "SizeInBytes", myBlob.Length },
                    { "CreatedOn", System.DateTime.UtcNow },
                    { "ContentType", GetContentType(name) }
                };

                // Serialize metadata to JSON
                var json = JsonConvert.SerializeObject(metadata, Formatting.Indented);

                using var writer = new StreamWriter(outputBlob);
                writer.Write(json);

            }
            catch (Exception ex)
            {
                log.LogError($"An eError occured while processing blob {name}: {ex.Message}");
                throw;
            }
        }

        private static string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".txt" => "text/plain",
                ".json" => "application/json",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };
        }
    }
}
