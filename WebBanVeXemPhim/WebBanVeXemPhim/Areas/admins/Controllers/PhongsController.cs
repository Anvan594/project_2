using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanVeXemPhim.Models;

namespace WebBanVeXemPhim.Areas.admins.Controllers
{
    public class PhongsController : BaseController
    {
        private readonly QuanLyBanVeXemPhimContext _context;

        public PhongsController(QuanLyBanVeXemPhimContext context)
        {
            _context = context;
        }

        // GET: admins/Phongs
        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            int pageSize = 5;
            var query = _context.Phongs.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.TenPhong.Contains(searchString));
            }

            var danhSachPhong = await query.OrderBy(p => p.MaPhong)
                                           .Skip((page - 1) * pageSize)
                                           .Take(pageSize)
                                           .ToListAsync();

            int totalRecords = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchString = searchString;

            return View(danhSachPhong);
        }

        // GET: admins/Phongs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phong = await _context.Phongs.FirstOrDefaultAsync(m => m.MaPhong == id);
            if (phong == null)
            {
                return NotFound();
            }

            return PartialView("Details", phong);
        }

        // GET: admins/Phongs/Create
        public IActionResult Create()
        {
            return PartialView("Create");
        }

        // POST: admins/Phongs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(string TenPhong, int TongSoGhe)
        {
            try
            {
                Console.WriteLine("TongSoGhe nhận được: " + TongSoGhe); // Debug
                using (var context = new QuanLyBanVeXemPhimContext())
                {
                    context.Database.ExecuteSqlRaw("EXEC ThemPhongVaGhe @TenPhong = {0}, @TongSoGhe = {1}", TenPhong, TongSoGhe);
                }
                return Json(new { success = true, message = "Thêm phòng thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi thêm phòng: " + ex.Message });
            }
        }


        // GET: admins/Phongs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phong = await _context.Phongs.FindAsync(id);
            if (phong == null)
            {
                return NotFound();
            }

            return PartialView("Edit", phong);
        }

        // POST: admins/Phongs/Edit/5
        [HttpPost]
        public IActionResult Edit(Phong phong)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using (var context = new QuanLyBanVeXemPhimContext())
                    {
                        context.Database.ExecuteSqlRaw(
                            "EXEC CapNhatPhongVaGhe @MaPhong = {0}, @TenPhongMoi = {1}, @TongSoGheMoi = {2}, @TrangThaiMoi = {3}",
                            phong.MaPhong, phong.TenPhong, phong.SoLuongGhe, phong.TrangThai);
                    }
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi khi cập nhật phòng: " + ex.Message);
                }
            }
            return View(phong);
        }


        // POST: admins/Phongs/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                using (var context = new QuanLyBanVeXemPhimContext())
                {
                    context.Database.ExecuteSqlRaw("EXEC XoaPhongVaGhe @MaPhong = {0}", id);
                }
                return Json(new { success = true, message = "Xóa phòng và ghế thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi xóa: " + ex.Message });
            }
        }



        private bool PhongExists(int id)
        {
            return _context.Phongs.Any(e => e.MaPhong == id);
        }
    }
}
