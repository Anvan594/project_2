using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanVeXemPhim.Areas.admins.Controllers;
using WebBanVeXemPhim.Models;
using Microsoft.EntityFrameworkCore;

namespace Web_Api.Areas.admins.Controllers
{
    public class HomeController : BaseController
    {
        private readonly HttpClient _httpClient;
        private readonly QuanLyBanVeXemPhimContext _context;

        public HomeController(IHttpClientFactory httpClientFactory,QuanLyBanVeXemPhimContext context)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
        }
        public async Task<IActionResult> IndexAsync()
        {
            var NguoiDung = _context.NguoiDungs.ToArray();
            ViewBag.SoNguoiDk = NguoiDung.Length;
            var ThanhToan =_context.ThanhToans.ToArray();
            ViewBag.SoDonHang=ThanhToan.Length;
            var Ves =_context.Ves.ToArray();
            ViewBag.DoanhSo = Ves.Sum(v => v.GiaVe).ToString("N0");
            var Phim = _context.Phims.ToArray();
            ViewBag.SoPhim = Phim.Length;
            // Thống kê doanh thu theo tuần
            var doanhSoTheoTuan = _context.Ves
            .Where(v => v.NgayDat.HasValue) // Bỏ qua dữ liệu null
            .AsEnumerable() // Chuyển sang bộ nhớ để xử lý
            .GroupBy(v => new
            {
                WeekStart = v.NgayDat.Value.Date.AddDays(-(v.NgayDat.Value.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)v.NgayDat.Value.DayOfWeek - 1))
            })
            .Select(g => new
            {
                WeekStart = g.Key.WeekStart, // Giữ lại WeekStart để sắp xếp
                Tuan = g.Key.WeekStart.ToString("dd/MM/yyyy") + " - " + g.Key.WeekStart.AddDays(6).ToString("dd/MM/yyyy"),
                DoanhSo = g.Sum(v => v.GiaVe)
            })
            .OrderBy(g => g.WeekStart) // Sắp xếp theo tuần từ cũ đến mới
            .ToList();

            // Truyền dữ liệu sang View để hiển thị trong biểu đồ
            ViewBag.DoanhSoLabels = doanhSoTheoTuan.Select(x => x.Tuan).ToArray();
            ViewBag.DoanhSoData = doanhSoTheoTuan.Select(x => x.DoanhSo).ToArray();
            var currentMonth = DateTime.Today.Month;
            var currentYear = DateTime.Today.Year;
            var TopPhim = await _context.Ves
               .Include(v => v.MaLichChieuNavigation.MaPhimNavigation) // Load thông tin phim
               .Where(v => v.NgayDat.HasValue &&
                           v.NgayDat.Value.Month == currentMonth &&
                           v.NgayDat.Value.Year == currentYear) // Chỉ lấy vé trong tháng hiện tại
               .GroupBy(v => new
               {
                   v.MaLichChieuNavigation.MaPhim,
                   TenPhim = v.MaLichChieuNavigation.MaPhimNavigation.TenPhim,
                   AnhPhim = v.MaLichChieuNavigation.MaPhimNavigation.Poster,
                   DaoDien = v.MaLichChieuNavigation.MaPhimNavigation.DaoDien,
                   ThoiLuong=v.MaLichChieuNavigation.MaPhimNavigation.ThoiLuong
               })
               .Select(g => new
               {
                   MaPhim = g.Key.MaPhim,
                   TenPhim = g.Key.TenPhim,
                   AnhPhim = g.Key.AnhPhim,
                   DaoDien = g.Key.DaoDien,
                   ThoiLuong=g.Key.ThoiLuong,
                   SoVe = g.Count() // Đếm số vé bán ra
               })
               .OrderByDescending(g => g.SoVe) // Sắp xếp giảm dần theo số vé
               .Take(5) // Lấy top 5 phim bán chạy nhất
               .ToListAsync();

            ViewData["TopMovies"] = TopPhim;
            ViewBag.TopPhim = TopPhim;
            return View();
        }
    }
}
