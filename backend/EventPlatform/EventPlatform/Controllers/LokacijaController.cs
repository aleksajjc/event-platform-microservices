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
                                    LokacijaID = l.LokacijaID,
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

        [HttpGet]
        public IActionResult Edit(int Id)
        {
            var lokacija = context.Lokacije
                           .FirstOrDefault(l => l.LokacijaID == Id);
            if(lokacija == null)
            {
                return NotFound();
            }
            var model = new LokacijaCreateViewModel
            {
                LokacijaID = lokacija.LokacijaID,
                Naziv = lokacija.Naziv,
                Adresa = lokacija.Adresa,
                Kapacitet = lokacija.Kapacitet
            };
            return View(model);
        }
        [HttpPost]
        public IActionResult Edit(LokacijaCreateViewModel model)
        {
            var lokacija = context.Lokacije
                            .FirstOrDefault(l => l.LokacijaID == model.LokacijaID);
            if(lokacija == null)
            {
                return NotFound();
            }
            lokacija.Naziv = model.Naziv;
            lokacija.Adresa = model.Adresa;
            lokacija.Kapacitet = model.Kapacitet;
            context.SaveChanges();

            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult Delete(int Id)
        {
            var lokacija = context.Lokacije
                            .FirstOrDefault(l => l.LokacijaID == Id);

            context.Lokacije.Remove(lokacija);
            context.SaveChanges();
            return RedirectToAction("Index");           
        }
    }
}
