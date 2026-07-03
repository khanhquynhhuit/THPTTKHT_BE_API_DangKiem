using System.Security.Claims;
using API_QuanLyDangKiem.DTOs;
using API_QuanLyDangKiem.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_QuanLyDangKiem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NotificationsController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyNotifications()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var notifications = await _context.ThongBaos
                .Where(x => x.MaNguoiDung == userId)
                .OrderByDescending(x => x.NgayTao)
                .Select(x => new NotificationDto
                {
                    MaThongBao = x.MaThongBao,
                    MaPhuongTien = x.MaPhuongTien,
                    NoiDung = x.NoiDung,
                    LoaiThongBao = x.LoaiThongBao,
                    TrangThai = x.TrangThai,
                    NgayTao = x.NgayTao
                })
                .ToListAsync();

            return Ok(notifications);
        }

        [Authorize]
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var notif = await _context.ThongBaos.FirstOrDefaultAsync(x => x.MaThongBao == id && x.MaNguoiDung == userId);
            
            if (notif == null) return NotFound();
            
            notif.TrangThai = "READ";
            await _context.SaveChangesAsync();
            return Ok();
        }

        [Authorize]
        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var notifs = await _context.ThongBaos.Where(x => x.MaNguoiDung == userId && (x.TrangThai == "UNREAD" || x.TrangThai == "SENT")).ToListAsync();
            
            foreach (var n in notifs)
            {
                n.TrangThai = "READ";
            }
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
