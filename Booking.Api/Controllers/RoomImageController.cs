using Booking.Service.Interfaces;
using Booking.Service.Models;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Booking.Api.Controllers
{
    [Route("api/rooms")]
    [ApiController]
    public class RoomImageController : ControllerBase
    {
        private readonly Cloudinary _cloudinary;
        private readonly IRoomImageService _roomImageService;
        public RoomImageController(Cloudinary cloudinary, IRoomImageService roomImageService)
        {
            _cloudinary = cloudinary ?? throw new ArgumentNullException(nameof(cloudinary));
            _roomImageService = roomImageService ?? throw new ArgumentNullException(nameof(roomImageService));
        }
        [HttpPost("{roomId}/images/upload")]
        public async Task<IActionResult> UploadImages(int roomId, [FromForm] List<IFormFile> images)
        {
            if (images == null || !images.Any())
            {
                return BadRequest(new { message = "No images provided" });
            }
            if (images.Count > 10)
            {
                return BadRequest(new { message = "Cannot upload more than 10 images at once" });
            }
            var uploadTasks = images.Select(async image =>
            {
                if (image.Length > 5 * 1024 * 1024) throw new ArgumentException($"Image {image.FileName} exceeds 5MB limit");
                if (!IsImageFile(image)) throw new ArgumentException($"File {image.FileName} is not a valid image");

                using var stream = image.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(image.FileName, stream),
                    Folder = $"rooms/{roomId}",
                    PublicId = $"rooms_{roomId}_{Guid.NewGuid()}"
                };
                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                    throw new Exception($"Failed to upload image {image.FileName}");

                return new RoomImageRequest
                {
                    RoomId = roomId,
                    ImageUrl = uploadResult.SecureUrl.ToString(),
                    IsMain = false
                };
            });

            List<RoomImageRequest> uploadedImages;
            try
            {
                uploadedImages = (await Task.WhenAll(uploadTasks)).ToList();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

            var response = await _roomImageService.AddRoomImagesAsync(roomId, uploadedImages);
            return response.Success ? Ok(response) : BadRequest(response.Message);
        }
        private bool IsImageFile(IFormFile file)
        {
            var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            return validExtensions.Contains(extension);
        }
        private string ExtractPublicIdFromUrl(string imageUrl)
        {
            try
            {
                var uri = new Uri(imageUrl);
                var segments = uri.Segments;

                int versionIndex = Array.FindIndex(segments, s => s.StartsWith("v"));
                if (versionIndex < 0 || versionIndex >= segments.Length - 1) return null;

                var pathSegments = segments.Skip(versionIndex + 1).Take(segments.Length - versionIndex - 1).ToArray();
                var fullPath = string.Join("", pathSegments).TrimEnd('/');

                var publicIdWithExtension = pathSegments.Last().Split('?')[0];
                var publicId = publicIdWithExtension.Contains('.')
                    ? publicIdWithExtension.Substring(0, publicIdWithExtension.LastIndexOf('.'))
                    : publicIdWithExtension;

                return fullPath.Replace(publicIdWithExtension, publicId);
            }
            catch (Exception)
            {
                return null;
            }
        }
        [HttpGet("images/{imageId}")]
        public async Task<IActionResult> GetImageById(int imageId)
        {
            var response = await _roomImageService.GetRoomImageByIdAsync(imageId);

            return response.Success
                ? Ok(response)
                : NotFound(response.Message);
        }
        [HttpPut("images/{imageId}/upload")]
        public async Task<IActionResult> UpdateImageFile(int imageId, [Required] IFormFile image, [FromQuery] bool? isMain = null)
        {
            if (image == null || image.Length == 0)
                return BadRequest(new { message = "No image provided" });
            if (image.Length > 5 * 1024 * 1024)
                return BadRequest(new { message = "Image exceeds 5MB limit" });
            if (!IsImageFile(image))
                return BadRequest(new { message = "File is not a valid image" });
            string newImageUrl;
            using (var stream = image.OpenReadStream())
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(image.FileName, stream),
                    Folder = $"rooms/{imageId}",
                    PublicId = $"room_image_{imageId}_{Guid.NewGuid()}"
                };
                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                    return BadRequest(new { message = "Failed to upload image" });
                newImageUrl = uploadResult.SecureUrl.ToString();
            }
            var response = await _roomImageService.UpdateRoomImageFileAsync(imageId, newImageUrl, isMain);
            return response.Success ? Ok(response) : BadRequest(response.Message);
        }
        [HttpDelete("images/{imageId}")]
        public async Task<IActionResult> DeleteImage(int imageId)
        {
            var image = await _roomImageService.GetRoomImageByIdAsync(imageId);

            if (image == null)
            {
                return NotFound(new { message = "Image not found" });
            }

            var publicId = ExtractPublicIdFromUrl(image.Data.ImageUrl);
            if (string.IsNullOrEmpty(publicId))
            {
                return BadRequest(new { message = "Invalid image URL format" });
            }

            var deletionParams = new DeletionParams(publicId);
            var deletionResult = await _cloudinary.DestroyAsync(deletionParams);

            if (deletionResult.Result != "ok")
            {
                return StatusCode(500, new { message = "Failed to delete image from Cloudinary" });
            }

            var response = await _roomImageService.DeleteRoomImageAsync(imageId);
            return response.Success
                ? Ok(response)
                : BadRequest(response.Message);
        }
    }
}
