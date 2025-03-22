using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using WebBanVeXemPhim.Models;
using Microsoft.EntityFrameworkCore;
using WebBanVeXemPhim.Areas.admins.Controllers;

namespace WebBanVeXemPhim.Controllers
{
    public class BaoCaoController : BaseController
    {
        private readonly QuanLyBanVeXemPhimContext _context;

        public BaoCaoController(QuanLyBanVeXemPhimContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;

            var revenueToday = await _context.Ves
                .Where(v => v.NgayDat.HasValue && v.NgayDat.Value.Date == today)
                .SumAsync(v => v.GiaVe);

            var ticketsSoldToday = await _context.Ves
                .Where(v => v.NgayDat.HasValue && v.NgayDat.Value.Date == today)
                .CountAsync();

            var currentMonth = DateTime.Today.Month;
            var currentYear = DateTime.Today.Year;

            var TopPhim = await _context.Ves
                .Include(v => v.MaLichChieuNavigation.MaPhimNavigation) // Load thông tin phim
                .Where(v => v.NgayDat.HasValue &&
                            v.NgayDat.Value.Month == currentMonth &&
                            v.NgayDat.Value.Year == currentYear) // Chỉ lấy vé trong tháng hiện tại
                .GroupBy(v => new
                {
                    v.MaLichChieuNavigation.MaPhim,
                    TenPhim = v.MaLichChieuNavigation.MaPhimNavigation.TenPhim,
                    AnhPhim = v.MaLichChieuNavigation.MaPhimNavigation.Poster, // Lấy ảnh phim
                    DaoDien = v.MaLichChieuNavigation.MaPhimNavigation.DaoDien // Lấy đạo diễn
                })
                .Select(g => new
                {
                    MaPhim = g.Key.MaPhim,
                    TenPhim = g.Key.TenPhim,
                    AnhPhim = g.Key.AnhPhim, // Gán ảnh phim vào kết quả
                    DaoDien = g.Key.DaoDien, // Gán đạo diễn vào kết quả
                    SoVe = g.Count() // Đếm số vé bán ra
                })
                .OrderByDescending(g => g.SoVe) // Sắp xếp giảm dần theo số vé
                .Take(5) // Lấy top 5 phim bán chạy nhất
                .ToListAsync();



            ViewData["RevenueToday"] = revenueToday;
            ViewData["TicketsSoldToday"] = ticketsSoldToday;
            ViewData["TopMovies"] = TopPhim;
            ViewBag.TopPhim = TopPhim;

            return View();
        }


        public async Task<IActionResult> XuatBaoCao(string kieuBaoCao, DateTime? ngay, string thangNam, int? nam, DateTime? tuNgay, DateTime? denNgay)
        {
            var query = _context.Ves.AsQueryable();

            if (kieuBaoCao == "ngay" && ngay.HasValue)
            {
                query = query.Where(v => v.NgayDat.HasValue && v.NgayDat.Value.Date == ngay.Value.Date);
            }
            else if (kieuBaoCao == "thang" && !string.IsNullOrEmpty(thangNam))
            {
                var thang = int.Parse(thangNam.Split('-')[1]); // Lấy tháng từ yyyy-MM
                var namThang = int.Parse(thangNam.Split('-')[0]); // Lấy năm từ yyyy-MM
                query = query.Where(v => v.NgayDat.HasValue && v.NgayDat.Value.Month == thang && v.NgayDat.Value.Year == namThang);
            }
            else if (kieuBaoCao == "nam" && nam.HasValue)
            {
                query = query.Where(v => v.NgayDat.HasValue && v.NgayDat.Value.Year == nam.Value);
            }
            else if (kieuBaoCao == "khoangNgay" && tuNgay.HasValue && denNgay.HasValue)
            {
                query = query.Where(v => v.NgayDat.HasValue && v.NgayDat.Value.Date >= tuNgay.Value.Date && v.NgayDat.Value.Date <= denNgay.Value.Date);
            }

            var tickets = await query
                .GroupBy(v => new
                {
                    v.MaLichChieuNavigation.MaPhim,
                    TenPhim = v.MaLichChieuNavigation.MaPhimNavigation.TenPhim
                })
                .Select(g => new
                {
                    g.Key.MaPhim,
                    g.Key.TenPhim,
                    TongSoVe = g.Count(),
                    TongTienVe = g.Sum(v => v.GiaVe)
                })
                .ToListAsync();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Báo cáo");
                decimal tongDoanhSo = 0;
                worksheet.Cells["A1:E1"].Merge = true;
                worksheet.Cells["A1"].Value = "Báo cáo doanh thu bán vé: " +
                    (kieuBaoCao == "ngay" ? "Ngày " + ngay?.ToString("dd/MM/yyyy") :
                    kieuBaoCao == "thang" ? "Tháng " + thangNam :
                    kieuBaoCao == "nam" ? "Năm " + nam :
                    kieuBaoCao == "khoangNgay" ? $"Từ {tuNgay?.ToString("dd/MM/yyyy")} đến {denNgay?.ToString("dd/MM/yyyy")}" : "Tất cả");

                worksheet.Cells["A1"].Style.Font.Size = 14;
                worksheet.Cells["A1"].Style.Font.Bold = true;

                worksheet.Cells["A3"].Value = "Mã phim";
                worksheet.Cells["B3"].Value = "Tên phim";
                worksheet.Cells["C3"].Value = "Tổng số vé";
                worksheet.Cells["D3"].Value = "Tổng tiền vé";

                int row = 4;
                foreach (var ticket in tickets)
                {
                    worksheet.Cells[row, 1].Value = ticket.MaPhim;
                    worksheet.Cells[row, 2].Value = ticket.TenPhim;
                    worksheet.Cells[row, 3].Value = ticket.TongSoVe;
                    worksheet.Cells[row, 4].Value = ticket.TongTienVe;
                    worksheet.Cells[row, 4].Style.Numberformat.Format = "#,##0₫";
                    row++;
                    tongDoanhSo += ticket.TongTienVe;
                }
                worksheet.Cells[row,1,row,4].Style.Font.Size = 14;
                worksheet.Cells[row, 1, row, 3].Merge = true;
                worksheet.Cells[row, 1,row,3].Value = "Tổng doanh số:";
                worksheet.Cells[row, 3].Style.Font.Bold = true;
                worksheet.Cells[row, 4].Value = tongDoanhSo;
                worksheet.Cells[row, 4].Style.Font.Bold = true;
                worksheet.Cells[row, 4].Style.Numberformat.Format = "#,##0₫"; // Hiển thị tiền Việt

                // Căn giữa dòng tổng doanh số
                worksheet.Cells[row, 3, row, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells.AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "BaoCaoBanVe.xlsx");
            }
        }


    }
}
