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
    public class OrdersController : ControllerBase
    {
        private readonly EcommerceDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public OrdersController(EcommerceDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private string GetUserId() => _userManager.GetUserId(User) ?? string.Empty;

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout()
        {
            var userId = GetUserId();
            var cartItems = await _context.CartItems.Where(c => c.UserId == userId).ToListAsync();
            if (!cartItems.Any()) return BadRequest("Cart is empty");
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = 0,
                Items = new List<OrderItem>()
            };
            foreach (var cart in cartItems)
            {
                var product = await _context.Products.FindAsync(cart.ProductId);
                if (product == null) continue;
                order.Items.Add(new OrderItem
                {
                    ProductId = cart.ProductId,
                    Quantity = cart.Quantity,
                    UnitPrice = product.Price
                });
                order.TotalAmount += product.Price * cart.Quantity;
            }
            _context.Orders.Add(order);
            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();
            return Ok(order);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrderHistory()
        {
            var userId = GetUserId();
            return await _context.Orders.Include(o => o.Items).Where(o => o.UserId == userId).ToListAsync();
        }

        [HttpGet("{orderId}")]
        public async Task<ActionResult<Order>> GetOrderDetails(int orderId)
        {
            var userId = GetUserId();
            var order = await _context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
            if (order == null) return NotFound();
            return order;
        }
    }
} 