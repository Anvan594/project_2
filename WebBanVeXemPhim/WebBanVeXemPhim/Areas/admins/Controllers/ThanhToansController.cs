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
    public class ThanhToansController : BaseController
    {
        private readonly QuanLyBanVeXemPhimContext _context;

        public ThanhToansController(QuanLyBanVeXemPhimContext context)
        {
            _context = context;
        }

        // GET: admins/ThanhToans
        public async Task<IActionResult> Index(int? pageNumber)
        {
            int pageSize = 8; // Số bản ghi trên mỗi trang
            int currentPage = pageNumber ?? 1; // Mặc định trang đầu tiên
            var query = _context.ThanhToans
                .Include(t => t.MaVeNavigation)
                .OrderByDescending(t => t.NgayThanhToan);
            // Sắp xếp theo ID hoặc một trường phù hợp

            var allRecords = await query.ToListAsync(); // Lấy toàn bộ dữ liệu từ DB

            // Tính tổng số trang
            int totalRecords = allRecords.Count;
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            // Lấy dữ liệu cho trang hiện tại
            var paginatedList = allRecords.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();

            // Truyền dữ liệu phân trang vào ViewBag
            ViewBag.CurrentPage = currentPage;
            ViewBag.TotalPages = totalPages;

            return View(paginatedList);
        }
        private bool ThanhToanExists(int id)
        {
            return _context.ThanhToans.Any(e => e.MaThanhToan == id);
        }
    }
}
