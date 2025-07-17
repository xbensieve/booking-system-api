using Booking.Application.DTOs.ML;

namespace Booking.Application.Interfaces
{
    public interface IMLService
    {
        HotelPrediction Predict(SearchData input);
    }
}
