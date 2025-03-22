using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanVeXemPhim.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace WebBanVeXemPhim.Controllers
{
    public class ChiTietPhimController : Controller
    {
        private readonly QuanLyBanVeXemPhimContext _context;

        public ChiTietPhimController(QuanLyBanVeXemPhimContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(int id)
        {
            // Tìm phim theo mã phim
            var phim = await _context.Phims
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.MaPhim == id);
            var phim1=_context.Phims.Where(p => p.MaPhim==id).FirstOrDefault();
            ViewBag.DanhSachPhim = phim1;
            var trailer = await _context.Trailers
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.MaPhim == id);
            ViewBag.DanhSachTrailer = trailer;

            return View();
        }

    }
}
