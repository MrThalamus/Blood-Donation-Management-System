using BloodDonation.EF.Tables;

namespace BloodDonation.Models
{
    public class DonationListVM
    {
        public List<Donation> Donations { get; set; } = new();

        // Current filter values, echoed back so the search form stays filled in.
        public int? DonorId { get; set; }

        public DateOnly? FromDate { get; set; }

        public DateOnly? ToDate { get; set; }

        public string? CampName { get; set; }

        // All donors, for the donor dropdown.
        public List<Donor> Donors { get; set; } = new();

        public PagerVM Pager { get; set; } = new();
    }
}
