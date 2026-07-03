using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_QuanLyDangKiem.Entities;

[Table("BangGiaDangKiem")]
public partial class BangGiaDangKiem
{
    [Key]
    public int MaGia { get; set; }

    public int? MaLoaiXe { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? GiaDangKiem { get; set; }

    [ForeignKey("MaLoaiXe")]
    [InverseProperty("BangGiaDangKiems")]
    public virtual LoaiXe? MaLoaiXeNavigation { get; set; }
}
