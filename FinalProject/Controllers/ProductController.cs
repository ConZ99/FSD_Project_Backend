using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinalProject.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly DataContext _context;
        public ProductController(DataContext context)
        {
            _context = context;
        }

        [HttpGet("getAllProducts")]
        [Authorize(Roles = "Admin,User")]
        public async Task<ActionResult<List<Product>>> GetAllProducts()
        {
            return Ok(await _context.Products.ToListAsync());
        }

        [HttpGet("getProductById/{id}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<ActionResult<Product>> GetProductById(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return BadRequest("Product not found.");
            }

            return Ok(product);
        }

        [HttpGet("getProductByUse/{use}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<ActionResult<List<Product>>> GetProductsByUse(string use)
        {
            var product = await _context.Products.Where(p => p.Uses.Contains(use)).ToListAsync();
            if (product == null)
            {
                return BadRequest("Product not found.");
            }

            return Ok(product);
        }

        [HttpPut("updateMedicine/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Product>> PutProduct(int id, Product product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }

            _context.Entry(product).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return Ok(product);
        }

        [HttpPost("addProduct")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<Product>>> AddProduct(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return Ok(await _context.Products.ToListAsync());
        }

        [HttpDelete("deleteProduct/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<Product>>> DeleteAccountById(int id)
        {
            var dbProduct = await _context.Products.FindAsync(id);
            if (dbProduct == null)
            {
                return BadRequest("Product not found.");
            }

            _context.Products.Remove(dbProduct);
            await _context.SaveChangesAsync();

            return Ok(await _context.Products.ToListAsync());
        }
    }
}
