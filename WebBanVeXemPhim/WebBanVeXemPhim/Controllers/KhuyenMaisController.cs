using Microsoft.AspNetCore.Mvc;
using WebBanVeXemPhim.Models;

namespace WebBanVeXemPhim.Controllers
{
    public class KhuyenMaisController : Controller
    {
        private readonly QuanLyBanVeXemPhimContext _context;
        public KhuyenMaisController(QuanLyBanVeXemPhimContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var khuyenmai=_context.KhuyenMais.ToList();
            return View(khuyenmai);
        }
    }
}
