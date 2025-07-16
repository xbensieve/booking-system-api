namespace Booking.Domain.Enums
{
    public enum VnPayResponseCode
    {
        /// <summary>Giao dịch thành công</summary>
        Success = 00,

        /// <summary>Giao dịch chưa hoàn tất</summary>
        Incomplete = 01,

        /// <summary>Giao dịch bị lỗi</summary>
        Failed = 02,

        /// <summary>Giao dịch đảo (Khách hàng bị trừ tiền tại ngân hàng nhưng GD chưa thành công ở VNPAY)</summary>
        Reversed = 04,

        /// <summary>VNPAY đang xử lý giao dịch này (Giao dịch hoàn tiền)</summary>
        Processing = 05,

        /// <summary>VNPAY đã gửi yêu cầu hoàn tiền sang Ngân hàng</summary>
        RefundRequested = 06,

        /// <summary>Giao dịch bị nghi ngờ gian lận</summary>
        FraudSuspicion = 07,

        /// <summary>Giao dịch hoàn trả bị từ chối</summary>
        RefundDenied = 09
    }
}
