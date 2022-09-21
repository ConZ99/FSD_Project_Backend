using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ConfigurationManager = System.Configuration.ConfigurationManager;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FinalProject.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly DataContext _context;
        public AccountController(DataContext context)
        {
            _context = context;
        }

        // GET: api/<AccountController>
        [HttpGet("getAllUsers")]
        public async Task<ActionResult<List<Account>>> GetAllAccounts()
        {
            return Ok(await _context.Accounts.ToListAsync());
        }

        // GET api/<AccountController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Account>> GetAccountById(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
                return BadRequest("Account not found.");
            
            return Ok(account);
        }

        // POST api/<AccountController>
        [HttpPost("editAccount")]
        public async Task<ActionResult<List<Account>>> AddAccount(Account account)
        {
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return Ok(await _context.Accounts.ToListAsync());
        }

        // DELETE api/<AccountController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<List<Account>>> DeleteAccountById(int id)
        {
            //if (!IsAdmin())
            //    return BadRequest("Not Authorized!");
            var dbAccount = await _context.Accounts.FindAsync(id);
            if (dbAccount == null)
                return BadRequest("Account not found.");

            _context.Accounts.Remove(dbAccount);
            await _context.SaveChangesAsync();

            return Ok(await _context.Accounts.ToListAsync());
        }
    }
}
