using EventPlatform.Data;
using EventPlatform.Domen;
using EventPlatform.Models.Dogadjaji;
using EventPlatform.Models.Lokacija;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EventPlatform.Controllers
{
    public class LokacijaController : Controller
    {
        public LokacijaController(Context context)
        {
            this.context = context;
        }
        public Context context { get; }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new LokacijaCreateViewModel());
        }

        [HttpPost]
        public IActionResult Create(LokacijaCreateViewModel model)
        {
            var novaLokacija = new Lokacija
            {
                Naziv = model.Naziv,
                Adresa = model.Adresa,
                Kapacitet = model.Kapacitet
            };
            context.Lokacije.Add(novaLokacija);
            context.SaveChanges();

            return RedirectToAction("Index");
        }


        public IActionResult Index()
        {
            var listaLokacija = context.Lokacije
                                .Include(l => l.StrucniDogadjaji)
                                .Select(l => new LokacijaViewModel
                                {
                                    Naziv = l.Naziv,
                                    Adresa = l.Adresa,
                                    Kapacitet = l.Kapacitet,
                                    OdabraniStrucniDogadjaji = l.StrucniDogadjaji.Select(sd => new DogadjajViewModel
                                    {
                                        Naziv = sd.Naziv,
                                        DatumVremeOdrzavanja = sd.DatumVremeOdrzavanja,
                                        Trajanje = sd.Trajanje
                                    }).ToList()
                                }).ToList();
            return View(listaLokacija);
        }
    }
}
