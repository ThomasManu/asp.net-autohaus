using System.Diagnostics;
using System.Text.Json;
using Auto.Data;
using Auto.Models;
using Auto.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Auto.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Angebotsliste(int? herstellerId)
        {
            List<Automobil> aut = new();
            Parameter? parameter = _context.parameters.FirstOrDefault();
            ViewBag.parameter = parameter;
            // Hole die Liste der Autos. Wenn herstellerId keinen Wert hat oder 0 ist, werden alle Autos angezeigt.
            aut = _context.autos.Include(a => a.Hersteller)
                               .Where(a => !herstellerId.HasValue || herstellerId.Value == 0 || a.HerstellerId == herstellerId.Value)
                               .ToList();


            if (aut.Count > 0)
            {
                foreach (var auto in aut)
                {
                    if (auto.JsonBilderListe != "")
                    {
                        auto.Bilderliste = JsonSerializer.Deserialize<List<string>>(auto.JsonBilderListe);
                        //hier weise ich jedem base 64 String in der Liste den Format-Präfix zu
                        //auto.Bilderliste = auto.Bilderliste.Select(b => Utility.findformat(b)).ToList();
                        auto.Herstellername = auto.Hersteller?.Name;
                    }
                }

                ViewBag.herstellerliste = _context.herstellers.ToList();
                return (View(aut));
            }

            return View(aut);
        }

        public IActionResult detaills(int id)
        {
            Parameter parameter;
            parameter = _context.parameters.FirstOrDefault();
            if (parameter == null)
            {
                parameter = new Parameter
                {
                    emailadresse = "example@web.de",
                    telefonnummer = "123456"
                };
            }
            ViewBag.parameter = parameter;
            Automobil? auto = _context.autos
             .Include(a => a.Hersteller)  // Lade den Hersteller mit dem Auto
             .FirstOrDefault(x => x.Id == id);
            if (auto == null) return NotFound("kein Auto mit dieser Id vorhanden");
            if (auto.JsonBilderListe != "")
            {
                auto.Bilderliste = JsonSerializer.Deserialize<List<string>>(auto.JsonBilderListe);
                //hier weise ich jedem base 64 String in der Liste den Format-Präfix zu
                //auto.Bilderliste = auto.Bilderliste.Select(b => Utility.findformat(b)).ToList();
                
            }
            if ((auto.Extras != "") && (auto.Extras != null))
            {
                string[] splitte = auto.Extras.Split(',');
                for (int i = 0; i < splitte.Length; i++)
                {
                    auto.ExtraListe.Add(splitte[i]);
                }

            }

            auto.Herstellername = auto.Hersteller?.Name;

            return View(auto);
        }
        public IActionResult Privacy()
        {
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
