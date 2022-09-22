using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinalProject.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IUserService _userService;
        public AccountController(DataContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        [HttpGet("getAllUsers")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<Account>>> GetAllAccounts()
        {
            return Ok(await _context.Accounts.ToListAsync());
        }

        [HttpGet("getUser-{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Account>> GetAccountById(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                return BadRequest("Account not found.");
            }

            return Ok(account);
        }

        [HttpPost("editAccount-{id}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<ActionResult<Account>> EditAccountById(Account newAccount, int id)
        {
            var oldAccount = await _context.Accounts.FindAsync(id);
            if (oldAccount == null)
                return BadRequest("Account not found.");

            oldAccount.FirstName = newAccount.FirstName;
            oldAccount.LastName = newAccount.LastName;
            oldAccount.Email = newAccount.Email;
            oldAccount.DateOfBirth = newAccount.DateOfBirth;
            oldAccount.Phone = newAccount.Phone;
            oldAccount.Address = newAccount.Address;
            oldAccount.Funds = newAccount.Funds;

            _context.Entry(oldAccount).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(oldAccount);
        }

        [HttpPost("editOwnAccount")]
        [Authorize(Roles = "Admin,User")]
        public async Task<ActionResult<Account>> EditOwnAccount(Account newAccount)
        {
            var userId = _userService.GetMyId();
            if (userId == null)
                return BadRequest("Account not found.");

            var oldAccount = await _context.Accounts.FindAsync(int.Parse(userId));
            if (oldAccount == null)
                return BadRequest("Account not found.");

            oldAccount.FirstName = newAccount.FirstName;
            oldAccount.LastName = newAccount.LastName;
            oldAccount.DateOfBirth = newAccount.DateOfBirth;
            oldAccount.Phone = newAccount.Phone;
            oldAccount.Address = newAccount.Address;

            _context.Entry(oldAccount).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(oldAccount);
        }

        [HttpDelete("deleteAccount-{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<Account>>> DeleteAccountById(int id)
        {
            var dbAccount = await _context.Accounts.FindAsync(id);
            if (dbAccount == null)
            {
                return BadRequest("Account not found.");
            }

            _context.Accounts.Remove(dbAccount);
            await _context.SaveChangesAsync();

            return Ok(await _context.Accounts.ToListAsync());
        }
    }
}
