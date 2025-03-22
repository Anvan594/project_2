using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Net.payOS;
using System.Threading.Tasks;

[ApiController]
[Route("api/payos")]
[TypeFilter(typeof(CheckLoginFilter))]
public class PayOSController : ControllerBase
{
    private readonly PayOS _payOS;

    public PayOSController(PayOS payOS)
    {
        _payOS = payOS;
    }

    [HttpGet("check-order/{orderCode}")]
    public async Task<IActionResult> CheckOrderStatus(long orderCode)
    {
        try
        {
            string LinhThanhToan = HttpContext.Session.GetString("LinkThanhToan");
            // Gọi API PayOS để lấy thông tin đơn hàng
            var response = await _payOS.getPaymentLinkInformation(orderCode);
            if (response.status == "PAID")
            {
                return RedirectToAction("ThongTinVe", "DatVe", new { check = true, });

            }
            if (response.status == "PENDING")
            {
                LinhThanhToan = HttpContext.Session.GetString("LinkThanhToan");
                return Redirect(LinhThanhToan);
            }
            
            return new JsonResult(new { redirectToUrl = LinhThanhToan });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Lỗi khi kiểm tra đơn hàng", error = ex.Message });
        }
    }
}
