using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using WebBanVeXemPhim.Models;

namespace WebBanVeXemPhim.Controllers
{
    public class LoginController : Controller
    {
        public QuanLyBanVeXemPhimContext _context;
        public LoginController(QuanLyBanVeXemPhimContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost] // POST -> khi submit form
        public IActionResult Index(Models.LoginNguoiDung model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);// trả về trạng thái lỗi
            }
            // sẽ xử lý logic phần đăng nhập tại đây
            var pass = HashPassword(model.MatKhau);
            var dataLogin = _context.NguoiDungs
    .Where(x => x.Email == model.Email && x.MatKhau == pass)
    .Select(x => new { x.MaNguoiDung, x.Email, x.TrangThai,x.TenNguoiDung,x.Token }) // Chỉ lấy cột cần thiết
    .FirstOrDefault();


            if (dataLogin == null)
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng!");
                return View(model);
            }

            // Kiểm tra trạng thái tài khoản
            if (dataLogin.TrangThai == false)
            {
                ModelState.AddModelError("", "Tài khoản của bạn đã bị khóa!");
                return View(model);
            }

            // Đăng nhập thành công
            if (dataLogin.Token != null&&dataLogin.Token.Trim() == "Nhan Vien")
            {
                HttpContext.Session.SetInt32("NguoiDung", dataLogin.MaNguoiDung);
                HttpContext.Session.SetString("Email", dataLogin.Email);
                HttpContext.Session.SetString("TenNguoiDung", dataLogin.TenNguoiDung);
                HttpContext.Session.SetString("ChucVu",dataLogin.Token.Trim());
                return RedirectToAction("index", "NhanVien");
            }
            else
            {
                HttpContext.Session.SetInt32("NguoiDung", dataLogin.MaNguoiDung);
                HttpContext.Session.SetString("Email", dataLogin.Email);
                HttpContext.Session.SetString("TenNguoiDung", dataLogin.TenNguoiDung);
                return RedirectToAction("index", "Home");
            }
        }
        public IActionResult DangKy()
        {
            return View();
        }
        public IActionResult QuenMatKhau()
        {
            return View();
        }
        [HttpGet]// thoát đăng nhập, huỷ session
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("NguoiDung"); // huỷ session với key AdminLogin đã lưu trước đó
            HttpContext.Session.Remove("ChucVu");
            return RedirectToAction("Index","home");
        }
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }
    }

}
