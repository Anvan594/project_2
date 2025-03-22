using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using WebBanVeXemPhim.Models;
using System.Transactions;

namespace WebBanVeXemPhim.Controllers
{
    [TypeFilter(typeof(CheckLoginFilter))]
    public class DatVeController : Controller
    {
        private readonly QuanLyBanVeXemPhimContext _context;
		private readonly ILogger<DatVeController> _logger;
		private readonly IHttpClientFactory _httpClientFactory;
		public DatVeController(QuanLyBanVeXemPhimContext context, ILogger<DatVeController> logger, IHttpClientFactory httpClientFactory)
        {
            _context = context;
			_logger = logger;
			_httpClientFactory = httpClientFactory;
		}
        
        [HttpPost]
        public IActionResult XacNhanDatVe([FromBody] DatVeRequest request)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var ticketIds = DatVe(request);

            if (ticketIds == null || ticketIds.Count == 0)
            {
                return BadRequest(new { message = "Không thể đặt vé. Vui lòng thử lại." });
            }

            return Ok(new { ticketIds, message = "Đặt vé thành công!" });
        }

        public List<int> DatVe(DatVeRequest request)
        {
            int MaNguoiDung = HttpContext.Session.GetInt32("NguoiDung") ?? 0;
            List<int> ticketIds = new List<int>();
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    foreach (var maGhe in request.Seats)
                    {
                        bool isSeatBooked = _context.Ves.Any(v => v.MaLichChieu == request.MaLichChieu && v.MaGhe == maGhe);
                        if (isSeatBooked)
                        {
                            throw new Exception($"Ghế {maGhe} đã có người đặt. Vui lòng chọn ghế khác!");
                        }

                        var ve = new Ve
                        {
                            MaLichChieu = request.MaLichChieu,
                            MaGhe = maGhe,
                            MaKhachHang = MaNguoiDung,
                            GiaVe = request.TotalPrice / request.SoSeats,
                            NgayDat = DateTime.Now,
                            TrangThai = false
                        };

                        _context.Ves.Add(ve);
                        _context.SaveChanges();

                        ticketIds.Add(ve.MaVe);
                    }

                    transaction.Commit();
                    return ticketIds;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine("Lỗi đặt vé: " + ex.Message);
                    return new List<int>();
                }
            }
        }

        public IActionResult Index(int orderId)
        {
            var combo =_context.Comboes.ToList();
            ViewBag.combo = combo;
            var ve = _context.Ves
                .Include(v => v.MaGheNavigation)
                .Where(v => v.MaVe == orderId && v.TrangThai==false)
                .Select(v => new
                {
                    v.MaLichChieu,
                    v.MaKhachHang
                })
                .FirstOrDefault();
            var order = _context.Ves
                .Include(v => v.MaGheNavigation)
                .Include(v => v.MaLichChieuNavigation)
                .Include(v => v.MaLichChieuNavigation.MaPhimNavigation)
                .Include(v => v.MaKhachHangNavigation)
                .Where(v => v.MaKhachHang == ve.MaKhachHang && v.MaLichChieu == ve.MaLichChieu && v.TrangThai == false)
                .Select(v => new
                {
                    v.MaVe,
                    Anh=v.MaLichChieuNavigation.MaPhimNavigation.Poster,
                    ThoiLuong = v.MaLichChieuNavigation.MaPhimNavigation.ThoiLuong,
                    GioChieu = v.MaLichChieuNavigation.GioChieu,
                    NgayChieu=v.MaLichChieuNavigation.NgayChieu,
                    v.MaLichChieu,
                    v.MaKhachHang,
                    TenPhim = v.MaLichChieuNavigation.MaPhimNavigation.TenPhim,
                    TenKhach = v.MaKhachHangNavigation.TenNguoiDung,
                    SoGhe = v.MaGheNavigation != null ? v.MaGheNavigation.SoGhe : "Chưa rõ",
                    v.MaGhe,
                    v.GiaVe,
                    v.NgayDat
                })
                .ToList();

            if (order == null)
            {
                return NotFound("Không tìm thấy đơn hàng!");
            }
            ViewBag.OrderId = orderId;
            return View(order);
        }
        public async Task<IActionResult> ThongTinVe( bool check)
        {
            int? MaCombo = HttpContext.Session.GetInt32("MaCombo") ?? null;
            var tencombo = HttpContext.Session.GetString("Tencombo") ?? "";
            tencombo = "<br/>"+tencombo;
            if (check == true)
            {
                int MaNguoiDung = HttpContext.Session.GetInt32("NguoiDung") ?? 0;

                var order = _context.Ves
                    .Include(v => v.MaGheNavigation)
                    .Include(v => v.MaLichChieuNavigation)
                    .Include(v => v.MaLichChieuNavigation.MaPhimNavigation)
                    .Include(v => v.MaLichChieuNavigation.MaPhongNavigation)
                    .Where(v => v.MaKhachHang == MaNguoiDung && v.TrangThai == false)
                    .Select(v => new
                    {
                        SoGhe = v.MaGheNavigation.SoGhe,
                        SoPhong = v.MaLichChieuNavigation.MaPhongNavigation.TenPhong,
                        GioChieu = v.MaLichChieuNavigation.GioChieu,
                        Ngaychieu = v.MaLichChieuNavigation.NgayChieu,
                        TenPhim = v.MaLichChieuNavigation.MaPhimNavigation.TenPhim,
                        ThoiLuong = v.MaLichChieuNavigation.MaPhimNavigation.ThoiLuong,
                        v.GiaVe,
                        v.MaVe,
                        v.MaLichChieu,
                        v.MaKhachHang
                    })
                    .ToArray();
                ViewBag.VeDaMua = order;
                int MaLichChieu = order[0].MaLichChieu;
                if (!order.Any())
                {
                    return NotFound("Không tìm thấy đơn hàng!");
                }

                var payments = new List<ThanhToan>();
                var SoGhe_ThongBao="";
                foreach (var item in order)
                {
                    if (!string.IsNullOrEmpty(SoGhe_ThongBao))
                    {
                        SoGhe_ThongBao += ", ";
                    }
                    SoGhe_ThongBao += item.SoGhe;

                    var existingPayment = _context.ThanhToans.FirstOrDefault(p => p.MaVe == item.MaVe);
                    if (existingPayment != null)
                    {
                        return BadRequest("Vé này đã được thanh toán!");
                    }

                    payments.Add(new ThanhToan
                    {
                        MaVe = item.MaVe,
                        MaComBo=MaCombo,
                        PhuongThuc = "Chuyển khoản qua ngân hàng",
                        NgayThanhToan = DateTime.Now,
                        TrangThai = "Đã Thanh Toán"
                    });
                }
                var ThongBao = new ThongBao
                {
                    MaNguoiDung = MaNguoiDung,
                    NoiDung ="Bạn đã đặt vé thành công<br/>Tên Phim: " + order[0].TenPhim+"<br/>Ngày chiếu:"+order[0].Ngaychieu+" Giờ chiếu: " + order[0].GioChieu + "<br/> Số ghế: "+SoGhe_ThongBao+tencombo
                };
                _context.ThongBaos.Add(ThongBao);
                _context.ThanhToans.AddRange(payments);
                await _context.SaveChangesAsync();

                await UpdateVe(MaNguoiDung, MaLichChieu);
                var ChucVu = HttpContext.Session.GetString("ChucVu") ?? "";
                if (ChucVu == "Nhan Vien")
                {
                    return RedirectToAction("Index", "NhanVien");
                }
                return RedirectToAction("Index", "Home");
              
            }
            return RedirectToAction("ThanhToanThanhCong", "DatVe");

        }


        public IActionResult Success(int orderId)
        {
            return View(orderId);
        }
        public async Task<IActionResult> dieuhuong()
        {
            int MaNguoiDung = HttpContext.Session.GetInt32("NguoiDung")??0;
            if (MaNguoiDung != 0)
            {
                var veCanXoa = await _context.Ves
               .Where(v => v.TrangThai == false && v.MaKhachHang == MaNguoiDung)
               .ToListAsync();
                _context.Ves.RemoveRange(veCanXoa);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }
            return RedirectToAction("Index", "Home");
        }
        public IActionResult ThanhToanThanhCong()
        {
            long MaDonHang = HttpContext.Session.GetInt32("MaDonHang") ?? 0;
            return RedirectToAction("CheckOrderStatus", "PayOS", new { orderCode = MaDonHang });

        }
        public async Task<IActionResult> UpdateVe(int MaNguoiDung, int MaLichChieu)
        {
            // Tìm vé cần cập nhật
            var veCanUpdate = _context.Ves
                .Where(v => v.TrangThai == false && v.MaKhachHang == MaNguoiDung && v.MaLichChieu==MaLichChieu)
                .ToList(); // Thêm `await` vì đây là truy vấn async

            // Kiểm tra nếu không tìm thấy vé nào
            if (veCanUpdate == null)
            {
                return NotFound("Không tìm thấy vé nào cần cập nhật!");
            }
            foreach (var ve in veCanUpdate)
            {
                ve.TrangThai = true;
               
            }
            await _context.SaveChangesAsync();
            // Cập nhật trạng thái của vé

            // Lưu thay đổi vào database
            return Ok("Cập nhật vé thành công!");
        }
        public async Task<IActionResult> XoaVe(int MaLichChieu)
        {
            var currentTime = DateTime.Now;
            int MaKhachHang = HttpContext.Session.GetInt32("NguoiDung") ?? 0;
            // Lọc các vé có trạng thái là false và thời gian đặt vé quá 10 phút
            var veCanXoa = await _context.Ves
                .Where(v => v.TrangThai == false && v.MaKhachHang == MaKhachHang && v.MaLichChieu == MaLichChieu&&
                            EF.Functions.DateDiffMinute(v.NgayDat, currentTime) > 10)
                .ToListAsync();
            _context.Ves.RemoveRange(veCanXoa);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
		[HttpPost]
		public IActionResult CreateTransaction(float amount)
		{
            Console.WriteLine(amount);
			string transactionId = $"TX{DateTime.Now:yyyyMMddHHmmss}{new Random().Next(1000, 9999)}";
        //https://api.vietqr.io/image/970422-3120182092003-e2ryJKx.jpg?accountName=NGUYEN%20VAN%20AN&amount=&amount={amount}&des={transactionId}
            string qrUrl = $"https://api.vietqr.io/image/970422-VQRQABSTC4478-e2ryJKx.jpg?accountName=NGUYEN%20VAN%20AN&amount={amount}&addInfo={transactionId}";
			return Json(new { transactionId, qrUrl });
		}

		[HttpGet]
		public async Task<IActionResult> TimGiaoDich(string content)
		{
			try
			{
				var client = _httpClientFactory.CreateClient();
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "XICNVOG02WQR45X1BOCBSKZTIIUYJTPZAJE9FADSRPBE6LWPL5QWHZTXD7FRIDPM");

				var response = await client.GetAsync("https://my.sepay.vn/userapi/transactions/list");

				if (!response.IsSuccessStatusCode)
				{
					_logger.LogError("Lỗi khi gọi API: {StatusCode}", response.StatusCode);
					return StatusCode((int)response.StatusCode);
				}

				var responseString = await response.Content.ReadAsStringAsync();
				var jsonObject = JObject.Parse(responseString);
                var success = jsonObject["messages"]?["success"]?.ToString();
                // Kiểm tra transactions tồn tại
                var transactions = jsonObject["transactions"];
				if (transactions == null || !transactions.Any())
				{
					return Json(new { message = "Không tìm thấy giao dịch nào!" });
				}

				// Regex tách mã giao dịch dạng "TX..."
				string ExtractTransactionId(string transactionContent)
				{
					var match = Regex.Match(transactionContent, @"TX\d+");
					return match.Success ? match.Value : null;
				}

				// Tìm kiếm giao dịch với mã TX đúng bằng content
				var matchedTransaction = transactions
					.Select(t => new
					{
						TransactionContent = t["transaction_content"]?.ToString(),
                        Amount=t["amount_in"]?.ToString(),
                        TransactionId = ExtractTransactionId(t["transaction_content"]?.ToString() ?? ""),
                        Success = success
                    })
					.FirstOrDefault(t => t.TransactionId == content&&t.TransactionId!=null);

				if (matchedTransaction == null)
				{
					return Json(new { message = "Không tìm thấy giao dịch nào!" });
				}

				return Json(matchedTransaction);
			}
			catch (Exception ex)
			{
				_logger.LogError("Lỗi khi tìm giao dịch: {Message}", ex.Message);
				return StatusCode(500, "Đã xảy ra lỗi!");
			}
		}
		public class DatVeRequest
        {
            [Required(ErrorMessage = "Mã lịch chiếu không được để trống.")]
            public int MaLichChieu { get; set; }

            [Required(ErrorMessage = "Vui lòng chọn ít nhất một ghế.")]
            public List<int> Seats { get; set; } = new List<int>();

            [Required]
            [Range(1, int.MaxValue, ErrorMessage = "Số ghế phải lớn hơn 0.")]
            public int SoSeats { get; set; }

            [Required]
            [Range(1000, double.MaxValue, ErrorMessage = "Tổng tiền phải lớn hơn 1.000 VNĐ.")]
            public decimal TotalPrice { get; set; }
        }
    }
}
