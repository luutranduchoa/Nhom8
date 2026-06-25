using Microsoft.AspNetCore.Http;

namespace Medicine.DTOs
{
    // Wrapper DTO for file upload so Swagger/Swashbuckle can generate proper form schema
    public class ImageUploadRequest
    {
        public IFormFile? File { get; set; }
    }
}
