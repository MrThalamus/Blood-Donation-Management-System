using BloodDonation.EF.Tables;

namespace BloodDonation.Models
{
    public class DonorListVM
    {
        public List<Donor> Donors { get; set; } = new();

        // Current filter values, echoed back so the search form stays filled in.
        public string? Name { get; set; }

        public string? BloodGroup { get; set; }

        public string? City { get; set; }

        // Distinct cities already present in the data, for the city dropdown.
        public List<string> Cities { get; set; } = new();

        public PagerVM Pager { get; set; } = new();
    }
}
