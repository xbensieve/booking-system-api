using AutoMapper;
using Booking.Application.DTOs.Auth;
using Booking.Application.DTOs.Hotel;
using Booking.Application.DTOs.Reservation;
using Booking.Application.DTOs.Room;
using Booking.Domain.Entities;

namespace Booking.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Hotel, HotelResponse>();
            CreateMap<HotelImage, HotelImageResponse>();
            CreateMap<Room, RoomResponse>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));
            CreateMap<RoomImage, RoomImageResponse>();
            CreateMap<Reservation, ReservationResponse>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
                .ForMember(dest => dest.Room, opt => opt.MapFrom(src => src.Room));
            CreateMap<User, UserResponse>();
        }
    }
}
