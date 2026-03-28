using EventPlatform.Data;
using EventPlatform.Domen;
using EventPlatform.Models.Dogadjaji;
using EventPlatform.Models.Predavac;
using Microsoft.AspNetCore.Mvc;

namespace EventPlatform.Controllers
{
    public class PredavacController : Controller
    {
        public PredavacController(Context context)
        {
            this.context = context;
        }
        public Context context { get; }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new PredavacCreateViewModel());
        }
        [HttpPost]
        public IActionResult Create(PredavacCreateViewModel model)
        {
            var noviPredavac = new Predavac
            {
                Ime = model.Ime,
                Prezime = model.Prezime,
                Titula = model.Titula,
                OblastStrucnosti = model.OblastStrucnosti
            };
            context.Predavaci.Add(noviPredavac);
            context.SaveChanges();

            return RedirectToAction("Index");
        }
        public IActionResult Index()
        {
            var predavaci = context.Predavaci.Select(p => new PredavacViewModel
            {
                PredavacID = p.PredavacID,
                Ime = p.Ime,
                Prezime = p.Prezime,
                Titula = p.Titula,
                OblastStrucnosti = p.OblastStrucnosti,
                OdabraniStrucniDogadjaji = p.StrucniDogadjaji.Select(sd => new DogadjajViewModel
                {
                    Naziv = sd.Naziv,
                    DatumVremeOdrzavanja = sd.DatumVremeOdrzavanja
                }).ToList()
            }).ToList();
            return View(predavaci);
        }
    }
}
