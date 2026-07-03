using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_QuanLyDangKiem.Entities;

[Table("TrungTamDangKiem")]
public partial class TrungTamDangKiem
{
    [Key]
    public int MaTrungTam { get; set; }

    [StringLength(255)]
    public string? TenTrungTam { get; set; }

    public string? DiaChi { get; set; }

    [StringLength(100)]
    public string? GioLamViec { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayTao { get; set; }

    [InverseProperty("MaTrungTamNavigation")]
    public virtual ICollection<KhungGio> KhungGios { get; set; } = new List<KhungGio>();

    [InverseProperty("MaTrungTamNavigation")]
    public virtual ICollection<LichHenDangKiem> LichHenDangKiems { get; set; } = new List<LichHenDangKiem>();
}
