using System;
using System.Collections.Generic;

namespace WebBanVeXemPhim.Models;

public partial class Ghe
{
    public int MaGhe { get; set; }

    public int MaPhong { get; set; }

    public string SoGhe { get; set; } = null!;

    public string LoaiGhe { get; set; } = null!;

    public bool? TrangThai { get; set; }

    public virtual Phong MaPhongNavigation { get; set; } = null!;

    public virtual ICollection<Ve> Ves { get; set; } = new List<Ve>();
}
