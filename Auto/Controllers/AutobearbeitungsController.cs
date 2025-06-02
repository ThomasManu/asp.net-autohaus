using Auto.Data;
using Auto.Models;

using Auto.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Configuration;
using System.Text.Json;
using Utility = Auto.Services.Utility;

namespace Auto.Controllers
{
    public class AutobearbeitungsController : Controller
    {
        private ApplicationDbContext _context;

        public AutobearbeitungsController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult AutoIndex()
        {  
           
            var autoliste = _context.autos.Include(a => a.Hersteller).ToList();

            // Herstellername nach dem Laden der Daten setzen
            foreach (var auto in autoliste)
            {
                if (auto.Hersteller != null)
                {
                    auto.Herstellername = auto.Hersteller.Name;
                }
            }

            if (autoliste.Count > 0)
            {
                foreach (var aut in autoliste)
                {
                    if (aut.JsonBilderListe != null)
                    {
                        aut.Bilderliste= JsonSerializer.Deserialize<List<string>>(aut.JsonBilderListe);
                        //hier weise ich jedem base 64 String in der Liste den Format-Präfix zu
                        //aut.Bilderliste = aut.Bilderliste.Select(b => Utility.findformat(b)).ToList();
                    }
                   

                }
             
               

            }
            return View(autoliste);
        }

        public IActionResult CreateEditAuto(int id)
        {
            if (id != 0)
            {
                var item = _context.autos
                .Include(x => x.Hersteller) // Hier wird die Hersteller-Tabelle eingeschlossen
                .SingleOrDefault(x => x.Id == id);
                // var item = _context.autos.SingleOrDefault(x => x.Id == id);
                // Speichere die aktualisierte Liste in der Session


                if (item == null)
                {
                    return NotFound();
                }
                else
                {
                    
                        // Update the following line to handle potential null values for item.JsonBilderListe
                        if (!string.IsNullOrEmpty(item.JsonBilderListe))
                        {
                            // Diese Zeile deserialisiert die Bilderliste
                            item.Bilderliste = JsonSerializer.Deserialize<List<string>>(item.JsonBilderListe);

                            // Erst danach die Session aktualisieren, da Bilderliste jetzt den korrekten Wert hat
                            HttpContext.Session.SetString("Autopfad", JsonSerializer.Serialize(item.Bilderliste));
                        }
                        // Wenn item.JsonBilderListe null oder leer ist, geschieht hier nichts
                        // mit item.Bilderliste und die Session wird auch nicht gesetzt.
                        // Das ist die logische Konsequenz dieses Codes.
                    

                    if (item.Hersteller != null)
                    {
                        item.Herstellername = item.Hersteller.Name;
                    }
                    return View(item);
                }
            }
            return View();
        }

        [HttpPost]
        public IActionResult CreateEditAuto(Automobil auto)
        {
            var hersteller = _context.herstellers.FirstOrDefault(x => x.Name == auto.Herstellername);
            if (hersteller == null)
            {
                hersteller = new Hersteller { Name = auto.Herstellername };
                _context.herstellers.Add(hersteller);
                _context.SaveChanges();
            }

            auto.HerstellerId = hersteller.Id;

            // ⬇️ Bildpfade aus Session laden
            string? sessionData = HttpContext.Session.GetString("Autopfad");
            if (!string.IsNullOrEmpty(sessionData))
            {
                auto.JsonBilderListe = sessionData;
            }
            else
            {
                // Wenn keine Bilder da sind, leere Liste speichern
                auto.JsonBilderListe = JsonSerializer.Serialize(new List<string>());
            }

            if (auto.Id == 0)
            {
                _context.autos.Add(auto);
            }
            else
            {
                var autoInDerDatenbank = _context.autos.SingleOrDefault(x => x.Id == auto.Id);
                if (autoInDerDatenbank == null)
                {
                    return NotFound();
                }

                autoInDerDatenbank.HerstellerId = auto.HerstellerId;
                autoInDerDatenbank.Modell = auto.Modell;
                autoInDerDatenbank.Farbe = auto.Farbe;
                autoInDerDatenbank.Kilometerstand = auto.Kilometerstand;
                autoInDerDatenbank.Herstellername = auto.Herstellername;
                autoInDerDatenbank.Baujahr = auto.Baujahr;
                autoInDerDatenbank.Preis = auto.Preis;
                autoInDerDatenbank.Leistung = auto.Leistung;
                autoInDerDatenbank.TüvBis = auto.TüvBis;
                autoInDerDatenbank.JsonBilderListe = auto.JsonBilderListe;
                autoInDerDatenbank.Extras = auto.Extras;
                autoInDerDatenbank.Autobild = auto.Autobild;

                _context.autos.Update(autoInDerDatenbank);
            }

            _context.SaveChanges();

            // ❗ Optional: Session aufräumen nach Speichern
            HttpContext.Session.Remove("Autopfad");

            return RedirectToAction("AutoIndex");
        }

