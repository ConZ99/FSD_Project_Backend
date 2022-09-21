using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinalProject.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly DataContext _context;
        public OrderController(DataContext context)
        {
            _context = context;
        }

        [HttpGet("getOrderByUserId/userId-{id}")]
        //[Authorize(Roles = "Admin,User")]
        public async Task<ActionResult<Order>> GetOrderByUserId(int id)
        {
            var order = await _context.Orders.Where(o => o.UserId == id).ToListAsync();

            if (order == null)
                return BadRequest("Order not found.");

            if (order.Count() <= 0)
                return BadRequest("Nothing inside.");

            return Ok(order);
        }

        [HttpPut("buyCartContent/userId-{id}")]
        //[Authorize(Roles="Admin,User")]
        public async Task<ActionResult<Order>> BuyCartContent(int id)
        {
            var carts = await _context.Carts.Where(c => c.UserId == id).ToListAsync();

            var account = await _context.Accounts.FindAsync(id);
            double price = 0;
            foreach (var cart in carts)
            {
                price += cart.TotalPrice;
            }
            if (account != null)
            {
                if (account.Funds < price)
                    return BadRequest("404");
            }

            string productNames = "";
            var order = new Order();
            foreach (var cart in carts)
            {
                _context.Carts.Remove(cart);
                var product = await _context.Products.FindAsync(cart.ProductId);
                if(product != null)
                    productNames += product.Name;
                order.TotalPrice += cart.TotalPrice;
            }

            order.ProductNames = productNames;
            order.UserId = id;
            order.Status = "Test";
            order.DateTime = DateTime.UtcNow;

            account = await _context.Accounts.FindAsync(id);
            if (account != null)
            {
                account.Funds -= order.TotalPrice;
                _context.Entry(account).State = EntityState.Modified;
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return Ok(order);
        }
    }
}
