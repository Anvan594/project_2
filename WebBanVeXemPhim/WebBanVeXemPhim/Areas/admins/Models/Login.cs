using System.ComponentModel.DataAnnotations;

namespace WebBanVeXemPhim.Areas.admins.Models
{
    public class Login
    {
        [Required(ErrorMessage = "Địa chỉ email không để trống")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Mật khẩu không để trống")]
        public string MatKhau { get; set; }
        public bool Remember { get; set; }
    }
}