using DTO.TipoviDogadjaja;
using EventPlatform.Data;
using EventPlatform.Models.Dogadjaji;
using EventPlatform.Models.TipDogadjaja;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace EventPlatform.Controllers
{
    public class TipDogadjajaController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public TipDogadjajaController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("EventsAPI");
            var tipovi = await client.GetFromJsonAsync<List<TipDogadjajaDTO>>("/TipoviDogadjaja");

            var model = tipovi.Select(t => new TipDogadjajaViewModel
            {
                TipDogadjajaID = t.TipDogadjajaID,
                NazivTipa = t.NazivTipa,
                StrucniDogadjaji = t.StrucniDogadjaji?.Select(sd => new DogadjajViewModel
                {
                    StrucniDogadjajID = sd.StrucniDogadjajID,
                    Naziv = sd.Naziv,
                    DatumVremeOdrzavanja = sd.DatumVremeOdrzavanja
                }).ToList() ?? new List<DogadjajViewModel>()
            }).ToList();

            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new TipDogadjajaCreateViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(TipDogadjajaCreateViewModel model)
        {
            var client = _httpClientFactory.CreateClient("EventsAPI");
            var dto = new TipDogadjajaCreateDTO
            {
                NazivTipa = model.NazivTipa
            };

            var response = await client.PostAsJsonAsync("/TipoviDogadjaja", dto);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var client = _httpClientFactory.CreateClient("EventsAPI");
            var tip = await client.GetFromJsonAsync<TipDogadjajaDTO>($"/TipoviDogadjaja/{id}");

            if (tip == null) return NotFound();

            var model = new TipDogadjajaCreateViewModel
            {
                TipDogadjajaID = tip.TipDogadjajaID,
                NazivTipa = tip.NazivTipa
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(TipDogadjajaCreateViewModel model)
        {
            var client = _httpClientFactory.CreateClient("EventsAPI");
            var dto = new TipDogadjajaCreateDTO
            {
                TipDogadjajaID = model.TipDogadjajaID,
                NazivTipa = model.NazivTipa
            };

            var response = await client.PutAsJsonAsync("/TipoviDogadjaja", dto);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var client = _httpClientFactory.CreateClient("EventsAPI");
            var response = await client.DeleteAsync($"/TipoviDogadjaja/{id}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            ViewBag.ExceptionMessage = "Greška pri brisanju tipa događaja.";
            return RedirectToAction("Index");
        }
    }
}
