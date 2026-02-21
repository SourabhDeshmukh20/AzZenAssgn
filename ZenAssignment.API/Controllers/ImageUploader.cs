using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZenAssignment.API.Interface;

namespace ZenAssignment.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageUploader : ControllerBase
    {
        private IImageRepo _imageRepo;
        private TelemetryClient _telemetryClient;
        public ImageUploader(IImageRepo imageRepo, TelemetryClient telemetryClient)
        {
            _imageRepo = imageRepo;
            _telemetryClient = telemetryClient;
        }

        [HttpPost]
        [Route("UploadImage")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            using (var operation = _telemetryClient.StartOperation<RequestTelemetry>("ImageUploadOperation"))
            {
                operation.Telemetry.Id = Guid.NewGuid().ToString();

                try
                {
                    _telemetryClient.TrackTrace($"Image Upload process started at "+DateTime.Now);
                    var url = await _imageRepo.UploadImageAsync(file);
                    _telemetryClient.TrackTrace($"Image upload process completed successfully at" + DateTime.Now);
                    return Ok(new { Url = url });
                }
                catch (Exception ex)
                {
                    _telemetryClient.TrackException(ex);
                    return BadRequest(new { Error = ex.Message });
                }
            }
        }
    }
}
