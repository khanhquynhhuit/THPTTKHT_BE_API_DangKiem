using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_QuanLyDangKiem.Entities;

[Table("KhungGio")]
[Index("MaTrungTam", "ThoiGian", Name = "UQ_KhungGio", IsUnique = true)]
public partial class KhungGio
{
    [Key]
    public int MaKhungGio { get; set; }

    public int? MaTrungTam { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ThoiGian { get; set; }

    public int? SoLuongToiDa { get; set; }

    public int? SoLuongDaDat { get; set; }

    [InverseProperty("MaKhungGioNavigation")]
    public virtual ICollection<LichHenDangKiem> LichHenDangKiems { get; set; } = new List<LichHenDangKiem>();

    [ForeignKey("MaTrungTam")]
    [InverseProperty("KhungGios")]
    public virtual TrungTamDangKiem? MaTrungTamNavigation { get; set; }
}
