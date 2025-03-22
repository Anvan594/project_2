using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebBanVeXemPhim.Models;

namespace WebBanVeXemPhim.Areas.admins.Controllers
{
    public class BannersController : BaseController
    {
        private readonly QuanLyBanVeXemPhimContext _context;

        public BannersController(QuanLyBanVeXemPhimContext context)
        {
            _context = context;
        }

        // GET: admins/Banners
        public async Task<IActionResult> Index()
        {
            return View(await _context.Banners.ToListAsync());
        }

        // GET: admins/Banners/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var banner = await _context.Banners
                .FirstOrDefaultAsync(m => m.MaBanner == id);
            if (banner == null)
            {
                return NotFound();
            }

            return View(banner);
        }

        // GET: admins/Banners/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: admins/Banners/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Anh,MoTa,TrangThai,MaBanner")] Banner banner)
        {
            if (ModelState.IsValid)
            {
                _context.Add(banner);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(banner);
        }

        // GET: admins/Banners/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var banner = await _context.Banners.FindAsync(id);
            ViewBag.Anh = banner.Anh;
            if (banner == null)
            {
                return NotFound();
            }
            return PartialView("Edit", banner);
        }

        // POST: admins/Banners/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IFormFile fileAnh, [Bind("MaBanner,MoTa,TrangThai")] Banner banner)
        {
            if (id != banner.MaBanner)
            {
                return NotFound();
            }

            var item = await _context.Banners.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            if (fileAnh == null)
            {
                item.MoTa = banner.MoTa;
                item.TrangThai = banner.TrangThai;
                _context.Update(item);
                await _context.SaveChangesAsync();

            }
            // Nếu có ảnh mới, cập nhật ảnh
            if (fileAnh != null && fileAnh.Length > 0)
            {
                // Xử lý ảnh mới như cũ
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
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
                item.Anh = uniqueFileName;
            }
            // Nếu không có ảnh mới, item.Anh giữ nguyên giá trị cũ từ cơ sở dữ liệu

            item.MoTa = banner.MoTa;
            item.TrangThai = banner.TrangThai;

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
                    if (!BannerExists(banner.MaBanner))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }

            return RedirectToAction("Index", "Banners");
        }


            // GET: admins/Banners/Delete/5
            public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var banner = await _context.Banners
                .FirstOrDefaultAsync(m => m.MaBanner == id);
            if (banner == null)
            {
                return NotFound();
            }

            return View(banner);
        }

        // POST: admins/Banners/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner != null)
            {
                _context.Banners.Remove(banner);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BannerExists(int id)
        {
            return _context.Banners.Any(e => e.MaBanner == id);
        }
    }
}
