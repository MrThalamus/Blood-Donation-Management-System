using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BloodDonation.EF.Tables;

public partial class Donation
{
    public int DonationId { get; set; }

    [Required]
    public int DonorId { get; set; }

    [Required(ErrorMessage = "Donation Date is required")]
    public DateOnly DonationDate { get; set; }

    [Required(ErrorMessage = "Volume in ML is required")]
    [Range(250, 500, ErrorMessage = "Volume must be between 250 and 500 mL.")]
    public int VolumeMl { get; set; }

    [Required(ErrorMessage = "Camp Name is required")]
    [StringLength(50, ErrorMessage = "Camp Name cannot exceed 50 characters.")]
    public string CampName { get; set; } = null!;

    [ValidateNever]
    public virtual Donor Donor { get; set; } = null!; // Navigation property to the Donor entity
}
