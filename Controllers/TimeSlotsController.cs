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
    public class TimeSlotsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TimeSlotsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("center/{centerId}")]
        public async Task<IActionResult> GetByCenter(int centerId)
        {
            var slots = await _context.KhungGios
                .Where(x => x.MaTrungTam == centerId)
                .Select(x => new TimeSlotDto
                {
                    MaKhungGio = x.MaKhungGio,
                    MaTrungTam = x.MaTrungTam ?? 0,
                    ThoiGian = x.ThoiGian ?? DateTime.Now,
                    SoLuongToiDa = x.SoLuongToiDa ?? 0,
                    SoLuongDaDat = x.SoLuongDaDat ?? 0,
                })
                .ToListAsync();

            return Ok(slots);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            //var slots = await _context.KhungGios
            //    .Select(x => new TimeSlotDto
            //    {
            //        MaKhungGio = x.MaKhungGio,
            //        MaTrungTam = x.MaTrungTam ?? 0,
            //        ThoiGian = x.ThoiGian ?? DateTime.Now,
            //        SoLuongToiDa = x.SoLuongToiDa ?? 0,
            //        SoLuongDaDat = x.SoLuongDaDat ?? 0,
            //    })
            //    .ToListAsync();
            var slots = await _context.KhungGios
            .OrderBy(x => x.MaTrungTam.Value)
            .ThenBy(x => x.ThoiGian.Value.Date)
            .ThenBy(x => x.ThoiGian.Value.TimeOfDay)
            .Select(x => new TimeSlotDto
            {
                MaKhungGio = x.MaKhungGio,
                MaTrungTam = x.MaTrungTam ?? 0,
                ThoiGian = x.ThoiGian ?? DateTime.Now,
                SoLuongToiDa = x.SoLuongToiDa ?? 0,
                SoLuongDaDat = x.SoLuongDaDat ?? 0,
            })
            .ToListAsync();

            return Ok(slots);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost]
        public async Task<IActionResult> Create(TimeSlotCreateDto dto)
        {
            var slot = new KhungGio
            {
                MaTrungTam = dto.MaTrungTam,
                ThoiGian = dto.ThoiGian,
                SoLuongToiDa = dto.SoLuongToiDa,
                SoLuongDaDat = 0
            };

            _context.KhungGios.Add(slot);
            await _context.SaveChangesAsync();

            return Ok(slot);
        }
    }
}
