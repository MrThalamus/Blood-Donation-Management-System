using BloodDonation.EF;
using BloodDonation.EF.Tables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using BloodDonation.Models;

namespace BloodDonation.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class DonorController : Controller
    {
        BloodBankDbContext dc;
        public DonorController(BloodBankDbContext dc)
        {
            this.dc = dc;
        }
        public IActionResult Read()
        {
            var donors = dc.Donors.ToList();
            return View(donors);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View(new Donor());
        }
        [HttpPost]
        public IActionResult Create(Donor donor) //================================================
        {
            string[] validBloodGroups ={"A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-"};

            if (!validBloodGroups.Contains(donor.BloodGroup))
            {
                ModelState.AddModelError("BloodGroup", "Invalid blood group.");
            }

            if (donor.LastDonationDate > DateOnly.FromDateTime(DateTime.Today))
            {
                ModelState.AddModelError("LastDonationDate",
                    "Last donation date cannot be in the future.");
            }

            if (ModelState.IsValid)
            {
                dc.Donors.Add(donor);
                dc.SaveChanges();
                return RedirectToAction("Read");
            }

            return View(donor);
        }
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var info = dc.Donors.Find(id);
            return View(info);
        }
        [HttpPost]
        public IActionResult Edit(Donor donor) //=================================================
        {
            var existingDonor = dc.Donors.Find(donor.DonorId);

            string[] validBloodGroups = { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };

            if (!validBloodGroups.Contains(donor.BloodGroup))
            {
                ModelState.AddModelError("BloodGroup", "Invalid blood group.");
            }

            if (donor.LastDonationDate > DateOnly.FromDateTime(DateTime.Today))
            {
                ModelState.AddModelError("LastDonationDate",
                    "Last donation date cannot be in the future.");
            }

            if (ModelState.IsValid)
            {
                if(existingDonor == null)
                {
                    return NotFound();
                }
                existingDonor.FullName = donor.FullName;
                existingDonor.BloodGroup = donor.BloodGroup;
                existingDonor.ContactNo = donor.ContactNo;
                existingDonor.City = donor.City;
                existingDonor.LastDonationDate = donor.LastDonationDate;
                
                dc.SaveChanges();
                return RedirectToAction("Read");
            }
            return View(donor);
        }
        [HttpGet]
        public IActionResult Delete(int id) //=======================================
        {
            var existingDonor = dc.Donors.Find(id);
            if (existingDonor == null)
            {
                return NotFound();
            }
            else
            {
                return View(existingDonor);
            }
        }
        [HttpPost]
        public IActionResult Delete(Donor donor)
        {
            var existingDonor = dc.Donors.Find(donor.DonorId);
            if (existingDonor == null)
            {
                return NotFound();
            }
            else
            {
                dc.Donors.Remove(existingDonor);
                dc.SaveChanges();
                return RedirectToAction("Read");
            }
        }
        //==================================================================
        [HttpGet]
        public IActionResult Filter(string bloodGroup)
        {
            
            List<Donor> donors;

            if (string.IsNullOrEmpty(bloodGroup))
            {
                donors = dc.Donors.ToList();
            }
            else
            {
                donors = dc.Donors
                           .Where(d => d.BloodGroup == bloodGroup)
                           .ToList();
            }

            return View(donors);
        }
        [HttpPost]
        public IActionResult Filter(string bloodGroup, string city)
        {
            var donors = dc.Donors.ToList();

            if (!string.IsNullOrEmpty(bloodGroup))
            {
                donors = dc.Donors
                           .Where(d => d.BloodGroup == bloodGroup)
                           .ToList();
            }

            return View(donors);
        }

        [HttpGet]
        public IActionResult RecentDonors()
        {
            var donors = dc.Donors
                           .OrderByDescending(d => d.LastDonationDate)
                           .ToList();

            return View(donors);
        }

        public IActionResult DonationCount()
        {
            var result = dc.Donors
               .Select(d => new DonorDonationCountVM
               {
                   DonorId = d.DonorId,
                   FullName = d.FullName,
                   TotalDonations = d.Donations.Count()
               })
               .ToList();

            return View(result);
        }
    }
}
