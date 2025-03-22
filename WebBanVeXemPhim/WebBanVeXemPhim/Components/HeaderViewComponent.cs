using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using WebBanVeXemPhim.Models;
using Microsoft.AspNetCore.Http.HttpResults;

public class HeaderViewComponent : ViewComponent
{
    private readonly QuanLyBanVeXemPhimContext _context;

    public HeaderViewComponent(QuanLyBanVeXemPhimContext context)
    {
        _context = context;
    }
    public IViewComponentResult Invoke()
    {
        var MaNguoiDung = HttpContext.Session.GetInt32("NguoiDung") ?? 0;
        var ThongBao = _context.ThongBaos
    .Where(t => t.MaNguoiDung == MaNguoiDung)
    .OrderByDescending(t => t.NgayGui) // Sắp xếp từ mới nhất đến cũ nhất
    .ToArray(); // Chuyển danh sách thành mảng
        var TbChuaDoc = _context.ThongBaos.Where(t => t.MaNguoiDung == MaNguoiDung && t.TrangThai == false).ToArray();
        var SoThongBao = TbChuaDoc.Length;
        ViewBag.ThongBao=ThongBao;
        return View("ThongBao", SoThongBao);
    }
}
