using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinalProject.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CartController : ControllerBase
    {
        private readonly DataContext _context;
        public CartController(DataContext context)
        {
            _context = context;
        }

        [HttpGet("getCartByUserId/{id}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<ActionResult<List<Cart>>> GetCartByUserId(int id)
        {
            var cart = await _context.Carts.Where(c => c.UserId == id).ToArrayAsync();
            return Ok(cart.Length == 0 ? 404 : cart);
        }

        [HttpPut("addToCart/userId-{userId}/productId-{productId}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<ActionResult<Cart>> AddToCart(int userId, int productId)
        {
            var checkProd = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);
            var checkUser = await _context.Accounts.FirstOrDefaultAsync(u => u.Id == userId);

            if (checkProd == null || checkUser == null)
            {
                return BadRequest("404");
            }

            if (checkProd.Quantity <= 0)
            {
                return BadRequest("400");
            }

            var cart = await _context.Carts.Where(u => u.UserId == userId).FirstOrDefaultAsync(p => p.ProductId == productId);

            if (cart == null)
            {
                var newCart = new Cart
                {
                    UserId = userId,
                    ProductId = productId,
                    Quantity = 1,
                    TotalPrice = checkProd.Price
                };

                checkProd.Quantity--;

                _context.Carts.Add(newCart);
                _context.Entry(checkProd).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(newCart);
            }

            cart.Quantity++;
            cart.TotalPrice += checkProd.Price;

            checkProd.Quantity--;

            _context.Entry(cart).State = EntityState.Modified;
            _context.Entry(checkProd).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(cart);
        }

        [HttpPut("removeFromCart/userId-{userId}/productId-{productId}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<ActionResult<Cart>> RemoveFromCart(int userId, int productId)
        {
            var checkProd = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);
            var checkUser = await _context.Accounts.FirstOrDefaultAsync(u => u.Id == userId);

            if (checkProd == null || checkUser == null)
            {
                return BadRequest("Not found.");
            }

            var cart = await _context.Carts.Where(u => u.UserId == userId).FirstOrDefaultAsync(p => p.ProductId == productId);

            if (cart == null)
            {
                return BadRequest("Nothing to remove.");
            }

            cart.Quantity--;
            cart.TotalPrice -= checkProd.Price;

            checkProd.Quantity++;

            if (cart.Quantity <= 0)
            {
                _context.Carts.Remove(cart);
            }
            else
            {
                _context.Entry(cart).State = EntityState.Modified;
            }

            _context.Entry(checkProd).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(cart);
        }
    }
}
