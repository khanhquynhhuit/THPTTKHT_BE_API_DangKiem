namespace API_QuanLyDangKiem.DTOs
{
    public class PaymentHistoryDto
    {
        public int MaLichSu { get; set; }
        public int MaThanhToan { get; set; }
        public decimal SoTien { get; set; }
        public string PhuongThucThanhToan { get; set; }
        public string BienSo { get; set; }
        public string TenNguoiDung { get; set; }
        public string TrangThai { get; set; }
        public string GhiChu { get; set; }
        public DateTime NgayTao { get; set; }
    }
}
