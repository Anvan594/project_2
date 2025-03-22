using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanVeXemPhim.Models;

namespace WebBanVeXemPhim.Areas.admins.Controllers
{
    public class KhuyenMaisController : BaseController
    {
        private readonly QuanLyBanVeXemPhimContext _context;

        public KhuyenMaisController(QuanLyBanVeXemPhimContext context)
        {
            _context = context;
        }

        // GET: admins/KhuyenMais - Hiển thị danh sách Khuyến Mãi với tìm kiếm và phân trang
        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            int pageSize = 5; // Số lượng Khuyến Mãi trên mỗi trang
            var query = _context.KhuyenMais.AsQueryable();

            // Lọc theo tên Khuyến Mãi
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(km => km.ThongTin.Contains(searchString));
            }

            // Lấy tổng số bản ghi sau khi lọc
            int totalRecords = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            // Sắp xếp theo tên Khuyến Mãi (A-Z) và lấy dữ liệu theo trang
            var danhSachKhuyenMai = await query
                .OrderBy(km => km.ThongTin)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Gửi dữ liệu phân trang về View
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchString = searchString;

            return View(danhSachKhuyenMai);
        }

        // GET: admins/KhuyenMais/Details/5 - Xem chi tiết Khuyến Mãi
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var khuyenMai = await _context.KhuyenMais.FirstOrDefaultAsync(m => m.MaKhuyenMai == id);
            if (khuyenMai == null) return NotFound();

            return PartialView("Details", khuyenMai);
        }

        // GET: admins/KhuyenMais/Create - Hiển thị form thêm Khuyến Mãi
        public IActionResult Create()
        {
            return PartialView("Create");
        }

        // POST: admins/KhuyenMais/Create - Xử lý thêm Khuyến Mãi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ThongTin,TrangThai")] KhuyenMai khuyenMai, IFormFile AnhFile)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return Json(new { success = false, message = "Dữ liệu không hợp lệ!", errors });
            }

            // Xử lý lưu ảnh nếu có file tải lên
            if (AnhFile != null && AnhFile.Length > 0)
            {
                var fileName = Path.GetFileName(AnhFile.FileName);
                var filePath = Path.Combine("wwwroot/images/KhuyenMai", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await AnhFile.CopyToAsync(stream);
                }

                khuyenMai.Anh = "KhuyenMai/" + fileName;
            }

            _context.KhuyenMais.Add(khuyenMai);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Thêm Khuyến Mãi thành công!" });
        }

        // GET: admins/KhuyenMais/Edit/5 - Hiển thị form chỉnh sửa Khuyến Mãi
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var khuyenMai = await _context.KhuyenMais.FindAsync(id);
            if (khuyenMai == null) return NotFound();

            return PartialView("Edit", khuyenMai);
        }

        // POST: admins/KhuyenMais/Edit/5 - Xử lý chỉnh sửa Khuyến Mãi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IFormFile fileAnh, [Bind("Anh,TrangThai,ThongTin,MaKhuyenMai")] KhuyenMai khuyenMai)
        {
            if (id != khuyenMai.MaKhuyenMai)
            {
                return NotFound();
            }

            var item = await _context.KhuyenMais.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            if (fileAnh == null)
            {
                item.ThongTin = khuyenMai.ThongTin;
                item.TrangThai = khuyenMai.TrangThai;
                _context.Update(item);
                await _context.SaveChangesAsync();
            }
            // Nếu có ảnh mới, cập nhật ảnh
            if (fileAnh != null && fileAnh.Length > 0)
            {
                // Xử lý ảnh mới như cũ
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/KhuyenMai");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(fileAnh.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await fileAnh.CopyToAsync(fileStream);
                }
                item.Anh = "KhuyenMai/" + uniqueFileName;
            }

            item.ThongTin = khuyenMai.ThongTin;
            item.TrangThai = khuyenMai.TrangThai;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(item);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KhuyenMaiExists(khuyenMai.MaKhuyenMai))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }

            return RedirectToAction("Index", "KhuyenMais");
        }

        // POST: admins/KhuyenMais/Delete/5 - Xóa Khuyến Mãi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var khuyenMai = await _context.KhuyenMais.FindAsync(id);
            if (khuyenMai == null)
                return Json(new { success = false, message = "Không tìm thấy Khuyến Mãi!" });

            try
            {
                _context.KhuyenMais.Remove(khuyenMai);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Xóa Khuyến Mãi thành công!" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Lỗi khi xóa Khuyến Mãi!" });
            }
        }

        private bool KhuyenMaiExists(int id)
        {
            return _context.KhuyenMais.Any(e => e.MaKhuyenMai == id);
        }
    }
}
