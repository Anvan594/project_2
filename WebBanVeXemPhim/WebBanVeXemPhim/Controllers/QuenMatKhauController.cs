using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Mail;
using System.Net;
using WebBanVeXemPhim.Models;
using System.Text;
using System.Security.Cryptography;
using WebBanVeXemPhim.Data;

namespace WebBanVeXemPhim.Controllers
{

    public class QuenMatKhauController : Controller
    {
        private readonly QuanLyBanVeXemPhimContext _context;
        private readonly IMemoryCache _cache;

        public QuenMatKhauController(QuanLyBanVeXemPhimContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public IActionResult QuenMatKhau()
        {
            return View();
        }
        [HttpPost("quenmatkhau")]
        public async Task<IActionResult> QuenMatKhau([FromBody] ForgotPasswordRequest model)
        {
            if (string.IsNullOrEmpty(model.Email))
            {
                return BadRequest("Vui lòng nhập email.");
            }

            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                return BadRequest("Email không tồn tại.");
            }

            // Tạo mã xác nhận 6 số
            string code = GenerateVerificationCode();

            // Lưu mã xác nhận vào MemoryCache (hết hạn sau 10 phút)
            _cache.Set($"ResetCode_{model.Email}", code, TimeSpan.FromMinutes(10));

            // Gửi email mã xác nhận
            bool emailSent = await GuiMaXacNhanEmail(model.Email, user.TenNguoiDung, code);
            if (emailSent)
            {
                return Ok("Mã xác nhận đã được gửi đến email của bạn.");
            }
            else
            {
                return StatusCode(500, "Lỗi gửi email.");
            }
        }
        [HttpPost("xacnhanma")]
        public IActionResult XacNhanMa([FromBody] XacNhanCode model)
        {
            if (!_cache.TryGetValue($"ResetCode_{model.Email}", out string? storedCode) || storedCode != model.Code)
            {
                return BadRequest("Mã không hợp lệ hoặc đã hết hạn.");
            }

            // Nếu mã đúng, xóa khỏi cache để tránh dùng lại
            _cache.Remove($"ResetCode_{model.Email}");

            return Ok("Mã hợp lệ. Bạn có thể đổi mật khẩu.");
        }
        [HttpPost("doimatkhau")]
        public async Task<IActionResult> DoiMatKhau([FromBody] DatLaiMatKhau model)
        {

            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                return BadRequest("Email không tồn tại.");
            }

            // Đổi mật khẩu mới
            user.MatKhau = HashPassword(model.NewPassword);
            await _context.SaveChangesAsync();

            // Xóa mã xác nhận khỏi cache
            _cache.Remove($"ResetCode_{model.Email}");

            return Ok("Mật khẩu đã được thay đổi thành công.");
        }

        private string GenerateVerificationCode()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }
        private async Task<bool> GuiMaXacNhanEmail(string email, string username, string code)
        {
            try
            {
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("vanan9524@gmail.com", "xfaiihtrombafguy"),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("vanan9524@gmail.com", "Web Bán Vé Xem Phim"),
                    Subject = "Mã xác nhận đặt lại mật khẩu",
                    Body = $"Chào {username},<br>Mã xác nhận của bạn là: <b>{code}</b>. Mã có hiệu lực trong 10 phút.",
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(email);
                await smtpClient.SendMailAsync(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi gửi email: " + ex.Message);
                return false;
            }
        }



    }
}
