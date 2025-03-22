
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System.Collections.Generic;
using WebBanVeXemPhim.Models;

namespace WebBanVeXemPhim.Areas.admins.Controllers
{
    [Area("Admins")]
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
        public IActionResult Index(Models.Login model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);// trả về trạng thái lỗi
            }
            // sẽ xử lý logic phần đăng nhập tại đây
            var pass = model.MatKhau;
            var dataLogin = _context.TaiKhoanAdmins
    .Where(x => x.Email == model.Email && x.MatKhau == pass)
    .Select(x => new { x.MaAdmin, x.Email,x.TrangThai,x.ChucVu,x.SoDienThoai,x.TenDangNhap }) // Chỉ lấy cột cần thiết
    .FirstOrDefault();


            if (dataLogin == null)
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng!");
                return View(model);
            }

            // Kiểm tra trạng thái tài khoản
            if (dataLogin.TrangThai==false)
            {
                ModelState.AddModelError("", "Tài khoản của bạn đã bị khóa!");
                return View(model);
            }

            // Đăng nhập thành công
            if (dataLogin.ChucVu == "Admin")
            {
                HttpContext.Session.SetString("AdminLogin", model.Email);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                HttpContext.Session.SetString("ChucVu", dataLogin.ChucVu);
                HttpContext.Session.SetInt32("NguoiDung", 37);
                HttpContext.Session.SetString("TenNguoiDung", dataLogin.TenDangNhap);
                HttpContext.Session.SetString("Email", dataLogin.Email);
                return Redirect("~/../NhanVien/Index");

            }
        }
        [HttpGet]// thoát đăng nhập, huỷ session
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("AdminLogin"); // huỷ session với key AdminLogin đã lưu trước đó
            HttpContext.Session.Remove("ChucVu");
            return RedirectToAction("Index","Home");
        }
    }

}