using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WebBanVeXemPhim.Models;
using System.Dynamic;
namespace WebBanVeXemPhim.Areas.admins.Controllers
{
    public class VesController : BaseController
    {
        private readonly QuanLyBanVeXemPhimContext _context;

        public VesController(QuanLyBanVeXemPhimContext context)
        {
            _context = context;
        }



public async Task<IActionResult> Index(int? pageNumber)
    {
        int pageSize = 10;
        int currentPage = pageNumber ?? 1;

        var allTickets = await _context.Ves
            .Include(v => v.MaGheNavigation)
            .Include(v => v.MaKhachHangNavigation)
            .Include(v => v.MaLichChieuNavigation)
            .ThenInclude(lc => lc.MaPhimNavigation)
            .OrderBy(v => v.MaKhachHang)
            .ThenBy(v => v.MaLichChieu)
            .ThenBy(v => v.MaGhe).OrderByDescending(t => t.NgayDat)
            .ToListAsync();

            var mergedList = new List<dynamic>();

        foreach (var item in allTickets)
        {
            var previousItem = mergedList.LastOrDefault(v => v.MaKhachHang == item.MaKhachHang && v.MaLichChieu == item.MaLichChieu);
            if (previousItem != null)
            {
                previousItem.SoGhe += ", " + item.MaGheNavigation.SoGhe;
                previousItem.GiaVe += item.GiaVe;
            }
            else
            {
                dynamic newItem = new ExpandoObject();
                newItem.MaKhachHang = item.MaKhachHang;
                newItem.TenNguoiDung = item.MaKhachHangNavigation.TenNguoiDung;
                newItem.TenPhim = item.MaLichChieuNavigation.MaPhimNavigation.TenPhim;
                newItem.SoGhe = item.MaGheNavigation.SoGhe;
                newItem.NgayDat = item.NgayDat?.ToString("dd/MM/yyyy HH:mm") ?? "N/A";
                newItem.GiaVe = item.GiaVe;
                newItem.MaLichChieu = item.MaLichChieu;

                mergedList.Add(newItem);
            }
        }

        // Phân trang
        var paginatedList = mergedList.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
        ViewBag.CurrentPage = currentPage;
        ViewBag.TotalPages = (int)Math.Ceiling((double)mergedList.Count / pageSize);

        return View(paginatedList);
    }


    private bool VeExists(int id)
        {
            return _context.Ves.Any(e => e.MaVe == id);
        }
    }
}
