using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceAPI.Data;
using EcommerceAPI.Models;

namespace EcommerceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly EcommerceDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CartController(EcommerceDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private string GetUserId() => _userManager.GetUserId(User) ?? string.Empty;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CartItem>>> GetCartItems()
        {
            var userId = GetUserId();
            return await _context.CartItems.Where(c => c.UserId == userId).ToListAsync();
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] CartItem item)
        {
            var userId = GetUserId();
            var existing = await _context.CartItems.FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == item.ProductId);
            if (existing != null)
            {
                existing.Quantity += item.Quantity;
                _context.CartItems.Update(existing);
            }
            else
            {
                item.UserId = userId;
                item.DateAdded = DateTime.UtcNow;
                _context.CartItems.Add(item);
            }
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> UpdateCartItem([FromBody] CartItem item)
        {
            var userId = GetUserId();
            var existing = await _context.CartItems.FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == item.ProductId);
            if (existing == null) return NotFound();
            existing.Quantity = item.Quantity;
            _context.CartItems.Update(existing);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{productId}")]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            var userId = GetUserId();
            var item = await _context.CartItems.FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);
            if (item == null) return NotFound();
            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
} 