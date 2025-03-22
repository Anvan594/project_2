using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanVeXemPhim.Models;

namespace WebBanVeXemPhim.Areas.admins.Controllers
{
    public class NguoiDungsController : BaseController
    {
        private readonly QuanLyBanVeXemPhimContext _context;

        public NguoiDungsController(QuanLyBanVeXemPhimContext context)
        {
            _context = context;
        }

        // GET: admins/NguoiDung
        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            int pageSize = 5;
            var query = _context.NguoiDungs.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(u => u.TenNguoiDung.Contains(searchString));
            }

            var danhSachNguoiDung = await query.OrderBy(u => u.MaNguoiDung)
                                               .Skip((page - 1) * pageSize)
                                               .Take(pageSize)
                                               .ToListAsync();

            int totalRecords = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchString = searchString;

            return View(danhSachNguoiDung);
        }

        // GET: admins/NguoiDung/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var nguoiDung = await _context.NguoiDungs.FirstOrDefaultAsync(m => m.MaNguoiDung == id);
            if (nguoiDung == null)
                return NotFound();

            return PartialView("Details", nguoiDung);
        }

        // GET: admins/NguoiDung/Create
        public IActionResult Create()
        {
            return PartialView("Create");
        }

        // POST: admins/NguoiDung/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(NguoiDung nguoiDung)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });

            if (_context.NguoiDungs.Any(u => u.Email == nguoiDung.Email))
                return Json(new { success = false, message = "Email đã tồn tại!" });

            _context.NguoiDungs.Add(nguoiDung);
            _context.SaveChanges();

            return Json(new { success = true, message = "Thêm người dùng thành công!" });
        }

        // GET: admins/NguoiDung/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var nguoiDung = await _context.NguoiDungs.FindAsync(id);
            if (nguoiDung == null)
                return NotFound();

            return PartialView("Edit", nguoiDung);
        }

        // POST: admins/NguoiDung/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, NguoiDung nguoiDung)
        {
            if (id != nguoiDung.MaNguoiDung)
                return NotFound();

            var existingUser = await _context.NguoiDungs.FindAsync(id);
            if (existingUser == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (string.IsNullOrEmpty(nguoiDung.MatKhau))
                    {
                        nguoiDung.MatKhau = existingUser.MatKhau;
                    }
                    _context.Entry(existingUser).CurrentValues.SetValues(nguoiDung);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.NguoiDungs.Any(e => e.MaNguoiDung == nguoiDung.MaNguoiDung))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return PartialView("Edit", nguoiDung);
        }

        // POST: admins/NguoiDung/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var nguoiDung = await _context.NguoiDungs.FindAsync(id);
            if (nguoiDung == null)
                return Json(new { success = false, message = "Không tìm thấy người dùng!" });

            _context.NguoiDungs.Remove(nguoiDung);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Xóa người dùng thành công!" });
        }

        private bool NguoiDungExists(int id)
        {
            return _context.NguoiDungs.Any(e => e.MaNguoiDung == id);
        }
    }
}
