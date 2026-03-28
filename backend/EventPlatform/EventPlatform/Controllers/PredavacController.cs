using EventPlatform.Data;
using EventPlatform.Domen;
using EventPlatform.Models.Dogadjaji;
using EventPlatform.Models.Predavac;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        [HttpGet]
        public IActionResult Edit(int Id)
        {
            var predavac = context.Predavaci
                            .FirstOrDefault(p => p.PredavacID == Id);

            if(predavac == null)
            {
                return NotFound();
            }
            var model = new PredavacCreateViewModel
            {
                PredavacID = predavac.PredavacID,
                Ime = predavac.Ime,
                Prezime = predavac.Prezime,
                Titula = predavac.Titula,
                OblastStrucnosti = predavac.OblastStrucnosti
            };
            return View(model);
        }
        [HttpPost]
        public IActionResult Edit(PredavacCreateViewModel model)
        {
            var predavac = context.Predavaci
                                .FirstOrDefault(p => p.PredavacID == model.PredavacID);
            if(predavac == null)
            {
                return NotFound();
            }
            predavac.Ime = model.Ime;
            predavac.Prezime = model.Prezime;
            predavac.Titula = model.Titula;
            predavac.OblastStrucnosti = model.OblastStrucnosti;

            context.SaveChanges();

            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult Delete(int Id)
        {
            var predavac = context.Predavaci
                            .FirstOrDefault(p => p.PredavacID == Id);
            if(predavac == null)
            {
                return NotFound();
            }
            context.Predavaci.Remove(predavac);
            context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
