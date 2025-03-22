using System;
using System.Collections.Generic;

namespace WebBanVeXemPhim.Models;

public partial class Banner
{
    public string? Anh { get; set; }

    public string? MoTa { get; set; }

    public bool? TrangThai { get; set; }

    public int MaBanner { get; set; }
}