        [HttpPost]
        public IActionResult DeleteImage(int index)
        {
            string? sessionData = HttpContext.Session.GetString("Autopfad");
            if (!string.IsNullOrEmpty(sessionData))
            {
                List<string>? l = JsonSerializer.Deserialize<List<string>>(sessionData);

                if (l != null && index >= 0 && index < l.Count)
                {
                    // Extrahiere den Dateinamen aus dem relativen Pfad, um auch das Bild aus dem Verzeichnis Autobilder zu löschen
                    var dateiname = Path.GetFileName(l[index]);
                    var pfad = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Autobilder", dateiname);
                    if (System.IO.File.Exists(pfad))
                    {
                        System.IO.File.Delete(pfad); // Lösche die Datei
                    }
                    l.RemoveAt(index);  // Löscht das Bild an der übergebenen Position
                   
                      
                    
                }
                // Speichere die aktualisierte Liste in der Session
                HttpContext.Session.SetString("Autopfad", JsonSerializer.Serialize(l));
                
               

                return Json(l);  // Gib die aktualisierte Liste zurück
            }

            // Optional: Rückgabe einer benutzerfreundlichen Nachricht
            return Json(new { message = "Bild nicht gefunden oder ungültiger Index." });
        }


        public IActionResult deleteAuto(int id)
        {
            if (id != 0)
            {
                Automobil? a = _context.autos.FirstOrDefault(x => x.Id == id);
                if (a == null) return NotFound("Auto ist nicht vorhanden");
                List<string>? bilderListe = JsonSerializer.Deserialize<List<string>>(a.JsonBilderListe) ?? new List<string>();
                // Entferne die Bilder aus dem Dateisystem
                if (bilderListe != null && bilderListe.Count > 0)
                {
                    foreach (var bild in bilderListe)
                    {
                        // Extrahiere den Dateinamen aus dem relativen Pfad
                        var dateiname = Path.GetFileName(bild);
                        var pfad = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Autobilder", dateiname);
                        if (System.IO.File.Exists(pfad))
                        {
                            System.IO.File.Delete(pfad); // Lösche die Datei
                        }
                    }
                }   
                _context.autos.Remove(a);
                _context.SaveChanges();
                var hersteller = _context.herstellers.ToList();
                var auto = _context.autos.ToList();
                var herstellerToRemove = new List<Hersteller>();
                foreach (var h in hersteller)
                {
                    if (!auto.Any(a => a.HerstellerId == h.Id))
                    {
                        herstellerToRemove.Add(h);

                    }

                }
                foreach (var h in herstellerToRemove)
                {
                    hersteller.Remove(h);  // Entferne aus der hersteller-Liste
                    _context.herstellers.Remove(h); // Entferne aus der Datenbank
                }
                _context.SaveChanges();
            }
            return RedirectToAction("AutoIndex");
        }



        public IActionResult Optionen()
        {
            Parameter? vorhandeneParameter = _context.parameters.FirstOrDefault();
            if (vorhandeneParameter != null) return View(vorhandeneParameter);
            return View();
        }
        [HttpPost]
        public IActionResult Optionen(Parameter parameter)
        {
            Parameter? vorhandeneParameter = _context.parameters.FirstOrDefault();
            if (vorhandeneParameter != null)
            {
                vorhandeneParameter.emailadresse = parameter.emailadresse;
                vorhandeneParameter.telefonnummer = parameter.telefonnummer;
                _context.parameters.Update(vorhandeneParameter);
            }
            else
            {
                // Füge einen neuen Eintrag hinzu
                _context.parameters.Add(parameter);
            }
            _context.SaveChanges();
            return View();
        }

        #region
      
        [HttpPost]
        public async Task<IActionResult> UploadBase64(IFormFile file)
        {
            // Hole bestehende Session-Daten oder initialisiere eine neue Liste
            List<string> bilderListe = new();
            string? sessionData = HttpContext.Session.GetString("Autopfad");

            if (!string.IsNullOrEmpty(sessionData))
            {
                bilderListe = JsonSerializer.Deserialize<List<string>>(sessionData) ?? new List<string>();
            }

            if (file != null && file.Length > 0)
            {
                // Generiere GUID als Dateinamen + behalte Original-Erweiterung
                var extension = Path.GetExtension(file.FileName);
                var dateiname = $"{Guid.NewGuid()}{extension}";

                // Zielpfad innerhalb von wwwroot
                var pfad = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Autobilder", dateiname);

                // Sicherstellen, dass der Ordner existiert
                var ordner = Path.GetDirectoryName(pfad);
                if (!Directory.Exists(ordner))
                {
                    Directory.CreateDirectory(ordner);
                }

                // Datei speichern
                using (var stream = new FileStream(pfad, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Relativen Pfad (z. B. für Bildanzeige) oder Dateinamen speichern
                var relativerPfad = "/Autobilder/" + dateiname;
                bilderListe.Add(relativerPfad);

                // Session aktualisieren
                HttpContext.Session.SetString("Autopfad", JsonSerializer.Serialize(bilderListe));

                return Json(new { success = true, imagePath = relativerPfad });
            }

            return Json(new { success = false, message = "Keine Datei erhalten" });
        }


        // Aktion zum Abrufen aller gespeicherten Bilder
        //[HttpGet]
        //public IActionResult GetBase64Images()
        //{
        //    return Json(_base64ImageList);
        //}
        #endregion
    }
}
