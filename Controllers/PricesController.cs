using API_QuanLyDangKiem.DTOs;
using API_QuanLyDangKiem.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_QuanLyDangKiem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PricesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PricesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var prices = await _context.BangGiaDangKiems
                .Include(x => x.MaLoaiXeNavigation)
                .Select(x => new PriceDto
                {
                    MaGia = x.MaGia,
                    MaLoaiXe = x.MaLoaiXe ?? 0,
                    GiaDangKiem = x.GiaDangKiem ?? 0,
                    TenLoaiXe = x.MaLoaiXeNavigation != null ? x.MaLoaiXeNavigation.TenLoaiXe : ""
                })
                .ToListAsync();

            return Ok(prices);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost]
        public async Task<IActionResult> Create(PriceCreateDto dto)
        {
            var exists = await _context.BangGiaDangKiems.AnyAsync(x => x.MaLoaiXe == dto.MaLoaiXe);
            if (exists) return BadRequest("Loại xe này đã có bảng giá.");

            var price = new BangGiaDangKiem
            {
                MaLoaiXe = dto.MaLoaiXe,
                GiaDangKiem = dto.GiaDangKiem
            };

            _context.BangGiaDangKiems.Add(price);
            await _context.SaveChangesAsync();
            return Ok(price);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, PriceCreateDto dto)
        {
            var price = await _context.BangGiaDangKiems.FirstOrDefaultAsync(x => x.MaGia == id);
            if (price == null) return NotFound("Không tìm thấy bảng giá");

            price.MaLoaiXe = dto.MaLoaiXe;
            price.GiaDangKiem = dto.GiaDangKiem;

            await _context.SaveChangesAsync();
            return Ok(price);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var price = await _context.BangGiaDangKiems.FirstOrDefaultAsync(x => x.MaGia == id);
            if (price == null) return NotFound("Không tìm thấy bảng giá");

            _context.BangGiaDangKiems.Remove(price);
            await _context.SaveChangesAsync();
            return Ok("Xóa thành công");
        }
    }
}
