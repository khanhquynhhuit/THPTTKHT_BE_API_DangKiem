using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_QuanLyDangKiem.Entities;

[Table("ThanhToan")]
[Index("MaLichHen", Name = "UQ__ThanhToa__150F264E657F3365", IsUnique = true)]
public partial class ThanhToan
{
    [Key]
    public int MaThanhToan { get; set; }

    public int? MaLichHen { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? SoTien { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string? PhuongThucThanhToan { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? TrangThai { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? MaGiaoDich { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string? DonViTienTe { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayThanhToan { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayTao { get; set; }

    [InverseProperty("MaThanhToanNavigation")]
    public virtual ICollection<LichSuThanhToan> LichSuThanhToans { get; set; } = new List<LichSuThanhToan>();

    [ForeignKey("MaLichHen")]
    [InverseProperty("ThanhToan")]
    public virtual LichHenDangKiem? MaLichHenNavigation { get; set; }
}
