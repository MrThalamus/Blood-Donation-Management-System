using BloodDonation.EF.Tables;

namespace BloodDonation.Models
{
    public class DashboardVM
    {
        public string WelcomeName { get; set; } = string.Empty;

        public int TotalDonors { get; set; }

        public int TotalDonations { get; set; }

        public int TotalBloodCollected { get; set; }

        public int TotalBloodGroups { get; set; }

        public List<BloodGroupStatVM> BloodGroupBreakdown { get; set; } = new();

        public List<Donation> RecentDonations { get; set; } = new();

        public int DonationsLast30Days { get; set; }

        public List<string> BloodGroupsWithNoDonors { get; set; } = new();
    }
}
