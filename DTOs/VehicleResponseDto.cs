namespace API_QuanLyDangKiem.DTOs
{
    public class VehicleResponseDto
    {
        public int MaPhuongTien { get; set; }

        public string BienSo { get; set; }

        public int? MaLoaiXe { get; set; }

        public int NamSanXuat { get; set; }

        public string TrangThai { get; set; }

        public DateOnly? NgayDangKiemGanNhat { get; set; }

        public DateOnly? NgayDangKiemTiepTheo { get; set; }
    }
}
