using DTO.Lokacije;
using EventPlatform.Data;
using EventPlatform.Domen;
using EventPlatform.Models.Dogadjaji;
using EventPlatform.Models.Lokacija;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace EventPlatform.Controllers
{
    public class LokacijaController : Controller
    {
        public LokacijaController(Context context, IHttpClientFactory httpClientFactory)
        {
            this.context = context;
            this.httpClientFactory = httpClientFactory;
        }
        public Context context { get; }
        public IHttpClientFactory httpClientFactory { get; }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new LokacijaCreateViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(LokacijaCreateViewModel model)
        {
            var novaLokacija = new LokacijaCreateDTO
            {
                Naziv = model.Naziv,
                Adresa = model.Adresa,
                Kapacitet = model.Kapacitet,
            };

            var client = httpClientFactory.CreateClient("EventsAPI");

            var response = await client.PostAsJsonAsync("/Lokacije", novaLokacija);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            return View(model);
        }


        public async Task<IActionResult> Index()
        {
            
            var client = httpClientFactory.CreateClient("EventsAPI");

            var lokacije = await client.GetFromJsonAsync<List<LokacijaDTO>>("/Lokacije");

            var listaLokacija = lokacije.Select(l => new LokacijaViewModel
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
        public async Task<IActionResult> Edit(int Id)
        {
            var client = httpClientFactory.CreateClient("EventsAPI");

            var lokacija = await client.GetFromJsonAsync<LokacijaDTO>($"/Lokacije/{Id}");

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
        public async Task<IActionResult> Edit(LokacijaCreateViewModel model)
        {
            var client = httpClientFactory.CreateClient("EventsAPI");

            var lokacijaDTO = new LokacijaCreateDTO
            {
                LokacijaID = model.LokacijaID,
                Naziv = model.Naziv,
                Adresa = model.Adresa,
                Kapacitet = model.Kapacitet
            };

            var response = await client.PutAsJsonAsync("/Lokacije",lokacijaDTO);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int Id)
        {
            var client = httpClientFactory.CreateClient("EventsAPI");

            var response = await client.DeleteAsync($"/Lokacije/{Id}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            ViewBag.ExceptionMessage = "Greška pri brisanju lokacije";
            return RedirectToAction("Index");
        }
    }
}
