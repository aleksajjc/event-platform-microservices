using DTO.Predavaci;
using EventPlatform.Data;
using EventPlatform.Domen;
using EventPlatform.Models.Dogadjaji;
using EventPlatform.Models.Predavac;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace EventPlatform.Controllers
{
    public class PredavacController : Controller
    {
        public PredavacController(Context context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }
        public Context _context { get; }
        public IHttpClientFactory _httpClientFactory { get; }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new PredavacCreateViewModel());
        }
        [HttpPost]
        public async Task<IActionResult> Create(PredavacCreateViewModel model)
        {
            var client = _httpClientFactory.CreateClient("EventsAPI");

            var noviPredavac = new PredavacCreateDTO
            {
                Ime = model.Ime,
                Prezime = model.Prezime,
                Titula = model.Titula,
                OblastStrucnosti = model.OblastStrucnosti
            };

            var response = await client.PostAsJsonAsync("/Predavaci", noviPredavac);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            return View(model);
        }
        public async Task<IActionResult> Index()
        {

            var client = _httpClientFactory.CreateClient("EventsAPI");

            var predavaci = await client.GetFromJsonAsync<List<PredavacDTO>>("/Predavaci");

            var listaPredavaci = predavaci.Select(p => new PredavacViewModel
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
            return View(listaPredavaci);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int Id)
        {
            var client = _httpClientFactory.CreateClient("EventsAPI");

            var predavac = await client.GetFromJsonAsync<PredavacDTO>($"/Predavaci/{Id}");

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
        public async Task<IActionResult> Edit(PredavacCreateViewModel model)
        {
            var client = _httpClientFactory.CreateClient("EventsAPI");

            var dto = new PredavacCreateDTO
            {
                PredavacID = model.PredavacID,
                Ime = model.Ime,
                Prezime = model.Prezime,
                Titula = model.Titula,
                OblastStrucnosti = model.OblastStrucnosti
            };

            var response = await client.PutAsJsonAsync("/Predavaci", dto);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int Id)
        {
            var client = _httpClientFactory.CreateClient("EventsAPI");

            var response = await client.DeleteAsync($"/Predavaci/{Id}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            ViewBag.ExceptionMessage = "Greška pri brisanju predavaca";
            return RedirectToAction("Index");
        }
    }
}
