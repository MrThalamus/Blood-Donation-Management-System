using BloodDonation.EF;
using BloodDonation.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BloodDonation.Controllers
{
    public class AccountController : Controller 
    {
        private readonly BloodBankDbContext dc;

        public AccountController(BloodBankDbContext dc)
        {
            this.dc = dc;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginVM());
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginVM login)
        {
            if (!ModelState.IsValid)
            {
                return View(login);
            }

            var user = await dc.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u =>
                    u.Username == login.Username &&
                    u.PasswordHash == login.Password &&
                    u.IsActive);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View(login);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.RoleName),
                new Claim("FullName", user.FullName)
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        } 

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
