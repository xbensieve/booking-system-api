using Booking.Repository.Interfaces;
using Booking.Repository.Models;
using Booking.Service.Interfaces;
using Booking.Service.Models;

namespace Booking.Service.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        public AuthService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<object>> GetUserByIdAsync(string uid)
        {
            if (string.IsNullOrEmpty(uid))
                return ApiResponse<object>.Fail("User ID is required.");

            var user = await _unitOfWork.Users.GetByIdAsync(uid);

            if (user == null)
                return ApiResponse<object>.Fail("User not found.", null);

            return ApiResponse<object>.Ok(user, "User retrieved successfully.");
        }

        public async Task<ApiResponse<object>> RegisterAsync(RegisterModel model)
        {
            if (model == null)
                return ApiResponse<object>.Fail("Invalid user data.");


            var user = new User
            {
                Uid = model.Uid,
                Email = model.Email,
                Name = model.FullName,
                AvatarUrl = model.AvatarUrl,
                AuthProvider = "Firebase",
            };

            try
            {
                await _unitOfWork.Users.AddAsync(user);
                int result = await _unitOfWork.SaveChangesAsync();

                if (result > 0)
                {
                    return ApiResponse<object>.Ok(new
                    {
                        Message = "User registered successfully",
                        User = new { user.Uid, user.Email, user.Name, user.PhoneNumber }
                    });
                }
                else
                {
                    return ApiResponse<object>.Fail("Failed to register user.");
                }
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.Fail($"An error occurred while registering the user: {ex.Message}");
            }
        }
    }
}
