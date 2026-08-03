using BloodDonation.EF;
using BloodDonation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BloodDonation.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly BloodBankDbContext dc;

        public UserController(BloodBankDbContext db)
        {
            dc = db;
        }

        public IActionResult Index()
        {
            var users = dc.Users
                          .Include(u => u.Role)
                          .ToList();

            return View(users);
        } 

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Roles = new SelectList(dc.Roles, "RoleId", "RoleName");
            return View(new CreateUserVM());
        }

        [HttpPost]
        public IActionResult Create(CreateUserVM user)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new SelectList(dc.Roles, "RoleId", "RoleName");
                return View(user);
            }

            // Check duplicate username
            if (dc.Users.Any(u => u.Username == user.Username))
            {
                ModelState.AddModelError("Username", "Username already exists.");
                ViewBag.Roles = new SelectList(dc.Roles, "RoleId", "RoleName");
                return View(user);
            }

            // Check duplicate email
            if (dc.Users.Any(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "Email already exists.");
                ViewBag.Roles = new SelectList(dc.Roles, "RoleId", "RoleName");
                return View(user);
            }

            var newUser = new EF.Tables.User
            {
                FullName = user.FullName,
                Username = user.Username,
                Email = user.Email,

                // Temporary (we'll hash later)
                PasswordHash = user.Password,

                RoleId = user.RoleId,

                IsActive = true
            };

            dc.Users.Add(newUser);
            dc.SaveChanges();

            TempData["Success"] = "User created successfully.";

            return RedirectToAction("Index");
        }
    }
}