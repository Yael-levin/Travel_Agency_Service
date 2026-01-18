using System.Net;
using System.Net.Mail;
using Travel_Agency_Service.Models;

namespace Travel_Agency_Service.Services
{
    public class EmailService
    {
        // =========================
        // 💳 PAYMENT CONFIRMATION
        // =========================
        public void SendPaymentEmail(string toEmail, Booking booking, byte[] paymentPdf)
        {
            var smtp = CreateSmtpClient();

            string subject = "Payment Confirmation – Travel Agency";

            string body = $@"
                <html>
                <body style=""margin:0;padding:0;background:#f4f6f8;
                font-family: Calibri, 'Segoe UI', Helvetica, Arial, sans-serif;"">

                <table width='100%' cellpadding='0' cellspacing='0'>
                <tr>
                <td align='center'>

                <table width='600' style='background:#ffffff;
                margin:30px 0;
                border-radius:12px;
                overflow:hidden;
                box-shadow:0 4px 12px rgba(0,0,0,0.1);'>

                <!-- IMAGE -->
                <tr>
                <td>
                <img src=""{booking.Trip.ImageUrl}""
                     style=""width:100%;max-height:240px;object-fit:cover;display:block;"" />
                </td>
                </tr>

                <!-- CONTENT -->
                <tr>
                <td style='padding:30px;color:#212529;'>

                <h2 style='margin-top:0;color:#0d6efd;'>
                Payment Completed Successfully 💳
                </h2>

                <p style='font-size:16px;'>
                Your payment was completed successfully. Thank you for your purchase!
                </p>

                <div style=""height:15px;""></div>

                <!-- DETAILS + LOGO -->
                <table width='100%' cellpadding='0' cellspacing='0'>
                <tr>
                <td style='font-size:15px;line-height:1.6;'>
                <strong>Trip:</strong> {booking.Trip.Destination}<br>
                <strong>Dates:</strong> {booking.Trip.StartDate:dd/MM/yyyy} - {booking.Trip.EndDate:dd/MM/yyyy}<br>
                <strong>Rooms:</strong> {booking.Rooms}<br>
                <strong>Total amount:</strong> {booking.Rooms * booking.Trip.Price} ₪
                </td>

                <td align='right' valign='middle'>
                <img src='https://i.imgur.com/ZqyNBqC.png'
                     alt='The Travel Agency'
                     style='height:70px;opacity:0.9;' />
                </td>
                </tr>
                </table>

                <p style='margin-top:25px;color:#6c757d;font-size:14px;'>
                Your payment receipt is attached to this email.<br>
                We wish you a wonderful journey! ✈️🌍
                </p>

                </td>
                </tr>

                </table>

                </td>
                </tr>
                </table>

                </body>
                </html>";


            var mail = new MailMessage
            {
                From = new MailAddress("sceyael@gmail.com", "Travel Agency | No Reply"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mail.To.Add(toEmail);

            mail.Attachments.Add(
                new Attachment(
                    new MemoryStream(paymentPdf),
                    "PaymentReceipt.pdf",
                    "application/pdf"
                )
            );

            smtp.Send(mail);
        }

        // =========================
        // ⏳ WAITING LIST PROMOTION
        // =========================
        public void SendWaitingListPromotionEmail(string toEmail, Trip trip)
        {
            var smtp = CreateSmtpClient();

            string subject = "Good News! You’ve Been Promoted 🎉";

            string body = $@"
                <html>
                <body style=""margin:0;padding:0;background:#f4f6f8;
                font-family: Calibri, 'Segoe UI', Helvetica, Arial, sans-serif;"">

                <table width='100%' cellpadding='0' cellspacing='0'>
                <tr>
                <td align='center'>

                <table width='600' style='background:#ffffff;
                margin:30px 0;
                border-radius:12px;
                overflow:hidden;
                box-shadow:0 4px 12px rgba(0,0,0,0.1);'>

                <!-- IMAGE -->
                <tr>
                <td>
                <img src=""{trip.ImageUrl}""
                     style=""width:100%;max-height:240px;object-fit:cover;display:block;"" />
                </td>
                </tr>

                <!-- CONTENT -->
                <tr>
                <td style='padding:30px;color:#212529;'>

                <h2 style='margin-top:0;color:#198754;'>
                Good News! You’ve Been Promoted 🎉
                </h2>

                <p style='font-size:16px;'>
                A spot has opened up and you have been promoted from the waiting list.
                </p>

                <div style=""height:15px;""></div>

                <!-- DETAILS + LOGO -->
                <table width='100%' cellpadding='0' cellspacing='0'>
                <tr>
                <td style='font-size:15px;line-height:1.6;'>
                <strong>Trip:</strong> {trip.Destination}<br>
                <strong>Dates:</strong> {trip.StartDate:dd/MM/yyyy} - {trip.EndDate:dd/MM/yyyy}
                </td>

                <td align='right' valign='middle'>
                <img src='https://i.imgur.com/ZqyNBqC.png'
                     alt='The Travel Agency'
                     style='height:70px;opacity:0.9;' />
                </td>
                </tr>
                </table>

                <div style=""height:20px;""></div>

                <!-- WARNING -->
                <div style='background:#fff3cd;
                border:1px solid #ffeeba;
                border-radius:8px;
                padding:15px;
                color:#856404;
                font-size:14px;'>

                <strong>⚠️ Important:</strong><br>
                You have <strong>24 hours</strong> to complete the payment.<br>
                If payment is not completed within this time, the booking will be cancelled automatically.

                </div>

                <p style='margin-top:25px;color:#6c757d;font-size:14px;'>
                Please log in to your account and complete the payment to secure your booking.
                </p>

                </td>
                </tr>

                </table>

                </td>
                </tr>
                </table>

                </body>
                </html>";


            var mail = new MailMessage
            {
                From = new MailAddress("sceyael@gmail.com", "Travel Agency | No Reply"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mail.To.Add(toEmail);

            smtp.Send(mail);
        }

        // =========================
        // 🔧 SHARED SMTP CREATION
        // =========================
        private SmtpClient CreateSmtpClient()
        {
            return new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                EnableSsl = true,
                Credentials = new NetworkCredential(
                    "sceyael@gmail.com",
                    "chfbjdpuvpucaewa"
                )
            };
        }

        // =========================
        // ⛔ AUTO CANCELLATION (24h)
        // =========================
        public void SendAutoCancellationEmail(string toEmail, Trip trip)
        {
            var smtp = CreateSmtpClient();

            string subject = "Booking Cancelled – Payment Time Expired";

            string body = $@"
                <html>
                <body style=""margin:0;padding:0;background:#f4f6f8;
                font-family: Calibri, 'Segoe UI', Helvetica, Arial, sans-serif;"">

                <table width='100%' cellpadding='0' cellspacing='0'>
                <tr>
                <td align='center'>

                <table width='600' style='background:#ffffff;
                margin:30px 0;
                border-radius:12px;
                overflow:hidden;
                box-shadow:0 4px 12px rgba(0,0,0,0.1);'>

                <!-- IMAGE -->
                <tr>
                <td>
                <img src=""{trip.ImageUrl}""
                     style=""width:100%;max-height:240px;object-fit:cover;display:block;"" />
                </td>
                </tr>

                <!-- CONTENT -->
                <tr>
                <td style='padding:30px;color:#212529;'>

                <h2 style='margin-top:0;color:#dc3545;'>
                Booking Cancelled
                </h2>

                <p style='font-size:16px;'>
                Your booking has been cancelled automatically.
                </p>

                <p style='font-size:15px;color:#495057;'>
                <strong>Reason:</strong><br>
                Payment was not completed within 24 hours after being promoted from the waiting list.
                </p>

                <div style=""height:20px;""></div>

                <!-- DETAILS + LOGO -->
                <table width='100%' cellpadding='0' cellspacing='0'>
                <tr>
                <td style='font-size:15px;line-height:1.6;'>
                <strong>Trip:</strong> {trip.Destination}<br>
                <strong>Dates:</strong> {trip.StartDate:dd/MM/yyyy} - {trip.EndDate:dd/MM/yyyy}
                </td>

                <td align='right' valign='middle'>
                <img src='https://i.imgur.com/ZqyNBqC.png'
                     alt='The Travel Agency'
                     style='height:70px;opacity:0.9;' />
                </td>
                </tr>
                </table>

                <p style='margin-top:25px;color:#6c757d;font-size:14px;'>
                If you are still interested, you may try booking again.<br>
                Our support team is always here to help.
                </p>

                </td>
                </tr>

                </table>

                </td>
                </tr>
                </table>

                </body>
                </html>";


            var mail = new MailMessage
            {
                From = new MailAddress("sceyael@gmail.com", "Travel Agency | No Reply"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mail.To.Add(toEmail);
            smtp.Send(mail);
        }

        // =========================
        // ❌ USER CANCELLATION
        // =========================
        public void SendUserCancellationEmail(string toEmail, Trip trip)
        {
            var smtp = CreateSmtpClient();

            string subject = "Trip Cancelled Successfully";

            string body = $@"
                <html>
                <body style=""margin:0;padding:0;background:#f4f6f8;
                font-family: Calibri, 'Segoe UI', Helvetica, Arial, sans-serif;"">

                <table width='100%' cellpadding='0' cellspacing='0'>
                <tr>
                <td align='center'>

                <table width='600' style='background:#ffffff;
                margin:30px 0;
                border-radius:12px;
                overflow:hidden;
                box-shadow:0 4px 12px rgba(0,0,0,0.1);'>

                <!-- IMAGE -->
                <tr>
                <td>
                <img src=""{trip.ImageUrl}""
                     style=""width:100%;max-height:240px;object-fit:cover;display:block;"" />
                </td>
                </tr>

                <!-- CONTENT -->
                <tr>
                <td style='padding:30px;color:#212529;'>

                <h2 style='margin-top:0;color:#0d6efd;'>
                Trip Cancelled Successfully
                </h2>

                <p style='font-size:16px;'>
                Your trip has been cancelled successfully.
                </p>

                <div style=""height:20px;""></div>

                <!-- DETAILS + LOGO -->
                <table width='100%' cellpadding='0' cellspacing='0'>
                <tr>
                <td style='font-size:15px;line-height:1.6;'>
                <strong>Trip:</strong> {trip.Destination}<br>
                <strong>Dates:</strong> {trip.StartDate:dd/MM/yyyy} - {trip.EndDate:dd/MM/yyyy}
                </td>

                <td align='right' valign='bottom'>
                <img src='https://i.imgur.com/ZqyNBqC.png'
                     alt='The Travel Agency'
                     style='height:70px;opacity:0.9;' />
                </td>
                </tr>
                </table>

                <p style='margin-top:25px;color:#6c757d;font-size:14px;'>
                If you need any assistance, our support team is always here to help.
                </p>


                </td>
                </tr>

                </table>

                </td>
                </tr>
                </table>

                </body>
                </html>";



            var mail = new MailMessage
            {
                From = new MailAddress("sceyael@gmail.com", "Travel Agency | No Reply"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mail.To.Add(toEmail);
            smtp.Send(mail);
        }

        // =========================
        // 🔔 TRIP REMINDER
        // =========================
        public void SendTripReminderEmail(string toEmail, Trip trip, int daysBefore)
        {
            var smtp = CreateSmtpClient();

            string subject = "⏰ Trip Reminder – Your trip is coming soon!";

            string body = $@"
                <html>
                <body style=""margin:0;padding:0;background:#f4f6f8;
                font-family: Calibri, 'Segoe UI', Helvetica, Arial, sans-serif;"">

                <table width='100%' cellpadding='0' cellspacing='0'>
                <tr>
                <td align='center'>

                <table width='600' style='background:#ffffff;
                margin:30px 0;
                border-radius:12px;
                overflow:hidden;
                box-shadow:0 4px 12px rgba(0,0,0,0.1);'>

                <!-- IMAGE -->
                <tr>
                <td>
                <img src=""{trip.ImageUrl}""
                     style=""width:100%;max-height:240px;object-fit:cover;display:block;"" />
                </td>
                </tr>

                <!-- CONTENT -->
                <tr>
                <td style='padding:30px;color:#212529;'>

                <h2 style='margin-top:0;color:#0d6efd;'>
                Trip Reminder
                </h2>

                <p style='font-size:16px;'>
                This is a friendly reminder that your upcoming trip is just around the corner.
                </p>

                <p style='font-size:16px;'>
                📅 <strong>The trip starts in {daysBefore} days.</strong>
                </p>

                <div style=""height:20px;""></div>

                <!-- DETAILS + LOGO -->
                <table width='100%' cellpadding='0' cellspacing='0'>
                <tr>
                <td style='font-size:15px;line-height:1.6;'>
                <strong>Trip:</strong> {trip.Destination}<br>
                <strong>Dates:</strong> {trip.StartDate:dd/MM/yyyy} - {trip.EndDate:dd/MM/yyyy}
                </td>

                <td align='right' valign='middle'>
                <img src='https://i.imgur.com/ZqyNBqC.png'
                     alt='The Travel Agency'
                     style='height:70px;opacity:0.9;' />
                </td>
                </tr>
                </table>

                <p style='margin-top:25px;color:#6c757d;font-size:14px;'>
                We wish you a wonderful journey! ✈️🌍<br>
                If you need any assistance, our support team is always here to help.
                </p>

                </td>
                </tr>

                </table>

                </td>
                </tr>
                </table>

                </body>
                </html>";

            var mail = new MailMessage
            {
                From = new MailAddress("sceyael@gmail.com", "Travel Agency | No Reply"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mail.To.Add(toEmail);
            smtp.Send(mail);
        }

    }
}
