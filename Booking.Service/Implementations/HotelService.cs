using AutoMapper;
using Booking.Repository.Interfaces;
using Booking.Repository.Models;
using Booking.Service.Interfaces;
using Booking.Service.Models;
using Microsoft.EntityFrameworkCore;

namespace Booking.Service.Implementations
{
    public class HotelService : IHotelService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public HotelService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<ApiResponse<object>> CreateHotelAsync(HotelRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Hotel request cannot be null.");
            }

            var hotel = new Hotel
            {
                Name = request.Name,
                Address = request.Address,
                City = request.City,
                Country = request.Country,
                Description = request.Description,
            };

            try
            {
                await _unitOfWork.Hotels.AddAsync(hotel);
                int result = await _unitOfWork.SaveChangesAsync();

                if (result > 0)
                {
                    return ApiResponse<object>.Ok(new { hotelId = hotel.Id }, "Hotel created successfully.");
                }
                else
                {
                    return ApiResponse<object>.Fail("Failed to create hotel. Please try again.");
                }
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.Fail("An error occurred while creating the hotel.", ex.Message);
            }
        }

        public async Task<ApiResponse<object>> DeleteHotelAsync(int id)
        {
            var hotel = await _unitOfWork.Hotels.GetByIdAsync(id);
            if (hotel == null)
            {
                return ApiResponse<object>.Fail("Hotel not found.");
            }

            try
            {
                hotel.IsDeleted = true;
                var result = await _unitOfWork.SaveChangesAsync();

                if (result > 0)
                {
                    return ApiResponse<object>.Ok(new { hotelId = hotel.Id }, "Hotel deleted successfully.");
                }
                else
                {
                    return ApiResponse<object>.Fail("Failed to delete hotel. Please try again.");
                }
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.Fail("An error occurred while deleting the hotel.", ex.Message);
            }
        }

        public async Task<ApiResponse<List<HotelResponse>>> GetAllHotelsAsync(int page = 1, int pageSize = 10)
        {
            var hotelsQuery = _unitOfWork.Hotels.Query().Include(h => h.Images).Include(h => h.Rooms);
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            var totalHotels = hotelsQuery.Count();
            var totalPages = (int)Math.Ceiling((double)totalHotels / pageSize);
            var hotels = await hotelsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var hotelResponses = _mapper.Map<List<HotelResponse>>(hotels);

            return ApiResponse<List<HotelResponse>>.Ok(hotelResponses, "Hotels retrieved successfully.");
        }

        public async Task<ApiResponse<HotelResponse>> GetHotelByIdAsync(int id)
        {
            var hotel = await _unitOfWork.Hotels.Query().Include(h => h.Images).Include(h => h.Rooms).FirstOrDefaultAsync();
            if (hotel == null)
            {
                return ApiResponse<HotelResponse>.Fail("Hotel not found.");
            }

            var hotelResponse = _mapper.Map<HotelResponse>(hotel);

            return ApiResponse<HotelResponse>.Ok(hotelResponse, "Hotel retrieved successfully.");
        }

        public async Task<ApiResponse<object>> UpdateHotelAsync(int id, HotelRequest request)
        {
            if (request == null)
            {
                return ApiResponse<object>.Fail("Hotel request cannot be null.");
            }

            var hotel = await _unitOfWork.Hotels.GetByIdAsync(id);
            if (hotel == null)
            {
                return ApiResponse<object>.Fail("Hotel not found.");
            }

            hotel.Name = request.Name?.Trim() ?? hotel.Name;
            hotel.Address = request.Address?.Trim() ?? hotel.Address;
            hotel.City = request.City?.Trim() ?? hotel.City;
            hotel.Country = request.Country?.Trim() ?? hotel.Country;
            hotel.Description = request.Description?.Trim() ?? hotel.Description;
            hotel.UpdatedAt = DateTime.UtcNow;

            try
            {
                _unitOfWork.Hotels.Update(hotel);
                var result = await _unitOfWork.SaveChangesAsync();

                return result > 0
                    ? ApiResponse<object>.Ok(new { hotelId = hotel.Id }, "Hotel updated successfully.")
                    : ApiResponse<object>.Fail("Failed to update hotel.");
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.Fail("An error occurred while updating the hotel.", ex.Message);
            }
        }
    }
}
