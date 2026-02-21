using ZenAssignment.API.Interface;
using Azure.Storage.Blobs;
using Microsoft.ApplicationInsights;


namespace ZenAssignment.API.Repository
{
    public class ImageRepo : IImageRepo
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;
        private readonly TelemetryClient _telemetryClient;
        public ImageRepo(BlobServiceClient blobServiceClient, IConfiguration configuration, TelemetryClient telemetryClient)
        {
            _blobServiceClient = blobServiceClient;
            _containerName = configuration["AzureBlobStorage:ContainerName"];
            _telemetryClient = telemetryClient;
        }
        public async Task<string> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                _telemetryClient.TrackTrace("Image file is empty");
                throw new ArgumentNullException($"{nameof(file)} is empty");
            }

            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync();

            _telemetryClient.TrackTrace("Blob service container client found and container created if not exists");

            var blobClient = containerClient.GetBlobClient($"{file.Name}");

            _telemetryClient.TrackTrace("Blob client created");

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }

            _telemetryClient.TrackMetric("ImageUploadSizeMB", file.Length / 1024.0);
            _telemetryClient.TrackMetric("ImageUploadedCount", 1);
            _telemetryClient.TrackMetric($"ImageUploadedIn{_containerName}", 1);

            _telemetryClient.TrackTrace($"Image uploaded successfully to blob storage. Image Path: {blobClient.Uri.ToString()}");

            return blobClient.Uri.ToString();
        }
    }
}
