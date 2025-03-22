using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using WebBanVeXemPhim.Models;

namespace WebBanVeXemPhim.Areas.admins.Controllers
{
    public class PhimsController : BaseController
    {
        private readonly QuanLyBanVeXemPhimContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PhimsController(IWebHostEnvironment webHostEnvironment, QuanLyBanVeXemPhimContext context)
        {
            _webHostEnvironment = webHostEnvironment;
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            int pageSize = 3;
            var query = _context.Phims.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.TenPhim.Contains(searchString));
            }

            // Tính tổng số bản ghi trước khi phân trang
            int totalRecords = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            var danhSachPhim = await query.OrderByDescending(p => p.NgayKhoiChieu) // Sắp xếp giảm dần theo ngày phát hành
                                          .Skip((page - 1) * pageSize)
                                          .Take(pageSize)
                                          .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchString = searchString;

            return View(danhSachPhim);


        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phim = await _context.Phims.FirstOrDefaultAsync(m => m.MaPhim == id);
            if (phim == null)
            {
                return NotFound();
            }

            return PartialView("Details", phim);
        }

        public IActionResult Create()
        {
            return PartialView("Create");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenPhim,TheLoai,DoTuoi,ThoiLuong,NgayKhoiChieu,MoTa,TrangThai,DaoDien,DienVien,NgonNgu")] Phim phim, IFormFile PosterFile)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join(", ", errors) });
            }

            if (PosterFile != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + PosterFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await PosterFile.CopyToAsync(fileStream);
                }

                phim.Poster = uniqueFileName;
            }

            _context.Add(phim);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Thêm phim thành công!" });
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                Console.WriteLine("Lỗi: ID phim không hợp lệ");
                return BadRequest("ID không hợp lệ.");
            }

            var phim = await _context.Phims.FirstOrDefaultAsync(m => m.MaPhim == id);
            if (phim == null)
            {
                Console.WriteLine($"Lỗi: Không tìm thấy phim có ID = {id}");
                return NotFound("Không tìm thấy phim.");
            }

            return PartialView("Edit", phim);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(int id, Phim phim, IFormFile? PosterFile)
        {
            if (id != phim.MaPhim)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingPhim = await _context.Phims.FindAsync(id);
                    if (existingPhim == null)
                    {
                        return NotFound();
                    }

                    // Kiểm tra nếu có file ảnh mới được tải lên
                    if (PosterFile != null && PosterFile.Length > 0)
                    {
                        var fileName = Path.GetFileName(PosterFile.FileName);
                        var filePath = Path.Combine("wwwroot/images", fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await PosterFile.CopyToAsync(stream);
                        }

                        phim.Poster =  fileName; // Lưu đường dẫn mới vào DB
                    }
                    else
                    {
                        phim.Poster = existingPhim.Poster; // Giữ nguyên đường dẫn ảnh cũ
                    }

                    // Cập nhật các thông tin khác
                    existingPhim.TenPhim = phim.TenPhim;
                    existingPhim.TheLoai = phim.TheLoai;
                    existingPhim.DoTuoi = phim.DoTuoi;
                    existingPhim.DaoDien = phim.DaoDien;
                    existingPhim.DienVien = phim.DienVien;
                    existingPhim.NgonNgu = phim.NgonNgu;
                    existingPhim.ThoiLuong = phim.ThoiLuong;
                    existingPhim.NgayKhoiChieu = phim.NgayKhoiChieu;
                    existingPhim.MoTa = phim.MoTa;
                    existingPhim.TrangThai = phim.TrangThai;
                    existingPhim.Poster = phim.Poster; // Giữ hoặc cập nhật ảnh

                    _context.Update(existingPhim);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Phims.Any(e => e.MaPhim == id))
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
            return View(phim);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var phim = await _context.Phims.FindAsync(id);
            if (phim == null)
            {
                return Json(new { success = false, message = "Không tìm thấy phim!" });
            }

            _context.Phims.Remove(phim);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Xóa phim thành công!" });
        }

        private bool PhimExists(int id)
        {
            return _context.Phims.Any(e => e.MaPhim == id);
        }
    }
}