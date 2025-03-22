using System;
using System.Collections.Generic;

namespace WebBanVeXemPhim.Models;

public partial class Ve
{
    public int MaVe { get; set; }

    public int MaLichChieu { get; set; }

    public int MaGhe { get; set; }

    public int MaKhachHang { get; set; }

    public decimal GiaVe { get; set; }

    public DateTime? NgayDat { get; set; }

    public bool? TrangThai { get; set; }

    public virtual Ghe MaGheNavigation { get; set; } = null!;

    public virtual NguoiDung MaKhachHangNavigation { get; set; } = null!;

    public virtual LichChieu MaLichChieuNavigation { get; set; } = null!;

    public virtual ICollection<ThanhToan> ThanhToans { get; set; } = new List<ThanhToan>();
}
