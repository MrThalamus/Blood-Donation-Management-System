using BloodDonation.EF;
using BloodDonation.EF.Tables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BloodDonation.Controllers
{
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
    }
}
