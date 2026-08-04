using BloodDonation.EF;
using BloodDonation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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

               
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.Password),

                RoleId = user.RoleId,

                IsActive = true
            };

            dc.Users.Add(newUser);
            dc.SaveChanges();

            TempData["Success"] = "User created successfully.";

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var user = dc.Users.Find(id);

            if (user == null)
            {
                return NotFound();
            }

            var vm = new EditUserVM
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Username = user.Username,
                Email = user.Email,
                RoleId = user.RoleId
            };

            ViewBag.Roles = new SelectList(dc.Roles, "RoleId", "RoleName", user.RoleId);

            return View(vm);
        }

        [HttpPost]
        public IActionResult Edit(EditUserVM vm)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new SelectList(dc.Roles, "RoleId", "RoleName", vm.RoleId);
                return View(vm);
            }

            var user = dc.Users.Include(u => u.Role).FirstOrDefault(u => u.UserId == vm.UserId);

            if (user == null)
            {
                return NotFound();
            }

            // Check duplicate username (excluding current user)
            if (dc.Users.Any(u => u.Username == vm.Username && u.UserId != vm.UserId))
            {
                ModelState.AddModelError("Username", "Username already exists.");
                ViewBag.Roles = new SelectList(dc.Roles, "RoleId", "RoleName", vm.RoleId);
                return View(vm);
            }

            // Check duplicate email (excluding current user)
            if (dc.Users.Any(u => u.Email == vm.Email && u.UserId != vm.UserId))
            {
                ModelState.AddModelError("Email", "Email already exists.");
                ViewBag.Roles = new SelectList(dc.Roles, "RoleId", "RoleName", vm.RoleId);
                return View(vm);
            }

            // Prevent demoting the last remaining Admin
            if (user.Role.RoleName == "Admin" && vm.RoleId != user.RoleId)
            {
                int adminCount = dc.Users.Count(u => u.Role.RoleName == "Admin");

                if (adminCount <= 1)
                {
                    ModelState.AddModelError("RoleId", "Cannot change the role of the last remaining Admin.");
                    ViewBag.Roles = new SelectList(dc.Roles, "RoleId", "RoleName", vm.RoleId);
                    return View(vm);
                }
            }

            // Update user information
            user.FullName = vm.FullName;
            user.Username = vm.Username;
            user.Email = vm.Email;
            user.RoleId = vm.RoleId;

            // Update password only if entered
            if (!string.IsNullOrWhiteSpace(vm.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(vm.Password);
            }

            dc.SaveChanges();

            TempData["Success"] = "User updated successfully.";

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var user = dc.Users
                         .Include(u => u.Role)
                         .FirstOrDefault(u => u.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
         {
            try
            {
                var user = dc.Users
                             .Include(u => u.Role)
                             .FirstOrDefault(u => u.UserId == id);

                if (user == null)
                    return NotFound();

                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                    return Unauthorized();

                int loggedInUserId = int.Parse(userIdClaim);

                if (loggedInUserId == user.UserId)
                {
                    TempData["Error"] = "You cannot delete your own account.";
                    return RedirectToAction("Index");
                }

                if (user.Role.RoleName == "Admin")
                {
                    int adminCount = dc.Users.Count(u => u.Role.RoleName == "Admin");

                    if (adminCount <= 1)
                    {
                        TempData["Error"] = "Cannot delete the last Admin account.";
                        return RedirectToAction("Index");
                    }
                }

                dc.Users.Remove(user);
                dc.SaveChanges();

                TempData["Success"] = "User deleted successfully.";

                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["Error"] = "Unable to delete this user. It may be referenced by other records.";
                return RedirectToAction("Index");
            }
        }
    }
}