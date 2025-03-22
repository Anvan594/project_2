using System.ComponentModel.DataAnnotations;

namespace WebBanVeXemPhim.Models
{
    public class LoginNguoiDung
    {
        [Required(ErrorMessage = "Địa chỉ email không để trống")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Mật khẩu không để trống")]
        public string MatKhau { get; set; }
        public int MaNguoiDung { get; set; }
        public bool Remember { get; set; }
    }
}
