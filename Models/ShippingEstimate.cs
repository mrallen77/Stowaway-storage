using System.ComponentModel.DataAnnotations;

namespace StowawayStorage.Models
{
    public class ShippingEstimate
    {
        [Required]
        [Range(0.1, 200)]
        public double WeightLbs { get; set; }

        [Required]
        [Range(1, 100)]
        public double LengthInches { get; set; }

        [Required]
        [Range(1, 100)]
        public double WidthInches { get; set; }

        [Required]
        [Range(1, 100)]
        public double HeightInches { get; set; }

        [Required]
        [StringLength(50)]
        public string DestinationZip { get; set; }

        public double EstimatedCost { get; set; }
    }
}
