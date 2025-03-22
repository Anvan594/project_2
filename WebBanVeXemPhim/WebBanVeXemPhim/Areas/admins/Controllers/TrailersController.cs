using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanVeXemPhim.Models;

namespace WebBanVeXemPhim.Areas.admins.Controllers
{
    public class TrailersController : BaseController
    {
        private readonly QuanLyBanVeXemPhimContext _context;

        public TrailersController(QuanLyBanVeXemPhimContext context)
        {
            _context = context;
        }

        // GET: admins/Trailers
        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            int pageSize = 3;
            var query = _context.Trailers.Include(t => t.MaPhimNavigation).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(t => t.MaPhimNavigation.TenPhim.Contains(searchString));
            }

            var danhSachTrailer = await query.OrderByDescending(t => t.MaPhimNavigation.NgayKhoiChieu) // Sắp xếp theo ngày khởi chiếu
                                             .Skip((page - 1) * pageSize)
                                             .Take(pageSize)
                                             .ToListAsync();

            ViewBag.TotalPages = (int)Math.Ceiling((double)await query.CountAsync() / pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.SearchString = searchString;

            return View(danhSachTrailer);


        }


        // GET: admins/Trailers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trailer = await _context.Trailers
                .Include(t => t.MaPhimNavigation) // Load thông tin phim liên quan
                .FirstOrDefaultAsync(m => m.MaTrailer == id);

            if (trailer == null)
            {
                return NotFound();
            }

            return PartialView("Details", trailer);
        }


        // GET: admins/Trailers/Create
        public IActionResult Create()
        {
            ViewBag.DanhSachPhim = _context.Phims
            .Where(p => p.TrangThai == true && !_context.Trailers.Any(t => t.MaPhim == p.MaPhim)) // Chỉ lấy phim chưa có trailer
            .OrderByDescending(p => p.NgayKhoiChieu)
            .ToList();

            return PartialView("Create");
        }

        // POST: admins/Trailers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Trailer trailer)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });
            }

            var checkTrailer = _context.Trailers.Any(t => t.MaPhim == trailer.MaPhim);
            if (checkTrailer)
            {
                return Json(new { success = false, message = "Phim đã có trailer tồn tại!" });
            }

            _context.Trailers.Add(trailer);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Thêm trailer thành công!" });
        }

        // GET: admins/Trailers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trailer = await _context.Trailers.FindAsync(id);
            if (trailer == null)
            {
                return NotFound();
            }

            // Lấy danh sách phim để hiển thị trong dropdown
            ViewBag.DanhSachPhim = _context.Phims
                .Select(p => new { p.MaPhim, p.TenPhim })
                .ToList();

            return PartialView(trailer);
        }


        // POST: admins/Trailers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Trailer trailer)
        {
            if (id != trailer.MaTrailer)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return PartialView("Edit", trailer);
            }

            try
            {
                _context.Entry(trailer).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TrailerExists(trailer.MaTrailer))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: admins/Trailers/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trailer = await _context.Trailers.FindAsync(id);
            if (trailer == null)
            {
                return Json(new { success = false, message = "Không tìm thấy trailer!" });
            }

            _context.Trailers.Remove(trailer);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Xóa trailer thành công!" });
        }

        private bool TrailerExists(int id)
        {
            return _context.Trailers.Any(e => e.MaTrailer == id);
        }
    }
}
