using BloodDonation.EF;
using BloodDonation.EF.Tables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodDonation.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class DonationController : Controller
    {
        BloodBankDbContext dc;
        public DonationController(BloodBankDbContext dc)
        {
            this.dc = dc;
        }

        public IActionResult Read()
        {
            var donations = dc.Donations.ToList();
            return View(donations);
        }
        [HttpGet]
        public IActionResult Create() //=======================================
        {
            ViewBag.Donors = dc.Donors.ToList();

            var donation = new Donation
            {
                DonationDate = DateOnly.FromDateTime(DateTime.Today)
            };

            return View(donation);
        }
        [HttpPost]
        public IActionResult Create(Donation donation)
        {
            var donorExists = dc.Donors.Any(d => d.DonorId == donation.DonorId);

            if (!donorExists)
            {
                ModelState.AddModelError("DonorId", "Donor does not exist.");
            }

            if (donation.DonationDate > DateOnly.FromDateTime(DateTime.Today))
            {
                ModelState.AddModelError("DonationDate",
                    "Donation date cannot be in the future.");
            }
            
            if (ModelState.IsValid)
            {
                dc.Donations.Add(donation);
                dc.SaveChanges();
                return RedirectToAction("Read");
            }
            ViewBag.Donors = dc.Donors.ToList();
            return View(donation);
        }
        [HttpGet]
        public IActionResult Edit(int id) //========================================
        {
            ViewBag.Donors = dc.Donors.ToList();
            var info = dc.Donations.Find(id);
            return View(info);
        }
        [HttpPost]
        public IActionResult Edit(int id, Donation updatedDonation)
        {
            var existingDonation = dc.Donations.Find(id);
            if (existingDonation == null)
            {
                return NotFound();
            }

            existingDonation.DonorId = updatedDonation.DonorId;
            existingDonation.DonationDate = updatedDonation.DonationDate;
            existingDonation.VolumeMl = updatedDonation.VolumeMl;
            existingDonation.CampName = updatedDonation.CampName;

            dc.SaveChanges();
            ViewBag.Donors = dc.Donors.ToList();
            return RedirectToAction("Read");
        }
        [HttpGet]
        public IActionResult Delete(int id) //========================================
        {
            ViewBag.Donors = dc.Donors.ToList();
            var info = dc.Donations.Find(id);
            if (info == null)
            {
                return NotFound();
            }

            return View(info);
        }
        [HttpPost]
        public IActionResult Delete(int id, Donation donation)
        {
            ViewBag.Donors = dc.Donors.ToList();
            var info = dc.Donations.Find(id);
            if (info == null)
            {
                return NotFound();
            }

            dc.Donations.Remove(info);
            dc.SaveChanges();
            return RedirectToAction("Read");
        }
        //==================================
        [HttpGet]
        public IActionResult TotalBloodCollected()
        {
            int totalVolume = dc.Donations.Sum(d => d.VolumeMl);

            return View(totalVolume);
        }
    }
}
