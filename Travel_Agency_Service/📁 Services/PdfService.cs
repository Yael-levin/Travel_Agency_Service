using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Travel_Agency_Service.Models;
using System.IO;
using System.Net.Http;


namespace Travel_Agency_Service.Services
{
    public class PdfService
    {
        private byte[]? LoadLogo()
        {
            var path = Path.Combine("wwwroot", "images", "The Travel Agency logo.png");
            return File.Exists(path) ? File.ReadAllBytes(path) : null;
        }

        // =========================
        // 📄 BOOKING / ITINERARY PDF
        // =========================
        public byte[] GenerateBookingPdf(Booking booking)
        {
            var logo = LoadLogo();
            var tripImage = LoadTripImage(booking.Trip.ImageUrl);

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);

                    page.Content()
                        .AlignCenter()
                        .Element(card =>
                        {
                            card
                                .Width(420)
                                .Padding(20)
                                .Border(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .Background(Colors.White)
                                .Column(col =>
                                {
                                    col.Spacing(12);

                                    // HEADER
                                    col.Item().Row(row =>
                                    {
                                        row.RelativeItem()
                                            .AlignMiddle()
                                            .AlignCenter()
                                            .Text("Travel Itinerary")
                                            .FontSize(22)
                                            .Bold()
                                            .FontColor(Colors.Blue.Medium);

                                        if (logo != null)
                                        {
                                            row.ConstantItem(60)
                                                .Container()
                                                .Height(60)
                                                .AlignMiddle()
                                                .Image(logo)
                                                .FitHeight();
                                        }
                                    });

                                    col.Item().LineHorizontal(1)
                                        .LineColor(Colors.Grey.Lighten2);

                                    // TRIP IMAGE
                                    if (tripImage != null)
                                    {
                                        col.Item().AlignCenter()
                                            .Container()
                                            .Height(200)
                                            .Image(tripImage)
                                            .FitArea();
                                    }

                                    col.Item().AlignCenter()
                                        .Text($"Booking ID: {booking.BookingId}")
                                        .FontSize(11)
                                        .FontColor(Colors.Grey.Darken1);

                                    col.Item().AlignCenter()
                                        .Text($"Destination: {booking.Trip.Destination}");

                                    col.Item().AlignCenter()
                                        .Text($"Country: {booking.Trip.Country}");

                                    col.Item().AlignCenter()
                                        .Text($"Dates: {booking.Trip.StartDate:dd/MM/yyyy} - {booking.Trip.EndDate:dd/MM/yyyy}");

                                    if (!string.IsNullOrWhiteSpace(booking.Trip.Description))
                                    {
                                        col.Item()
                                            .PaddingTop(10)
                                            .AlignCenter()
                                            .Text(booking.Trip.Description)
                                            .FontSize(11)
                                            .FontColor(Colors.Grey.Darken1);
                                    }

                                    col.Item().AlignCenter()
                                        .Text($"Rooms: {booking.Rooms}");

                                    col.Item().AlignCenter()
                                        .Text($"Total Price: {booking.Rooms * booking.Trip.Price} ₪")
                                        .Bold();

                                    col.Item()
                                        .PaddingTop(20)
                                        .AlignCenter()
                                        .Text("Passenger Details")
                                        .Bold()
                                        .FontSize(14);

                                    col.Item().AlignCenter()
                                        .Text($"Name: {booking.User.FullName}");

                                    col.Item().AlignCenter()
                                        .Text($"Email: {booking.User.Email}");

                                    col.Item()
                                        .PaddingTop(15)
                                        .AlignCenter()
                                        .Text("We wish you a wonderful journey ✈️🌍")
                                        .FontSize(10)
                                        .FontColor(Colors.Grey.Darken1);
                                });
                        });
                });
            }).GeneratePdf();
        }


        // =========================
        // 🧾 PAYMENT / RECEIPT PDF
        // =========================
        public byte[] GeneratePaymentPdf(Payment payment)
        {
            var logo = LoadLogo();
            var tripImage = LoadTripImage(payment.Booking.Trip.ImageUrl);

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);

                    page.Content()
                        .AlignCenter()
                        .Element(card =>
                        {
                            card
                                .Width(420)
                                .Padding(20)
                                .Border(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .Background(Colors.White)
                                .Column(col =>
                                {
                                    col.Spacing(12);

                                    // HEADER
                                    col.Item().Row(row =>
                                    {
                                        row.RelativeItem()
                                            .AlignMiddle()
                                            .AlignCenter()
                                            .Text("Payment Receipt")
                                            .FontSize(22)
                                            .Bold()
                                            .FontColor(Colors.Blue.Medium);

                                        if (logo != null)
                                        {
                                            row.ConstantItem(60)
                                                .Container()
                                                .Height(60)
                                                .AlignMiddle()
                                                .Image(logo)
                                                .FitHeight();
                                        }
                                    });

                                    col.Item()
                                        .LineHorizontal(1)
                                        .LineColor(Colors.Grey.Lighten2);

                                    // TRIP IMAGE 
                                    if (tripImage != null)
                                    {
                                        col.Item()
                                            .AlignCenter()
                                            .Container()
                                            .Height(200)
                                            .Image(tripImage)
                                            .FitArea();
                                    }

                                    // DETAILS
                                    col.Item().AlignCenter()
                                        .Text($"Payment ID: {payment.PaymentId}")
                                        .FontSize(11)
                                        .FontColor(Colors.Grey.Darken1);

                                    col.Item().AlignCenter()
                                        .Text($"Booking ID: {payment.BookingId}");

                                    col.Item().AlignCenter()
                                        .Text($"Destination: {payment.Booking.Trip.Destination}");

                                    col.Item().AlignCenter()
                                        .Text($"Dates: {payment.Booking.Trip.StartDate:dd/MM/yyyy} - {payment.Booking.Trip.EndDate:dd/MM/yyyy}");

                                    col.Item().AlignCenter()
                                        .Text($"Amount Paid: {payment.Amount} ₪")
                                        .Bold();

                                    col.Item().AlignCenter()
                                        .Text($"Payment Date: {payment.PaymentDate:dd/MM/yyyy HH:mm}");

                                    col.Item().AlignCenter()
                                        .Text($"Status: {payment.Status}");

                                    col.Item()
                                        .PaddingTop(20)
                                        .AlignCenter()
                                        .Text($"Customer Email: {payment.User.Email}")
                                        .FontSize(11)
                                        .FontColor(Colors.Grey.Darken1);

                                    col.Item()
                                        .PaddingTop(15)
                                        .AlignCenter()
                                        .Text("Thank you for your purchase! 💳✈️")
                                        .FontSize(10)
                                        .FontColor(Colors.Grey.Darken1);
                                });
                        });
                });
            }).GeneratePdf();
        }


        private byte[]? LoadTripImage(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return null;

            try
            {
                using var client = new HttpClient();
                return client.GetByteArrayAsync(imageUrl).Result;
            }
            catch
            {
                return null;
            }
        }
    }
}


