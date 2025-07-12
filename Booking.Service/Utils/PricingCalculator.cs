using Booking.Service.Models;

namespace Booking.Service.Utils
{
    public class PricingCalculator
    {
        public PricingBreakdown CalculateDetailedRoomPrice(
            DateTime actualCheckIn,
            DateTime expectedCheckIn,
            DateTime actualCheckOut,
            DateTime expectedCheckOut,
            decimal baseRatePerNight)
        {
            var result = new PricingBreakdown();

            result.Nights = (expectedCheckOut.Date - expectedCheckIn.Date).Days;
            result.BasePrice = baseRatePerNight * result.Nights;

            if (actualCheckIn.Date == expectedCheckIn.Date && actualCheckIn.TimeOfDay < expectedCheckIn.TimeOfDay)
            {
                var hour = actualCheckIn.TimeOfDay.Hours;
                if (hour >= 5 && hour < 9)
                    result.EarlyCheckInFee = baseRatePerNight * 0.5m;
                else if (hour >= 9 && hour < 14)
                    result.EarlyCheckInFee = baseRatePerNight * 0.3m;
            }

            if (actualCheckOut.Date == expectedCheckOut.Date && actualCheckOut.TimeOfDay > expectedCheckOut.TimeOfDay)
            {
                var hour = actualCheckOut.TimeOfDay.Hours;
                if (hour >= 11 && hour < 15)
                    result.LateCheckOutFee = baseRatePerNight * 0.3m;
                else if (hour >= 15 && hour < 18)
                    result.LateCheckOutFee = baseRatePerNight * 0.5m;
                else if (hour >= 18)
                    result.LateCheckOutFee = baseRatePerNight * 1.0m;
            }

            return result;
        }
    }
}
