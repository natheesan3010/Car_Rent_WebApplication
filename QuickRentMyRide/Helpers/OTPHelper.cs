using System;

namespace QuickRentMyRide.Helpers
{
    public static class OTPHelper
    {
        private static Random _random = new Random();

        // 6-digit OTP generate
        public static string GenerateOTP()
        {
            return _random.Next(100000, 999999).ToString();
        }

        // Check if OTP is expired (5 minutes validity)
        public static bool IsOTPExpired(DateTime otpGeneratedAt)
        {
            return DateTime.Now > otpGeneratedAt.AddMinutes(5);
        }
    }
}
