using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Net.payOS;
using Net.payOS.Types;
using WebBanVeXemPhim.Models;

namespace WebBanVeXemPhim.Controllers
{
    [TypeFilter(typeof(CheckLoginFilter))]
    public class CheckoutApiController : Controller
    {
        private readonly PayOS _payOS;
        private readonly QuanLyBanVeXemPhimContext _context;

        // Inject PayOS service
        public CheckoutApiController(PayOS payOS , QuanLyBanVeXemPhimContext context)
        {
            _payOS = payOS;
            _context = context;
        }
        [HttpPost]
        public async Task<IActionResult> Create(int maKhach, decimal giave,decimal tongtien, string soghe, DateTime ngaydat, string giochieu,int MaLichChieu,string TenPhim,int selectedCombo)
        {
            var tencombo = "";
            var combo=_context.Comboes.Where(c=>c.MaCombo==selectedCombo).FirstOrDefault();
            if (combo != null) { 
                tencombo = combo.TenCombo;
                tencombo = tencombo + " x" + (tongtien - giave) / combo.Gia;
                HttpContext.Session.SetString("Tencombo", tencombo);
            }
            var scheme = Request.Scheme; // http hoặc https
            var host = Request.Host.Value; // tên miền + cổng, ví dụ "localhost:7126"
            var domain = $"{scheme}://{host}";

            HttpContext.Session.SetInt32("MaCombo", selectedCombo);
            long code = int.Parse(DateTimeOffset.Now.ToString("ffffff"));
            long expirationTime = ((DateTimeOffset)DateTime.UtcNow.AddMinutes(30)).ToUnixTimeSeconds();

            int MaDonHang = (int)code;
            HttpContext.Session.SetInt32("MaDonHang", MaDonHang);
            var paymentLinkRequest = new PaymentData(
                orderCode: code,
                amount: Convert.ToInt32(tongtien),
                description: $"Thanh toán đơn hàng {MaLichChieu}",
                items: [new($"Phim:{TenPhim}, Số ghế: {soghe}, Giờ chiếu: {giochieu},{tencombo} ", 1, Convert.ToInt32(tongtien))], // Khởi tạo mảng items đúng cách
                returnUrl: domain + "/DatVe/ThanhToanThanhCong",
                cancelUrl: domain + "/DatVe/dieuhuong",
                expiredAt: expirationTime
            );
            
            var response = await _payOS.createPaymentLink(paymentLinkRequest);
            // Chuyển hướng đến trang thanh toán
            HttpContext.Session.SetString("LinkThanhToan", response.checkoutUrl);
            Response.Headers.Append("Location", response.checkoutUrl);
            return new StatusCodeResult(303);
        }
        [HttpPost]
        public async Task<IActionResult> Create_Json(int maKhach, decimal giave, string soghe, DateTime ngaydat, string giochieu, int MaLichChieu, string TenPhim,int orderId)
        {
            var domain = "https://localhost:7126";

            long code = int.Parse(DateTimeOffset.Now.ToString("ffffff"));
            long expirationTime = ((DateTimeOffset)DateTime.UtcNow.AddMinutes(10)).ToUnixTimeSeconds();

            int MaDonHang = (int)code;
            HttpContext.Session.SetInt32("MaDonHang", MaDonHang);
            var paymentLinkRequest = new PaymentData(
                orderCode: code,
                amount: Convert.ToInt32(giave),
                description: $"Thanh toán đơn hàng {MaLichChieu}",
                items: [new($"Phim:{TenPhim}, Số ghế: {soghe}, Giờ chiếu: {giochieu}, Giá vé: {giave}", 1, Convert.ToInt32(giave))], // Khởi tạo mảng items đúng cách
                returnUrl: "",
                cancelUrl: domain + "/DatVe/dieuhuong",
                expiredAt: expirationTime
            );
            var response = await _payOS.createPaymentLink(paymentLinkRequest);
            HttpContext.Session.SetString("LinkThanhToan", response.checkoutUrl);

            return Redirect("/DatVe/index?orderId=" + orderId);
        }

    }
}

