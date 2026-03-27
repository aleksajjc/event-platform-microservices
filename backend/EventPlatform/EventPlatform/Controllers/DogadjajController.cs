using EventPlatform.Data;
using EventPlatform.Domen;
using EventPlatform.Models.Dogadjaji;
using EventPlatform.Models.Lokacija;
using EventPlatform.Models.Predavac;
using EventPlatform.Models.TipDogadjaja;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EventPlatform.Controllers
{
    public class DogadjajController : Controller
    {

        public DogadjajController(Context context)
        {
            this.context = context;
        }
        public Context context { get; }

        [HttpGet]
        public IActionResult Create()  
        {
            var lokacije = context.Lokacije.ToList();
            ViewBag.lokacijeList = new SelectList(lokacije, "LokacijaID", "Naziv");

            var tipovi = context.TipoviDogadjaja.ToList();
            ViewBag.tipoviList = new SelectList(tipovi, "TipDogadjajaID", "NazivTipa");

            var predavaci = context.Predavaci.ToList();
            ViewBag.predavaciList = new SelectList(predavaci, "PredavacID", "Ime");

            return View(new DogadjajCreateViewModel());
        }

        [HttpPost]
        public IActionResult Create(DogadjajCreateViewModel model)
        {

            var noviDogadjaj = new StrucniDogadjaj
            {
                Naziv = model.Naziv,
                Agenda = model.Agenda,
                DatumVremeOdrzavanja = model.DatumVremeOdrzavanja,
                Trajanje = model.Trajanje,
                CenaKotizacije = model.CenaKotizacije,
                LokacijaID = model.LokacijaID,
                TipDogadjajaID = model.TipDogadjajaID,
                Predavaci = context.Predavaci
                            .Where(p => model.OdabraniPredavaciID.Contains(p.PredavacID))
                            .ToList()
            };
            context.StrucniDogadjaji.Add(noviDogadjaj);
            context.SaveChanges();

            return RedirectToAction("Index");
        }


        public IActionResult Index()
        {
            var listaDogadjaja = context.StrucniDogadjaji
                                  .Include(sd => sd.Predavaci)
                                  .Include(sd => sd.TipDogadjaja)
                                  .Include(sd => sd.Lokacija)
                                  .Select(d => new DogadjajViewModel
                                  {
                                      Naziv = d.Naziv,
                                      Agenda = d.Agenda,
                                      DatumVremeOdrzavanja = d.DatumVremeOdrzavanja,
                                      Trajanje = d.Trajanje,
                                      CenaKotizacije = d.CenaKotizacije,
                                      Lokacija = new LokacijaViewModel { Naziv = d.Lokacija.Naziv, Adresa = d.Lokacija.Adresa },
                                      TipDogadjaja = new TipDogadjajaViewModel {NazivTipa = d.TipDogadjaja.NazivTipa},
                                      Predavaci = d.Predavaci.Select(p => new PredavacViewModel { Ime = p.Ime, Prezime = p.Prezime }).ToList()
                                  }).ToList();
            return View(listaDogadjaja);
        }
    }
}
