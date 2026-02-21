namespace ZenAssignment.API.Interface
{
    public interface IImageRepo
    {
        Task<string> UploadImageAsync(IFormFile file);
    }
}
