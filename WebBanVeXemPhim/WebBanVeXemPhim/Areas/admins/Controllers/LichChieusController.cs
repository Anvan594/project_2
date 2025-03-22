using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using WebBanVeXemPhim.Models;

namespace WebBanVeXemPhim.Areas.admins.Controllers
{
    public class LichChieusController : BaseController
    {
        private readonly QuanLyBanVeXemPhimContext _context;

        public LichChieusController(QuanLyBanVeXemPhimContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách lịch chiếu (có tìm kiếm, phân trang)
        public async Task<IActionResult> Index(string searchString,DateOnly searchDate, int page = 1)
        {
            int pageSize = 5; // Số bản ghi mỗi trang
            var query = _context.LichChieus
                .Include(l => l.MaPhimNavigation)
                .Include(l => l.MaPhongNavigation)
                .AsQueryable();

            // Lọc theo tên phim (nếu có nhập từ khóa tìm kiếm)
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(l => l.MaPhimNavigation.TenPhim.Contains(searchString));
            }
            if (searchDate != default)
            {
                query = query.Where(l => l.NgayChieu == searchDate);
            }

            // Lấy tổng số bản ghi sau khi lọc
            int totalRecords = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            // Sắp xếp theo ngày giảm dần + giờ giảm dần
            var danhSachLichChieu = await query
                .OrderByDescending(l => l.NgayChieu)
                .ThenByDescending(l => l.GioChieu)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Gửi dữ liệu phân trang về View
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchString = searchString;
            ViewBag.SearchDate = searchDate;

            return View(danhSachLichChieu);

        }


        // Hiển thị chi tiết lịch chiếu
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var lichChieu = await _context.LichChieus.Include(l => l.MaPhimNavigation).Include(l => l.MaPhongNavigation)
                                                     .FirstOrDefaultAsync(m => m.MaLichChieu == id);
            if (lichChieu == null) return NotFound();

            return PartialView("Details", lichChieu);
        }

        // GET: Thêm lịch chiếu
        public IActionResult Create()
        {
            ViewBag.MaPhim = new SelectList(_context.Phims
        .Where(p => p.TrangThai == true &&
                 p.NgayKhoiChieu <= DateOnly.FromDateTime(DateTime.Now.AddDays(8))) // Chuyển đổi để so sánh
        .OrderByDescending(p => p.NgayKhoiChieu), // Sắp xếp theo ngày khởi chiếu mới nhất
        "MaPhim", "TenPhim"); // Chọn dữ liệu cho dropdown

            ViewBag.MaPhong = new SelectList(_context.Phongs, "MaPhong", "TenPhong");
            return PartialView("Create");
        }

        // POST: Thêm lịch chiếu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LichChieu lichChieu, int songay)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => $"{x.Key}: {x.Value.Errors.First().ErrorMessage}")
                    .ToArray();

