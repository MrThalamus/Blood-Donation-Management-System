namespace BloodDonation.Models
{
    /// <summary>
    /// Single source of truth for the valid blood groups.
    /// Used by controller validation and by the dropdowns in the views.
    /// </summary>
    public static class BloodGroups
    {
        public static readonly string[] All =
        {
            "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-"
        };

        public static bool IsValid(string? bloodGroup)
        {
            return !string.IsNullOrWhiteSpace(bloodGroup)
                   && All.Contains(bloodGroup.Trim());
        }
    }
}
