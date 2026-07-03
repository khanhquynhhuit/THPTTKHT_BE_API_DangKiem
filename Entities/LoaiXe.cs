using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_QuanLyDangKiem.Entities;

[Table("LoaiXe")]
public partial class LoaiXe
{
    [Key]
    public int MaLoaiXe { get; set; }

    [StringLength(100)]
    public string? TenLoaiXe { get; set; }

    public int ChuKyDangKiemThang { get; set; }

    [InverseProperty("MaLoaiXeNavigation")]
    public virtual ICollection<BangGiaDangKiem> BangGiaDangKiems { get; set; } = new List<BangGiaDangKiem>();

    [InverseProperty("MaLoaiXeNavigation")]
    public virtual ICollection<PhuongTien> PhuongTiens { get; set; } = new List<PhuongTien>();
}
