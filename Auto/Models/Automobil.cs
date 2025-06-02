using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Auto.Models
{
    public class Automobil
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Fremdschlüssel
        public int HerstellerId { get; set; }

        // Navigation Property zum Hersteller
        public Hersteller? Hersteller { get; set; }

        // Restliche Properties bleiben unverändert
        public string? Modell { get; set; }
        public string? Farbe { get; set; }
        public int Kilometerstand { get; set; }
        public string? Baujahr { get; set; }
        public double Preis { get; set; }
        public int Leistung { get; set; }
        public DateOnly TüvBis { get; set; }
        public byte[]? Autobild { get; set; }
        public string? JsonBilderListe { get; set; }
        public string? BildPfad { get; set; } // Relativer Pfad zum Bild
        public string? Extras { get; set; }
        [NotMapped]
        public List<string>? ExtraListe { get; set; }

      
        public List<string>? Bilderliste { get; set; }

        [NotMapped]
        public string? image { get; set; }
        [NotMapped]
        public string? Herstellername { get; set; }

        public Automobil()
        {
            ExtraListe = new List<string>();
            Bilderliste = new List<string>();
           
        }


        // ... (deine anderen Properties)
    }
}