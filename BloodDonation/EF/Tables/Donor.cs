using System;
using System.Collections.Generic;

namespace BloodDonation.EF.Tables;

public partial class Donor
{
    public int DonorId { get; set; }

    public string FullName { get; set; } = null!;

    public string BloodGroup { get; set; } = null!;

    public string ContactNo { get; set; } = null!;

    public string City { get; set; } = null!;

    public DateOnly LastDonationDate { get; set; }

    public virtual ICollection<Donation> Donations { get; set; } = new List<Donation>();
}
