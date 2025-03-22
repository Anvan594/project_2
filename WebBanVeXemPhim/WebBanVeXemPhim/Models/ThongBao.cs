using System;
using System.Collections.Generic;

namespace WebBanVeXemPhim.Models;

public partial class ThongBao
{
    public int MaThongBao { get; set; }

    public int MaNguoiDung { get; set; }

    public string NoiDung { get; set; } = null!;

    public DateTime? NgayGui { get; set; }

    public bool? TrangThai { get; set; } = false;

    public virtual NguoiDung MaNguoiDungNavigation { get; set; } = null!;
}
