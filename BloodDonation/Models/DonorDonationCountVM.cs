namespace BloodDonation.Models
{
    public class DonorDonationCountVM
    {
        public int DonorId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public int TotalDonations { get; set; }
    }
}
