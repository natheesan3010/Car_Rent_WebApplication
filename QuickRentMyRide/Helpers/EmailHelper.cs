using System.Net;
using System.Net.Mail;

namespace QuickRentMyRide.Helpers
{
    public static class EmailHelper
    {
        private static string FromEmail = "yourgmail@gmail.com"; // உங்கள் Gmail
        private static string Password = "yourapppassword";      // Gmail App Password

        public static void SendOTP(string toEmail, string otp)
        {
            var message = new MailMessage(FromEmail, toEmail);
            message.Subject = "Your Booking OTP";
            message.Body = $"Your OTP for booking confirmation is: {otp}. Valid for 5 minutes.";

            using var client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(FromEmail, Password),
                EnableSsl = true
            };

            client.Send(message);
        }

        public static void SendBookingApproved(string toEmail, int bookingId)
        {
            var message = new MailMessage(FromEmail, toEmail);
            message.Subject = "Booking Approved";
            message.Body = $"Your booking #{bookingId} has been approved by admin. Thank you!";

            using var client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(FromEmail, Password),
                EnableSsl = true
            };

            client.Send(message);
        }

        public static void SendBookingRejected(string toEmail, int bookingId)
        {
            var message = new MailMessage(FromEmail, toEmail);
            message.Subject = "Booking Rejected";
            message.Body = $"Your booking #{bookingId} has been rejected by admin.";

            using var client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(FromEmail, Password),
                EnableSsl = true
            };

            client.Send(message);
        }
    }
}
