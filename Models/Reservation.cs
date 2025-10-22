using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StowawayStorage.Models
{
    public class Reservation
    {
        public int Id { get; set; }

        [Required]
        public int UnitId { get; set; }
        public StorageUnit? Unit { get; set; }

        // Stored in UTC
        [Required]
        public DateTime StartDateUtc { get; set; }

        [Required]
        public DateTime EndDateUtc { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [StringLength(240)]
        public string? Notes { get; set; }

        public DateTime CreatedUtc { get; set; }
    }
}
