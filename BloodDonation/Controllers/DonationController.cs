using BloodDonation.EF;
using BloodDonation.EF.Tables;
using BloodDonation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BloodDonation.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class DonationController : Controller
    {
        private const int PageSize = 10;

        BloodBankDbContext dc;
        public DonationController(BloodBankDbContext dc)
        {
            this.dc = dc;
        }

        /// <summary>
        /// Applies the donation search filters. Each filter is optional and only
        /// narrows the query when a value is supplied.
        /// </summary>
        private IQueryable<Donation> ApplyFilters(
            IQueryable<Donation> query,
            int? donorId,
            DateOnly? fromDate,
            DateOnly? toDate,
            string? campName)
        {
            if (donorId.HasValue && donorId.Value > 0)
            {
                query = query.Where(d => d.DonorId == donorId.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(d => d.DonationDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(d => d.DonationDate <= toDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(campName))
            {
                query = query.Where(d => d.CampName.Contains(campName.Trim()));
            }

            return query;
        }

        /// <summary>
        /// Recomputes a donor's LastDonationDate from their donation records.
        /// Donor.LastDonationDate is denormalized, so every write to a donation
        /// has to refresh it or the donor list and the Recent Donors report
        /// start disagreeing with the Donation table.
        /// </summary>
        private void SyncLastDonationDate(int donorId)
        {
            var donor = dc.Donors.Find(donorId);

            if (donor == null)
            {
                return;
            }

            // Cast to DateOnly? because SQL MAX over zero rows returns NULL,
            // which cannot materialize into a non-nullable DateOnly.
            var latest = dc.Donations
                           .Where(d => d.DonorId == donorId)
                           .Max(d => (DateOnly?)d.DonationDate);

            // No donations left: keep whatever date the donor already had, since
            // it may predate this system and the column cannot be null.
            if (latest.HasValue && donor.LastDonationDate != latest.Value)
            {
                donor.LastDonationDate = latest.Value;
                dc.SaveChanges();
            }
        }

        public IActionResult Read(
            int? donorId,
            DateOnly? fromDate,
            DateOnly? toDate,
            string? campName,
            int page = 1)
        {
            var query = ApplyFilters(dc.Donations, donorId, fromDate, toDate, campName);

            var pager = new PagerVM
            {
                PageSize = PageSize,
                TotalItems = query.Count()
            };

            // Keep an out-of-range page number in bounds so Skip never goes negative.
            if (page < 1)
            {
                page = 1;
            }

            if (page > pager.TotalPages)
            {
                page = pager.TotalPages;
            }

            pager.PageNumber = page;

            var donations = query
                            .Include(d => d.Donor)
                            .OrderByDescending(d => d.DonationDate)
                            .ThenByDescending(d => d.DonationId)
                            .Skip((page - 1) * PageSize)
                            .Take(PageSize)
                            .ToList();

            var vm = new DonationListVM
            {
                Donations = donations,
                DonorId = donorId,
                FromDate = fromDate,
                ToDate = toDate,
                CampName = campName,
                Donors = dc.Donors.OrderBy(d => d.FullName).ToList(),
                Pager = pager
            };

            return View(vm);
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
            // The Donor navigation property is a non-nullable reference type, so MVC
            // treats it as required even though the form never posts it. Drop that
            // entry or ModelState can never become valid.
            ModelState.Remove(nameof(Donation.Donor));

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

                SyncLastDonationDate(donation.DonorId);

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
            // Same reason as in Create: the unposted Donor navigation property is
            // reported as required and would otherwise block every save.
            ModelState.Remove(nameof(Donation.Donor));

            var existingDonation = dc.Donations.Find(id);
            if (existingDonation == null)
            {
                return NotFound();
            }

            var donorExists = dc.Donors.Any(d => d.DonorId == updatedDonation.DonorId);

            if (!donorExists)
            {
                ModelState.AddModelError("DonorId", "Donor does not exist.");
            }

            if (updatedDonation.DonationDate > DateOnly.FromDateTime(DateTime.Today))
            {
                ModelState.AddModelError("DonationDate",
                    "Donation date cannot be in the future.");
            }

            if (ModelState.IsValid)
            {
                // Remember the previous owner: if the donation is reassigned, the
                // donor it moved away from also needs its date recomputed.
                int previousDonorId = existingDonation.DonorId;

                existingDonation.DonorId = updatedDonation.DonorId;
                existingDonation.DonationDate = updatedDonation.DonationDate;
                existingDonation.VolumeMl = updatedDonation.VolumeMl;
                existingDonation.CampName = updatedDonation.CampName;

                dc.SaveChanges();

                SyncLastDonationDate(updatedDonation.DonorId);

                if (previousDonorId != updatedDonation.DonorId)
                {
                    SyncLastDonationDate(previousDonorId);
                }

                return RedirectToAction("Read");
            }

            ViewBag.Donors = dc.Donors.ToList();
            return View(updatedDonation);
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
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var info = dc.Donations.Find(id);
            if (info == null)
            {
                return NotFound();
            }

            int donorId = info.DonorId;

            dc.Donations.Remove(info);
            dc.SaveChanges();

            SyncLastDonationDate(donorId);

            return RedirectToAction("Read");
        }
        //==================================
        [HttpGet]
        public IActionResult TotalBloodCollected()
        {
            // Cast to int? so an empty Donation table returns SQL NULL -> 0
            // instead of failing to materialize into a non-nullable int.
            int totalVolume = dc.Donations.Sum(d => (int?)d.VolumeMl) ?? 0;

            return View(totalVolume);
        }
    }
}
