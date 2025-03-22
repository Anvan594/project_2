using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanVeXemPhim.Models;

namespace WebBanVeXemPhim.Areas.admins.Controllers
{
    public class ComboesController : BaseController
    {
        private readonly QuanLyBanVeXemPhimContext _context;

        public ComboesController(QuanLyBanVeXemPhimContext context)
        {
            _context = context;
        }

        // GET: admins/Comboes - Hiển thị danh sách Combo với tìm kiếm và phân trang
        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            int pageSize = 5; // Số lượng Combo trên mỗi trang
            var query = _context.Comboes.AsQueryable();

            // Lọc theo tên Combo
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(c => c.TenCombo.Contains(searchString));
            }

            // Lấy tổng số bản ghi sau khi lọc
            int totalRecords = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            // Sắp xếp theo tên Combo (A-Z) và lấy dữ liệu theo trang
            var danhSachCombo = await query
                .OrderBy(c => c.TenCombo)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Gửi dữ liệu phân trang về View
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchString = searchString;

            return View(danhSachCombo);
        }

        // GET: admins/Comboes/Details/5 - Xem chi tiết Combo
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var combo = await _context.Comboes.FirstOrDefaultAsync(m => m.MaCombo == id);
            if (combo == null) return NotFound();

            return PartialView("Details", combo);
        }

        // GET: admins/Comboes/Create - Hiển thị form thêm Combo
        public IActionResult Create()
        {
            return PartialView("Create");
        }

        // POST: admins/Comboes/Create - Xử lý thêm Combo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenCombo,Gia")] Combo combo, IFormFile AnhFile)
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
                var filePath = Path.Combine("wwwroot/images/Combo", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await AnhFile.CopyToAsync(stream);
                }

                combo.Anh ="Combo/"+ fileName;
            }

            _context.Comboes.Add(combo);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Thêm Combo thành công!" });
        }

        // GET: admins/Comboes/Edit/5 - Hiển thị form chỉnh sửa Combo
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var combo = await _context.Comboes.FindAsync(id);
            if (combo == null) return NotFound();

            return PartialView("Edit", combo);
        }

        // POST: admins/Comboes/Edit/5 - Xử lý chỉnh sửa Combo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IFormFile fileAnh, [Bind("Anh,Gia,TenCombo,SoLuong,MaCombo")] Combo combo)
        {
            if (id != combo.MaCombo)
            {
                return NotFound();
            }

            var item = await _context.Comboes.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            if (fileAnh == null)
            {
               item.TenCombo = combo.TenCombo;
                item.Gia = combo.Gia;
                _context.Update(item);
                await _context.SaveChangesAsync();

            }
            // Nếu có ảnh mới, cập nhật ảnh
            if (fileAnh != null && fileAnh.Length > 0)
            {
                // Xử lý ảnh mới như cũ
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/Combo");
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
                item.Anh = "Combo/"+ uniqueFileName;
            }
            // Nếu không có ảnh mới, item.Anh giữ nguyên giá trị cũ từ cơ sở dữ liệu

            item.TenCombo = combo.TenCombo;
            item.Gia = combo.Gia;

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
                    if (!ComboExists(combo.MaCombo))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }

            return RedirectToAction("Index", "Comboes");
        }

        // POST: admins/Comboes/Delete/5 - Xóa Combo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var combo = await _context.Comboes.FindAsync(id);
            if (combo == null)
                return Json(new { success = false, message = "Không tìm thấy Combo!" });

            try
            {
                _context.Comboes.Remove(combo);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Xóa Combo thành công!" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Lỗi khi xóa Combo!" });
            }
        }

        private bool ComboExists(int id)
        {
            return _context.Comboes.Any(e => e.MaCombo == id);
        }
    }
}
