using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BloodDonation.EF.Tables;

public partial class Donor
{
    public int DonorId { get; set; }
    [Required(ErrorMessage = "Full Name is required")]
    [StringLength(100, MinimumLength = 3,
        ErrorMessage = "Full Name must be between 3 and 100 characters.")]
    public string FullName { get; set; } = null!;
    [Required(ErrorMessage = "Blood Group is required")]
    public string BloodGroup { get; set; } = null!;
    [Required(ErrorMessage = "Contact Number is required")]
    [RegularExpression(@"^01[3-9]\d{8}$",
        ErrorMessage = "Invalid Bangladeshi mobile number.")]
    public string ContactNo { get; set; } = null!;
    [Required(ErrorMessage = "City is required")]
    [StringLength(50,
        ErrorMessage = "City name cannot exceed 50 characters.")]
    public string City { get; set; } = null!;

    public DateOnly LastDonationDate { get; set; } // Navigation property for the related donations

    // One-to-many relationship: A donor can have multiple donations
    public virtual ICollection<Donation> Donations { get; set; } = new List<Donation>();

}
