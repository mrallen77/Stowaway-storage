using System.ComponentModel.DataAnnotations;

namespace StowawayStorage.Models
{
    public class StorageUnit
    {
        public int Id { get; set; }

        [Required, StringLength(120)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(60)]
        public string Size { get; set; } = string.Empty; // e.g., "5x10"

        [Range(0, 999999)]
        public decimal MonthlyPrice { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
