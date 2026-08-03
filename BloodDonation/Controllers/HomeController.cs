using BloodDonation.EF;
using BloodDonation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

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

            dashboard.TotalDonors = dc.Donors.Count();

            dashboard.TotalDonations = dc.Donations.Count();

            dashboard.TotalBloodCollected = dc.Donations.Sum(d => d.VolumeMl);

            dashboard.TotalBloodGroups = dc.Donors
                                           .Select(d => d.BloodGroup)
                                           .Distinct()
                                           .Count();

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
