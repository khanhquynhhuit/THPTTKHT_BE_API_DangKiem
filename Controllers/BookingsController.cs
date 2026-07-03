using System.Security.Claims;
using API_QuanLyDangKiem.DTOs;
using API_QuanLyDangKiem.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_QuanLyDangKiem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BookingsController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(BookingCreateDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var vehicle = await _context.PhuongTiens
                .FirstOrDefaultAsync(x =>
                    x.MaPhuongTien == dto.MaPhuongTien &&
                    x.MaNguoiDung == userId);

            if (vehicle == null)
                return BadRequest("Xe không thuộc user");

            var slot = await _context.KhungGios
                .FirstOrDefaultAsync(x => x.MaKhungGio == dto.MaKhungGio);

            if (slot == null)
                return BadRequest("Slot không tồn tại");

            if (slot.SoLuongDaDat >= slot.SoLuongToiDa)
                return BadRequest("Slot đã đầy");

            var existingBooking = await _context.LichHenDangKiems
                .FirstOrDefaultAsync(x => x.MaPhuongTien == dto.MaPhuongTien && x.ThoiGianHen == slot.ThoiGian);

            LichHenDangKiem booking;

            if (existingBooking != null)
            {
                if (existingBooking.TrangThai != "CANCELLED")
                {
                    return BadRequest("Phương tiện này đã được đặt lịch vào thời gian này.");
                }
                
                // Nếu đã hủy trước đó, ta tái sử dụng (Update) để tránh lỗi UNIQUE KEY constraint
                existingBooking.TrangThai = "BOOKED";
                existingBooking.MaTrungTam = dto.MaTrungTam;
                existingBooking.MaKhungGio = dto.MaKhungGio;
                booking = existingBooking;
                
                slot.SoLuongDaDat++;
            }
            else
            {
                slot.SoLuongDaDat++;

                booking = new LichHenDangKiem
                {
                    MaPhuongTien = dto.MaPhuongTien,
                    MaTrungTam = dto.MaTrungTam,
                    MaKhungGio = dto.MaKhungGio,
                    ThoiGianHen = slot.ThoiGian,
                    TrangThai = "BOOKED"
                };

                _context.LichHenDangKiems.Add(booking);
            }

            // Gửi thông báo cho User
            var thongBao = new ThongBao
            {
                MaNguoiDung = userId,
                MaPhuongTien = dto.MaPhuongTien,
                NoiDung = $"Đặt lịch đăng kiểm thành công cho xe {vehicle.BienSo} vào lúc {slot.ThoiGian.Value.ToString("dd/MM/yyyy HH:mm")}",
                LoaiThongBao = "SYSTEM",
                TrangThai = "SENT",
                NgayTao = DateTime.Now
            };
            _context.ThongBaos.Add(thongBao);

            await _context.SaveChangesAsync();

            return Ok(new {
                maLichHen = booking.MaLichHen,
                trangThai = booking.TrangThai,
                message = "Đặt lịch thành công"
            });
        }

        [HttpGet("my")]
        public async Task<IActionResult> MyBookings()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var bookings = await _context.LichHenDangKiems
                .Include(x => x.MaPhuongTienNavigation)
                .Include(x => x.MaTrungTamNavigation)
                .Include(x => x.MaKhungGioNavigation)
                .Where(x => x.MaPhuongTienNavigation.MaNguoiDung == userId)
                .Select(x => new
                {
                    x.MaLichHen,
                    x.ThoiGianHen,
                    x.TrangThai,
                    BienSo = x.MaPhuongTienNavigation.BienSo,
                    TenTrungTam = x.MaTrungTamNavigation.TenTrungTam,
                    MaTrungTam = x.MaTrungTam,
                    MaKhungGio = x.MaKhungGio,
                    MaPhuongTien = x.MaPhuongTien
                })
                .ToListAsync();

            return Ok(bookings);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, BookingCreateDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var booking = await _context.LichHenDangKiems
                .Include(x => x.MaPhuongTienNavigation)
                .FirstOrDefaultAsync(x => x.MaLichHen == id);

            if (booking == null)
                return NotFound("Không tìm thấy lịch hẹn");

            if (booking.MaPhuongTienNavigation.MaNguoiDung != userId)
                return Forbid("Không có quyền sửa lịch hẹn này");

            if (booking.TrangThai != "BOOKED" && booking.TrangThai != "PENDING")
                return BadRequest("Chỉ có thể sửa lịch hẹn đang chờ");

            // Kiểm tra phương tiện mới
            var vehicle = await _context.PhuongTiens
                .FirstOrDefaultAsync(x => x.MaPhuongTien == dto.MaPhuongTien && x.MaNguoiDung == userId);
            if (vehicle == null) return BadRequest("Xe không thuộc user");

            // Kiểm tra khung giờ mới
            var newSlot = await _context.KhungGios.FirstOrDefaultAsync(x => x.MaKhungGio == dto.MaKhungGio);
            if (newSlot == null) return BadRequest("Khung giờ không tồn tại");

            // Nếu đổi khung giờ
            if (booking.MaKhungGio != dto.MaKhungGio)
            {
                if (newSlot.SoLuongDaDat >= newSlot.SoLuongToiDa)
                    return BadRequest("Khung giờ mới đã đầy");

                // Trả lại slot cũ
                var oldSlot = await _context.KhungGios.FirstOrDefaultAsync(x => x.MaKhungGio == booking.MaKhungGio);
                if (oldSlot != null)
                {
                    oldSlot.SoLuongDaDat--;
                }

                // Chiếm slot mới
                newSlot.SoLuongDaDat++;
                booking.MaKhungGio = dto.MaKhungGio;
                booking.ThoiGianHen = newSlot.ThoiGian;
            }

            booking.MaPhuongTien = dto.MaPhuongTien;
            booking.MaTrungTam = dto.MaTrungTam;

            // Kiểm tra xem giờ mới có bị trùng lịch hẹn khác không
            var existing = await _context.LichHenDangKiems
                .FirstOrDefaultAsync(x => x.MaLichHen != id && x.MaPhuongTien == dto.MaPhuongTien && x.ThoiGianHen == newSlot.ThoiGian && x.TrangThai != "CANCELLED");
            if (existing != null)
                return BadRequest("Phương tiện này đã có lịch vào thời gian này.");

            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật lịch hẹn thành công" });
        }

        [HttpPut("cancel/{id}")]
        public async Task<IActionResult> Cancel(int id)
        {
            var booking = await _context.LichHenDangKiems
                .FirstOrDefaultAsync(x => x.MaLichHen == id);

            if (booking == null)
                return NotFound();

            booking.TrangThai = "CANCELLED";

            // lấy slot riêng
            var slot = await _context.KhungGios
                .FirstOrDefaultAsync(x => x.MaKhungGio == booking.MaKhungGio);

            if (slot != null)
            {
                slot.SoLuongDaDat--;
            }

            await _context.SaveChangesAsync();

            return Ok("Cancelled");
        }
        [Authorize(Roles = "STAFF,ADMIN")]
        [HttpGet("staff")]
        public async Task<IActionResult> GetAllForStaff()
        {
            var bookings = await _context.LichHenDangKiems
                .Include(x => x.MaPhuongTienNavigation)
                .ThenInclude(p => p.MaNguoiDungNavigation)
                .Select(x => new
                {
                    maLichHen = x.MaLichHen,
                    bienSo = x.MaPhuongTienNavigation.BienSo,
                    tenKhachHang = x.MaPhuongTienNavigation.MaNguoiDungNavigation.HoTen,
                    ngayHen = x.ThoiGianHen.HasValue ? x.ThoiGianHen.Value.ToString("dd/MM/yyyy") : "",
                    gioHen = x.ThoiGianHen.HasValue ? x.ThoiGianHen.Value.ToString("HH:mm") : "",
                    trangThai = x.TrangThai
                })
                .ToListAsync();

            return Ok(bookings);
        }

        [Authorize(Roles = "STAFF,ADMIN")]
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var todayBookingsCount = await _context.LichHenDangKiems
                .Where(x => x.ThoiGianHen >= today && x.ThoiGianHen < tomorrow)
                .CountAsync();

            var completedCount = await _context.LichHenDangKiems
                .Where(x => x.TrangThai == "COMPLETED")
                .CountAsync();

            var pendingCount = await _context.LichHenDangKiems
                .Where(x => x.TrangThai == "BOOKED" || x.TrangThai == "PENDING")
                .CountAsync();

            return Ok(new
            {
                todayBookings = todayBookingsCount,
                completed = completedCount,
                pending = pendingCount
            });
        }
        [Authorize(Roles = "ADMIN")]
        [HttpGet("admin-dashboard")]
        public async Task<IActionResult> GetAdminDashboardStats()
        {
            var totalUsers = await _context.NguoiDungs.CountAsync();
            var totalVehicles = await _context.PhuongTiens.CountAsync();
            var totalBookings = await _context.LichHenDangKiems.CountAsync();
            var completedBookings = await _context.LichHenDangKiems.Where(x => x.TrangThai == "COMPLETED").CountAsync();

            return Ok(new
            {
                users = totalUsers,
                vehicles = totalVehicles,
                bookings = totalBookings,
                completed = completedBookings
            });
        }

        [Authorize(Roles = "STAFF,ADMIN")]
        [HttpGet("detail/{id}")]
        public async Task<IActionResult> GetBookingDetail(int id)
        {
            var booking = await _context.LichHenDangKiems
                .Include(x => x.MaPhuongTienNavigation)
                    .ThenInclude(p => p.MaNguoiDungNavigation)
                .Include(x => x.MaPhuongTienNavigation)
                    .ThenInclude(p => p.MaLoaiXeNavigation)
                .FirstOrDefaultAsync(x => x.MaLichHen == id);

            if (booking == null) return NotFound("Không tìm thấy lịch hẹn");

            var phuongTien = booking.MaPhuongTienNavigation;
            var chuXe = phuongTien?.MaNguoiDungNavigation;
            var loaiXe = phuongTien?.MaLoaiXeNavigation;

            return Ok(new
            {
                maLichHen = booking.MaLichHen,
                thoiGianHen = booking.ThoiGianHen,
                trangThai = booking.TrangThai,
                phuongTien = new
                {
                    bienSo = phuongTien?.BienSo,
                    namSanXuat = phuongTien?.NamSanXuat,
                    tenLoaiXe = loaiXe?.TenLoaiXe
                },
                chuXe = new
                {
                    hoTen = chuXe?.HoTen,
                    soDienThoai = chuXe?.SoDienThoai,
                    email = chuXe?.Email
                }
            });
        }
    }
}
