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

            if (actualCheckIn < expectedCheckIn)
            {
                var hoursEarly = (expectedCheckIn - actualCheckIn).TotalHours;

                if (hoursEarly >= 5 && hoursEarly <= 9)
                    result.EarlyCheckInFee = baseRatePerNight * 0.5m;
                else if (hoursEarly >= 1 && hoursEarly < 5)
                    result.EarlyCheckInFee = baseRatePerNight * 0.3m;
            }

            if (actualCheckOut > expectedCheckOut)
            {
                var hoursLate = (actualCheckOut - expectedCheckOut).TotalHours;

                if (hoursLate >= 0 && hoursLate < 4)
                    result.LateCheckOutFee = baseRatePerNight * 0.3m;
                else if (hoursLate >= 4 && hoursLate < 7)
                    result.LateCheckOutFee = baseRatePerNight * 0.5m;
                else if (hoursLate >= 7)
                    result.LateCheckOutFee = baseRatePerNight * 1.0m;
            }


            return result;
        }
    }
}
