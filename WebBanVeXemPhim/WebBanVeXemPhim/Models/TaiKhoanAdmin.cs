using System;
using System.Collections.Generic;

namespace WebBanVeXemPhim.Models;

public partial class TaiKhoanAdmin
{
    public int MaAdmin { get; set; }

    public string TenDangNhap { get; set; } = null!;

    public string MatKhau { get; set; } = null!;

    public string HoTen { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? SoDienThoai { get; set; }

    public DateTime? NgayTao { get; set; }

    public bool? TrangThai { get; set; }

    public string? ChucVu { get; set; }
}
