namespace RentalVehicleService.Models
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; } = "smtp.gmail.com";
        public int Port { get; set; } = 587;
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderName { get; set; } = "SaigonRide";
        public string Password { get; set; } = string.Empty;
    }
}
