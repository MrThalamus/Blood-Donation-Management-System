using BloodDonation.EF;
using BloodDonation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace BloodDonation.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly BloodBankDbContext dc;

        public HomeController(BloodBankDbContext dc)
        {
            this.dc = dc;
        }

        public IActionResult Index()
        {
            DashboardVM dashboard = new DashboardVM();

            dashboard.WelcomeName = User.FindFirst("FullName")?.Value ?? User.Identity?.Name ?? "";

            dashboard.TotalDonors = dc.Donors.Count();

            dashboard.TotalDonations = dc.Donations.Count();

            dashboard.TotalBloodCollected = dc.Donations.Sum(d => d.VolumeMl);

            dashboard.TotalBloodGroups = dc.Donors
                                           .Select(d => d.BloodGroup)
                                           .Distinct()
                                           .Count();

            dashboard.BloodGroupBreakdown = dc.Donors
                                              .GroupBy(d => d.BloodGroup)
                                              .Select(g => new BloodGroupStatVM
                                              {
                                                  BloodGroup = g.Key.Trim(),
                                                  DonorCount = g.Count()
                                              })
                                              .OrderBy(g => g.BloodGroup)
                                              .ToList();

            dashboard.RecentDonations = dc.Donations
                                          .Include(d => d.Donor)
                                          .OrderByDescending(d => d.DonationDate)
                                          .Take(5)
                                          .ToList();

            var thirtyDaysAgo = DateOnly.FromDateTime(DateTime.Today.AddDays(-30));

            dashboard.DonationsLast30Days = dc.Donations.Count(d => d.DonationDate >= thirtyDaysAgo);

            string[] allBloodGroups = { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };

            var groupsWithDonors = dc.Donors.Select(d => d.BloodGroup.Trim()).Distinct().ToList();

            dashboard.BloodGroupsWithNoDonors = allBloodGroups
                                                 .Except(groupsWithDonors)
                                                 .ToList();

            return View(dashboard);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
