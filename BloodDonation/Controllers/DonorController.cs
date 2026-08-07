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
        private const int PageSize = 10;

        BloodBankDbContext dc;
        public DonorController(BloodBankDbContext dc)
        {
            this.dc = dc;
        }
        /// <summary>
        /// Applies the donor search filters. Each filter is optional and only
        /// narrows the query when a value is supplied. Shared by Read and Filter
        /// so the two screens can never disagree about what a filter means.
        /// </summary>
        private IQueryable<Donor> ApplyFilters(
            IQueryable<Donor> query,
            string? name,
            string? bloodGroup,
            string? city)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(d => d.FullName.Contains(name.Trim()));
            }

            if (!string.IsNullOrWhiteSpace(bloodGroup))
            {
                query = query.Where(d => d.BloodGroup == bloodGroup.Trim());
            }

            if (!string.IsNullOrWhiteSpace(city))
            {
                query = query.Where(d => d.City == city.Trim());
            }

            return query;
        }

        /// <summary>
        /// Distinct cities already recorded against donors, for the city dropdown.
        /// </summary>
        private List<string> GetCities()
        {
            return dc.Donors
                     .Select(d => d.City)
                     .Distinct()
                     .OrderBy(c => c)
                     .ToList();
        }

        /// <summary>
        /// Runs the filtered query for one page of donors and returns the
        /// populated view model. Shared by Read and Filter.
        /// </summary>
        private DonorListVM BuildDonorList(
            string? name,
            string? bloodGroup,
            string? city,
            int page)
        {
            var query = ApplyFilters(dc.Donors, name, bloodGroup, city);

            var pager = new PagerVM
            {
                PageSize = PageSize,
                TotalItems = query.Count()
            };

            // A page number out of range (bookmarked, hand-typed, or left over
            // after a filter shrank the result set) must be pulled back in range,
            // otherwise Skip receives a negative count and throws.
            if (page < 1)
            {
                page = 1;
            }

            if (page > pager.TotalPages)
            {
                page = pager.TotalPages;
            }

            pager.PageNumber = page;

            var donors = query
                         .OrderBy(d => d.DonorId)
                         .Skip((page - 1) * PageSize)
                         .Take(PageSize)
                         .ToList();

            return new DonorListVM
            {
                Donors = donors,
                Name = name,
                BloodGroup = bloodGroup,
                City = city,
                Cities = GetCities(),
                Pager = pager
            };
        }

        public IActionResult Read(string? name, string? bloodGroup, string? city, int page = 1)
        {
            return View(BuildDonorList(name, bloodGroup, city, page));
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View(new Donor());
        }
        [HttpPost]
        public IActionResult Create(Donor donor) //================================================
        {
            if (!BloodGroups.IsValid(donor.BloodGroup))
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

            if (!BloodGroups.IsValid(donor.BloodGroup))
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
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                var existingDonor = dc.Donors.Find(id);

                if (existingDonor == null)
                {
                    return NotFound();
                }

                // A donor with donation history cannot be removed: the Donation
                // foreign key would be orphaned. Explain it instead of failing.
                int donationCount = dc.Donations.Count(d => d.DonorId == id);

                if (donationCount > 0)
                {
                    TempData["Error"] = $"Cannot delete {existingDonor.FullName}. " +
                        $"This donor has {donationCount} donation record(s). " +
                        "Delete those donations first.";

                    return RedirectToAction("Read");
                }

                dc.Donors.Remove(existingDonor);
                dc.SaveChanges();

                TempData["Success"] = $"Donor {existingDonor.FullName} deleted successfully.";

                return RedirectToAction("Read");
            }
            catch (Exception)
            {
                TempData["Error"] = "Unable to delete this donor. It may be referenced by other records.";
                return RedirectToAction("Read");
            }
        }
        //==================================================================
        [HttpGet]
        public IActionResult Filter(string? bloodGroup, string? city, int page = 1)
        {
            return View(BuildDonorList(null, bloodGroup, city, page));
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
