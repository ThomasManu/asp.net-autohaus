using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Auto.Models
{
    public class Hersteller
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }

        // Navigation Property zur "vielen" Seite (Automobile)
        public ICollection<Automobil>? Automobile { get; set; }
    }
}