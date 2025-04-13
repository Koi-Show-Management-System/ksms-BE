using System.ComponentModel.DataAnnotations;

namespace KSMS.Domain.Dtos.Requests.Authentication
{
    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại")]
        public string OldPassword { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        public string NewPassword { get; set; }
        
        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới")]
        public string ConfirmNewPassword { get; set; }
    }
} 