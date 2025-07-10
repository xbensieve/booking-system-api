using AutoMapper;
using Booking.Repository.Models;
using Booking.Service.Models;

namespace Booking.Service.Mapping
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
        }
    }
}
