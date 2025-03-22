using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WebBanVeXemPhim.Models;

namespace WebBanVeXemPhim.Controllers
{
    public class NhanVienController : Controller
    {
        private readonly QuanLyBanVeXemPhimContext _context;

        public NhanVienController(QuanLyBanVeXemPhimContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> IndexAsync()
        {
            var ChucVu = HttpContext.Session.GetString("ChucVu") ?? "";
            Console.WriteLine("chức vụ là :"+ChucVu);
            if (ChucVu != "Nhan Vien")
            {
                return RedirectToAction("Index", "Home");
            }
            int MaKhachHang = HttpContext.Session.GetInt32("NguoiDung") ?? 0;
            var danhSachPhim = _context.Phims.ToList(); // Lấy danh sách phim từ database
            ViewBag.DanhSachPhim = danhSachPhim;

            var danhSachMaVeThanhToan = _context.ThanhToans.Select(t => t.MaVe).ToList();

            // Cập nhật trạng thái vé đã thanh toán
            var danhSachVeThanhToan = _context.Ves
                .Where(v => danhSachMaVeThanhToan.Contains(v.MaVe) && v.TrangThai == false)
                .ToList();

            foreach (var ve in danhSachVeThanhToan)
            {
                ve.TrangThai = true;
            }
            var DanhSachVe = await _context.Ves.AsNoTracking()
             .Include(v => v.MaGheNavigation) // Load thông tin ghế
             .Select(v => new
             {
                 MaVe = v.MaVe,         // Mã vé
                 Giave = v.GiaVe,
                 MaLichChieu = v.MaLichChieu, // Mã lịch chiếu
                 MaGhe = v.MaGhe,       // Mã ghế
                 SoGhe = v.MaGheNavigation.SoGhe, // Số ghế
                 TrangThai = v.TrangThai // Trạng thái vé (true: đã đặt, false: còn trống)
             })
             .ToListAsync();
            var danhSachGhe = await _context.Ghes.AsNoTracking()
             .Select(g => new
             {
                 g.LoaiGhe,
                 g.MaGhe,   // Mã ghế
                 g.SoGhe,   // Số ghế (A1, A2, B1, B2)
                 g.MaPhong  // Mã phòng
             })
             .OrderBy(g => g.SoGhe) // Sắp xếp theo số ghế
             .ToListAsync();
            var danhSachLichChieu = await _context.LichChieus.AsNoTracking()
            .Include(lc => lc.MaPhongNavigation) // Load phòng chiếu
            .Select(lc => new
            {
                lc.NgayChieu,
                lc.GioChieu,
                lc.MaPhim,
                lc.MaLichChieu,
                lc.MaPhong,
                lc.GiaVe,
                TongGhe = lc.MaPhongNavigation.SoLuongGhe,
                SoGheDaDat = _context.Ves
                    .Where(dv => dv.MaLichChieu == lc.MaLichChieu && dv.TrangThai == true)
                    .Count()
            })
            .OrderBy(lc => lc.NgayChieu) // 🟢 Sắp xếp theo ngày chiếu tăng dần
            .ThenBy(lc => lc.GioChieu)  // 🟢 Sắp xếp tiếp theo giờ chiếu tăng dần
            .ToListAsync();

            // Tính số ghế trống và truyền vào ViewBag
            ViewBag.DanhSachLichChieu = danhSachLichChieu.Select(lc => new
            {
                lc.NgayChieu,
                lc.GioChieu,
                lc.MaPhim,
                lc.MaLichChieu,
                lc.MaPhong,
                lc.TongGhe,
                lc.GiaVe,
                TongGheTrong = lc.TongGhe - lc.SoGheDaDat

            }).ToList();
            ViewBag.DanhSachGhe = danhSachGhe;
            ViewBag.DanhSachVe = DanhSachVe;
            return View();
        }
        public async Task<IActionResult> InVe()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Inve(int MaKhach, int MaLichChieu)
        {
            var danhSachVe = _context.Ves
              .Include(v => v.MaGheNavigation)
              .Include(v => v.MaLichChieuNavigation)
              .Include(v => v.MaLichChieuNavigation.MaPhimNavigation)
              .Include(v => v.MaLichChieuNavigation.MaPhongNavigation)
              .Where(v => v.MaKhachHang == MaKhach&& v.MaLichChieu==MaLichChieu && v.TrangThai == true)
              .OrderByDescending(v => v.NgayDat)
              .ThenBy(v => v.MaLichChieu) // Giữ thứ tự ban đầu
              .ThenBy(v => v.MaVe) // Đảm bảo vé cùng lịch chiếu giữ đúng thứ tự
              .ToList();

            var result = new List<object>();
            int? currentMaLichChieu = null;
            string soghe = "";
            decimal giave = 0;

            foreach (var ve in danhSachVe)
            {
                if (currentMaLichChieu == null || ve.MaLichChieu != currentMaLichChieu)
                {
                    // Nếu không phải dòng đầu tiên, thêm dữ liệu đã gộp vào danh sách kết quả
                    if (currentMaLichChieu != null)
                    {
                        result.Add(new
                        {
                            MaLichChieu = currentMaLichChieu,
                            ThoiLuong = danhSachVe.First(v => v.MaLichChieu == currentMaLichChieu).MaLichChieuNavigation.MaPhimNavigation.ThoiLuong,
                            TenPhim = danhSachVe.First(v => v.MaLichChieu == currentMaLichChieu).MaLichChieuNavigation.MaPhimNavigation.TenPhim,
                            NgayChieu = danhSachVe.First(v => v.MaLichChieu == currentMaLichChieu).MaLichChieuNavigation.NgayChieu,
                            GioChieu = danhSachVe.First(v => v.MaLichChieu == currentMaLichChieu).MaLichChieuNavigation.GioChieu,
                            SoPhong = danhSachVe.First(v => v.MaLichChieu == currentMaLichChieu).MaLichChieuNavigation.MaPhongNavigation.TenPhong,
                            SoGhe = soghe.TrimEnd(',', ' '), // Bỏ dấu phẩy cuối cùng
                            GiaVe = giave
                        });
                    }

                    // Reset dữ liệu cho nhóm mới
                    currentMaLichChieu = ve.MaLichChieu;
                    soghe = ve.MaGheNavigation.SoGhe + ", ";
                    giave = ve.GiaVe;
                }
                else
                {
                    // Nếu cùng lịch chiếu, tiếp tục ghép ghế & cộng tiền
                    soghe += ve.MaGheNavigation.SoGhe + ", ";
                    giave += ve.GiaVe;
                }
            }

            // Thêm nhóm cuối cùng vào danh sách kết quả
            if (currentMaLichChieu != null)
            {
                result.Add(new
                {

                    MaLichChieu = currentMaLichChieu,
                    ThoiLuong = danhSachVe.First(v => v.MaLichChieu == currentMaLichChieu).MaLichChieuNavigation.MaPhimNavigation.ThoiLuong,
                    TenPhim = danhSachVe.First(v => v.MaLichChieu == currentMaLichChieu).MaLichChieuNavigation.MaPhimNavigation.TenPhim,
                    NgayChieu = danhSachVe.First(v => v.MaLichChieu == currentMaLichChieu).MaLichChieuNavigation.NgayChieu,
                    GioChieu = danhSachVe.First(v => v.MaLichChieu == currentMaLichChieu).MaLichChieuNavigation.GioChieu,
                    SoPhong = danhSachVe.First(v => v.MaLichChieu == currentMaLichChieu).MaLichChieuNavigation.MaPhongNavigation.TenPhong,
                    SoGhe = soghe.TrimEnd(',', ' '),
                    GiaVe = giave
                });
            }

            if (result == null)
            {
                return Json(new { success = false, message = "Không tìm thấy vé!" });
            }
            Console.WriteLine(JsonConvert.SerializeObject(result));

            return Json(new { success = true, data = result });
        }


        public async Task<IActionResult> DatVe(int selectedCombo,string HinhThucTT, decimal giave, decimal tongtien)
        {
            Console.WriteLine(selectedCombo.ToString(), HinhThucTT, giave, tongtien);
            int MaNguoiDung = HttpContext.Session.GetInt32("NguoiDung") ?? 0;
            int? MaCombo = null;
			if (selectedCombo != 0) {
           
                MaCombo = selectedCombo;
            }
            var tencombo = "";
            var combo = _context.Comboes.Where(c => c.MaCombo == selectedCombo).FirstOrDefault();
            if (combo != null)
            {
                tencombo = combo.TenCombo;
                tencombo = tencombo + " x" + (tongtien - giave) / combo.Gia;
                tencombo = "<br/>Combo: " + tencombo;
            }
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
            var SoGhe_ThongBao = "";
            var TT = "";
            if (HinhThucTT == "CK")
            {
                 TT = "Chuyển khoản qua ngân hàng";
            }
            else
            {
                 TT = "Thanh Toán Bằng Tiền Mặt";
            }
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
                    PhuongThuc = TT,
                    NgayThanhToan = DateTime.Now,
                    MaComBo= MaCombo,
                    TrangThai = "Đã Thanh Toán"
                });
            }
            var ThongBao = new ThongBao
            {
                MaNguoiDung = MaNguoiDung,
                NoiDung = "Bạn đã đặt vé thành công<br/>Tên Phim: " + order[0].TenPhim + "<br/>Ngày chiếu:" + order[0].Ngaychieu + " Giờ chiếu: " + order[0].GioChieu + "<br/> Số ghế: " + SoGhe_ThongBao + tencombo

            };
            _context.ThongBaos.Add(ThongBao);
            _context.ThanhToans.AddRange(payments);
            await _context.SaveChangesAsync();
            await UpdateVe(MaNguoiDung, MaLichChieu);
            return RedirectToAction("Index", "NhanVien");
        }
        public async Task<IActionResult> UpdateVe(int MaNguoiDung, int MaLichChieu)
        {
            // Tìm vé cần cập nhật
            var veCanUpdate = _context.Ves
                .Where(v => v.TrangThai == false && v.MaKhachHang == MaNguoiDung && v.MaLichChieu == MaLichChieu)
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
    }
    
}
