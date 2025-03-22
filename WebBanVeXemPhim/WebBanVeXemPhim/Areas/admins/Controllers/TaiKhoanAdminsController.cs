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
    public class TaiKhoanAdminsController : BaseController
    {
        private readonly QuanLyBanVeXemPhimContext _context;

        public TaiKhoanAdminsController(QuanLyBanVeXemPhimContext context)
        {
            _context = context;
        }

        // GET: admins/TaiKhoanAdmins
        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            int pageSize = 5; // Số bản ghi mỗi trang

            var query = _context.TaiKhoanAdmins.AsQueryable();

            // Nếu có giá trị tìm kiếm, lọc danh sách
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(t => t.HoTen.Contains(searchString));
            }

            // Lấy danh sách tài khoản theo phân trang
            var danhSachTaiKhoan = await query.OrderBy(t => t.MaAdmin)
                                              .Skip((page - 1) * pageSize)
                                              .Take(pageSize)
                                              .ToListAsync();

            // Tổng số trang
            int totalRecords = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchString = searchString; // Lưu giá trị tìm kiếm để hiển thị lại trên giao diện

            return View(danhSachTaiKhoan);
        }



        // GET: admins/TaiKhoanAdmins/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taiKhoanAdmin = await _context.TaiKhoanAdmins
                .FirstOrDefaultAsync(m => m.MaAdmin == id);
            if (taiKhoanAdmin == null)
            {
                return NotFound();
            }

            return PartialView("Details", taiKhoanAdmin);
        }

        // GET: admins/TaiKhoanAdmins/Create
        public IActionResult Create()
        {
            return PartialView("Create");
        }

        // POST: admins/TaiKhoanAdmins/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(TaiKhoanAdmin admin)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });
            }
            var check_Ten = _context.TaiKhoanAdmins.Any(a => a.TenDangNhap == admin.TenDangNhap);
            if (check_Ten)
            {
                return Json(new { success = false, message = "Tên đăng nhập đã tồn tại!" });
            }
            // Kiểm tra tài khoản đã tồn tại chưa
            var check = _context.TaiKhoanAdmins.Any(a => a.Email == admin.Email);
            if (check)
            {
                return Json(new { success = false, message = "Email đã tồn tại!" });
            }
            _context.TaiKhoanAdmins.Add(admin);
            _context.SaveChanges();

            return Json(new { success = true, message = "Thêm tài khoản thành công!" });
        }

        // GET: admins/TaiKhoanAdmins/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taiKhoanAdmin = await _context.TaiKhoanAdmins
                .FirstOrDefaultAsync(m => m.MaAdmin == id);
            if (taiKhoanAdmin == null)
            {
                return NotFound();
            }

            return PartialView("Edit", taiKhoanAdmin);
        }

        // POST: admins/TaiKhoanAdmins/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaAdmin,TenDangNhap,MatKhau,HoTen,Email,SoDienThoai,NgayTao,TrangThai,ChucVu")] TaiKhoanAdmin taiKhoanAdmin)
        {
            if (id != taiKhoanAdmin.MaAdmin)
            {
                return NotFound();
            }

            var existingTaiKhoan = await _context.TaiKhoanAdmins.FindAsync(id);
            if (existingTaiKhoan == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Nếu mật khẩu không nhập, giữ nguyên mật khẩu cũ
                    if (string.IsNullOrEmpty(taiKhoanAdmin.MatKhau))
                    {
                        taiKhoanAdmin.MatKhau = existingTaiKhoan.MatKhau;
                    }

                    _context.Entry(existingTaiKhoan).CurrentValues.SetValues(taiKhoanAdmin);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.TaiKhoanAdmins.Any(e => e.MaAdmin == taiKhoanAdmin.MaAdmin))
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

            return PartialView("Edit", taiKhoanAdmin);
        }


        // GET: admins/TaiKhoanAdmins/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var admin = await _context.TaiKhoanAdmins.FindAsync(id);
            if (admin == null)
            {
                return Json(new { success = false, message = "Không tìm thấy tài khoản!" });
            }

            _context.TaiKhoanAdmins.Remove(admin);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Xóa tài khoản thành công!" });
        }



        private bool TaiKhoanAdminExists(int id)
        {
            return _context.TaiKhoanAdmins.Any(e => e.MaAdmin == id);
        }
    }
}
