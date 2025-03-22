using System;
using System.Collections.Generic;

namespace WebBanVeXemPhim.Models;

public partial class ThanhToan
{
    public int MaThanhToan { get; set; }

    public int MaVe { get; set; }

    public string PhuongThuc { get; set; } = null!;

    public string TrangThai { get; set; } = null!;

    public DateTime? NgayThanhToan { get; set; }

    public int? MaComBo { get; set; }

    public virtual Ve MaVeNavigation { get; set; } = null!;
}
