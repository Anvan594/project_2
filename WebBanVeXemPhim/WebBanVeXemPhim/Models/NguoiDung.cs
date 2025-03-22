using System;
using System.Collections.Generic;

namespace WebBanVeXemPhim.Models;

public partial class NguoiDung
{
    public int MaNguoiDung { get; set; }

    public string TenNguoiDung { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string MatKhau { get; set; } = null!;

    public bool? TrangThai { get; set; }

    public string? Token { get; set; }

    public virtual ICollection<ThongBao> ThongBaos { get; set; } = new List<ThongBao>();

    public virtual ICollection<Ve> Ves { get; set; } = new List<Ve>();
}
