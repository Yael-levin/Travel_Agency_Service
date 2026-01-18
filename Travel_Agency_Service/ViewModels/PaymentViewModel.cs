using System.ComponentModel.DataAnnotations;

namespace Travel_Agency_Service.ViewModels
{
    public class PaymentViewModel
    {
        [Required]
        public int BookingId { get; set; }
        [Required(ErrorMessage = "Card number is required")]
        [RegularExpression(@"^(\d{4}\s){3}\d{4}$",
            ErrorMessage = "Card number must be in format 1234 5678 9012 3456")]
        public string CardNumber { get; set; }


        [Required(ErrorMessage = "CVV is required")]
        [RegularExpression(@"^\d{3}$", ErrorMessage = "CVV must contain exactly 3 digits")]
        public string CVV { get; set; }

        [Required]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$", ErrorMessage = "Expiration date must be MM/YY")]
        public string ExpirationDate { get; set; }

        [Required(ErrorMessage = "Card holder name is required")]
        [RegularExpression(@"^[A-Za-zא-ת ]+$", ErrorMessage = "Name must contain letters only")]

        public string CardHolderName { get; set; }


        [Required(ErrorMessage = "ID number is required")]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "ID number must contain exactly 9 digits")]
        public string IDNumber { get; set; }
    }
}
