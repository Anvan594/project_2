using System;
using System.Collections.Generic;

namespace WebBanVeXemPhim.Models;

public partial class Phong
{
    public int MaPhong { get; set; }

    public string TenPhong { get; set; } = null!;

    public int SoLuongGhe { get; set; }

    public bool? TrangThai { get; set; }

    public virtual ICollection<Ghe> Ghes { get; set; } = new List<Ghe>();

    public virtual ICollection<LichChieu> LichChieus { get; set; } = new List<LichChieu>();
}
