using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanVeXemPhim.Models;

namespace WebBanVeXemPhim.Controllers
{
    public class ThongBaoController : Controller
    {
        private readonly QuanLyBanVeXemPhimContext _context;

        public ThongBaoController(QuanLyBanVeXemPhimContext context)
        {
            _context = context;
        }
        [HttpPost]
        public IActionResult updateThongBao(int MaThongBao)
        {
            int makhachhang = HttpContext.Session.GetInt32("NguoiDung") ?? 0;
            var thongBao = _context.ThongBaos.FirstOrDefault(t => t.MaThongBao == MaThongBao);
            if (thongBao != null)
            {
                thongBao.TrangThai = true; // Đánh dấu là đã đọc
                _context.SaveChanges();

                // Đếm số thông báo chưa đọc còn lại
                int soLuongChuaDoc = _context.ThongBaos.Count(t => t.TrangThai == false && t.MaNguoiDung == makhachhang);

                return Ok(soLuongChuaDoc); // Trả về số lượng thông báo chưa đọc
            }
            return NotFound();
        }
    }
}