                return Json(new { success = false, message = "Dữ liệu không hợp lệ.", errors = string.Join("\n", errors) });
            }

            List<LichChieu> lichChieus = new List<LichChieu>();
            TimeOnly gioChieu = lichChieu.GioChieu; // Lấy giờ chiếu ban đầu
            var ngaychieu = lichChieu.NgayChieu;
            if (ngaychieu != null)
            {
                for (var j = 0; j < songay; j++)
                {
                    gioChieu = lichChieu.GioChieu;
                    for (int i = 0; i < 8; i++)
                    {
                        // Kiểm tra lịch chiếu có bị trùng không
                        bool isDuplicate = await _context.LichChieus.AnyAsync(lc =>
                            lc.MaPhong == lichChieu.MaPhong &&
                            lc.NgayChieu == lichChieu.NgayChieu &&
                            lc.GioChieu == gioChieu);

                        if (!isDuplicate)
                        {
                            lichChieus.Add(new LichChieu
                            {
                                MaPhim = lichChieu.MaPhim,
                                MaPhong = lichChieu.MaPhong,
                                NgayChieu = ngaychieu,
                                GioChieu = gioChieu, // Gán giờ chiếu hiện tại
                                GiaVe = lichChieu.GiaVe,
                                TrangThai = lichChieu.TrangThai
                            });
                        }

                        // Tăng giờ chiếu thêm 2 tiếng
                        gioChieu = gioChieu.Add(TimeSpan.FromHours(2));
                        if (gioChieu == new TimeOnly(23, 30) || gioChieu == new TimeOnly(22, 00))
                            break;
                    }
                    ngaychieu = ngaychieu.AddDays(1);
                }
            }
            else {
                for (int i = 0; i < 8; i++)
                {
                    // Kiểm tra lịch chiếu có bị trùng không
                    bool isDuplicate = await _context.LichChieus.AnyAsync(lc =>
                        lc.MaPhong == lichChieu.MaPhong &&
                        lc.NgayChieu == lichChieu.NgayChieu &&
                        lc.GioChieu == gioChieu);

                    if (!isDuplicate)
                    {
                        lichChieus.Add(new LichChieu
                        {
                            MaPhim = lichChieu.MaPhim,
                            MaPhong = lichChieu.MaPhong,
                            NgayChieu = ngaychieu,
                            GioChieu = gioChieu, // Gán giờ chiếu hiện tại
                            GiaVe = lichChieu.GiaVe,
                            TrangThai = lichChieu.TrangThai
                        });
                    }

                    // Tăng giờ chiếu thêm 2 tiếng
                    gioChieu = gioChieu.Add(TimeSpan.FromHours(2));
                    if (gioChieu == new TimeOnly(23, 30) || gioChieu == new TimeOnly(22, 00))
                        break;
                }
            }



            if (lichChieus.Count > 0)
            {
                await _context.LichChieus.AddRangeAsync(lichChieus);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = $"Thêm {lichChieus.Count} lịch chiếu thành công!" });
            }
            else
            {
                return Json(new { success = false, message = "Tất cả lịch chiếu đã tồn tại!" });
            }
        }


        // GET: Chỉnh sửa lịch chiếu
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var lichChieu = await _context.LichChieus.FindAsync(id);
            if (lichChieu == null) return NotFound();
            List<string> gioChieuList = (lichChieu.MaPhong % 2 == 0)
        ? new List<string> { "06:00", "08:00", "10:00", "12:00", "14:00", "16:00", "18:00", "20:00", "22:00" }
        : new List<string> { "07:30", "09:30", "11:30", "13:30", "15:30", "17:30", "19:30", "21:30", "23:30" };

            ViewBag.GioChieu = gioChieuList;
            ViewBag.MaPhim = new SelectList(_context.Phims, "MaPhim", "TenPhim", lichChieu.MaPhim);
            ViewBag.MaPhong = new SelectList(_context.Phongs, "MaPhong", "TenPhong", lichChieu.MaPhong);
            return PartialView("Edit", lichChieu);
        }

        // POST: Chỉnh sửa lịch chiếu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LichChieu lichChieu)
        {
            var existingLichChieu = await _context.LichChieus.FindAsync(id);
            if (existingLichChieu == null) return NotFound();

            existingLichChieu.NgayChieu = lichChieu.NgayChieu;
            existingLichChieu.GioChieu = lichChieu.GioChieu;
            existingLichChieu.GiaVe = lichChieu.GiaVe;
            existingLichChieu.TrangThai = lichChieu.TrangThai;
            existingLichChieu.MaPhong = lichChieu.MaPhong;
            existingLichChieu.MaPhim = lichChieu.MaPhim;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.LichChieus.Any(e => e.MaLichChieu == id)) return NotFound();
                else throw;
            }

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public IActionResult CheckDuplicate(int maPhong, DateOnly ngayChieu, TimeOnly gioChieu, int? maLichChieu)
        {
            var isDuplicate = _context.LichChieus.Any(lc =>
                lc.MaPhong == maPhong &&
                lc.NgayChieu == ngayChieu &&
                lc.GioChieu == gioChieu &&
                (maLichChieu == null || lc.MaLichChieu != maLichChieu) // Nếu là cập nhật thì bỏ qua chính nó
            );

            return Json(new { isDuplicate });
        }




        // Xóa lịch chiếu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lichChieu = await _context.LichChieus.FindAsync(id);
            if (lichChieu == null) return Json(new { success = false, message = "Không tìm thấy lịch chiếu!" });

            _context.LichChieus.Remove(lichChieu);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Xóa lịch chiếu thành công!" });
        }
    }
}
