using Booking.Service.Interfaces;
using Booking.Service.Models;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Booking.Api.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api")]
    [ApiController]
    public class HotelImageController : ControllerBase
    {
        private readonly Cloudinary _cloudinary;
        private readonly IHotelImageService _hotelImageService;
        public HotelImageController(Cloudinary cloudinary, IHotelImageService hotelImageService)
        {
            _cloudinary = cloudinary;
            _hotelImageService = hotelImageService;
        }

        [HttpPost("hotels/{id}/images/upload")]
        public async Task<IActionResult> Post(int id, [Required] List<IFormFile> images)
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
                    Folder = $"hotels/{id}",
                    PublicId = $"hotel_{id}_{Guid.NewGuid()}"
                };
                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                    throw new Exception($"Failed to upload image {image.FileName}");

                return new HotelImageRequest
                {
                    HotelId = id,
                    ImageUrl = uploadResult.SecureUrl.ToString(),
                    IsMain = false
                };
            });

            List<HotelImageRequest> uploadedImages;
            try
            {
                uploadedImages = (await Task.WhenAll(uploadTasks)).ToList();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

            var response = await _hotelImageService.AddHotelImagesAsync(id, uploadedImages);
            return response.Success ? Ok(response) : BadRequest(response.Message);
        }
        [HttpDelete("hotels/images/{imageId}")]
        public async Task<IActionResult> DeleteImage(int imageId)
        {
            var image = await _hotelImageService.GetHotelImageByIdAsync(imageId);

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

            var response = await _hotelImageService.DeleteHotelImageAsync(imageId);

            return response.Success
                ? Ok(response)
                : BadRequest(response.Message);

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

        [HttpGet("hotels/images/{imageId}")]
        public async Task<IActionResult> GetImageById(int imageId)
        {
            var response = await _hotelImageService.GetHotelImageByIdAsync(imageId);
            if (response.Success)
            {
                return Ok(response);
            }
            return NotFound(new { message = response.Message });
        }
        [HttpPut("hotels/images/{imageId}/upload")]
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
                    Folder = $"hotels/{imageId}",
                    PublicId = $"hotel_image_{imageId}_{Guid.NewGuid()}"
                };
                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                    return StatusCode(500, new { message = $"Failed to upload image {image.FileName}" });

                newImageUrl = uploadResult.SecureUrl.ToString();
            }

            var response = await _hotelImageService.UpdateHotelImageFileAsync(imageId, newImageUrl, isMain);
            return response.Success ? Ok(response) : BadRequest(response.Message);
        }
    }
}
