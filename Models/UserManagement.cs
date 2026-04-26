namespace RentalVehicleService.Models.ViewModels
{
    public class UserManagement
    {
        // Thông tin cơ bản từ IdentityUser
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        // Các thuộc tính tùy chỉnh từ file Migration của bạn
        public string FullName { get; set; }
        public string UserType { get; set; }
        public string? CCCD { get; set; }
        public string? PassportNumber { get; set; }
        public string? Nationality { get; set; }

        // Trạng thái tài khoản
        public bool IsLockedOut { get; set; }

        // Quyền hạn (Roles) của User này
        public IEnumerable<string> Roles { get; set; }
    }
}