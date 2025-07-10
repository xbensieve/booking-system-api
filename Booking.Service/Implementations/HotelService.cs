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
    }
}
