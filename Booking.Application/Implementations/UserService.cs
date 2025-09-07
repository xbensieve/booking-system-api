using AutoMapper;
using Booking.Application.DTOs.Auth;
using Booking.Application.DTOs.Common;
using Booking.Application.Interfaces;
using Booking.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Booking.Application.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public UserService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<ApiResponse<List<UserResponse>>> GetAllUsersAsync(int page, int pageSize)
        {
            var query = _unitOfWork.Users.Query().AsQueryable();
            int total = await query.CountAsync();
            var users = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            var mapped = _mapper.Map<List<UserResponse>>(users);
            return ApiResponse<List<UserResponse>>.Ok(mapped, $"Found {total} users.");
        }
        public async Task<ApiResponse<object>> DeleteUserAsync(Guid userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<object>.Fail("User not found.");
            }
            _unitOfWork.Users.Delete(user);
            await _unitOfWork.SaveChangesAsync();
            return ApiResponse<object>.Ok(null, "User deleted successfully.");
        }
        public async Task<ApiResponse<UserResponse>> UpdateUserAsync(Guid userId, DTOs.User.UpdateUserRequest request)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<UserResponse>.Fail("User not found.");
            }
            user.Name = request.Name ?? user.Name;
            user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;
            user.AvatarUrl = request.AvatarUrl ?? user.AvatarUrl;
            user.IsActive = request.IsActive;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();
            var mapped = _mapper.Map<UserResponse>(user);
            return ApiResponse<UserResponse>.Ok(mapped, "User updated successfully.");
        }
    }
}
