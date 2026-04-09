using DTO.Prijave;
using DTO.StrucniDogadjaji;
using EventPlatform.Models.Dogadjaji;
using EventPlatform.Models.Predavac;
using EventPlatform.Models.Prijava;
using EventPlatform.Models.Ucesnik;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace EventPlatform.Controllers
{
    public class PrijavaController : Controller
    {
        public IHttpClientFactory _httpClientFactory { get; set; }
        public PrijavaController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var client = _httpClientFactory.CreateClient("EventsAPI");

            var dogadjaji = await client.GetFromJsonAsync<List<StrucniDogadjajDTO>>("/Dogadjaji");

            var dogadjajiZaComboBox = dogadjaji.Select(d => new
            {
                Id = d.StrucniDogadjajID,
                Prikaz = $"{d.Naziv} | {d.Agenda} | {d.Lokacija.Naziv} | {d.DatumVremeOdrzavanja:dd.MM.yyyy} | Predavači: " +
                         string.Join(", ", d.Predavaci.Select(p => p.Ime + " " + p.Prezime))
            }).ToList();

            ViewBag.Dogadjaji = new SelectList(dogadjajiZaComboBox, "Id", "Prikaz");

            return View(new PrijavaCreateViewModel());
        }
        [HttpPost]
        public async Task<IActionResult> Create(PrijavaCreateViewModel model)
        {
            var client = _httpClientFactory.CreateClient("PrijaveAPI");

            var novaPrijava = new PrijavaCreateDTO
            {
                StrucniDogadjajID = model.StrucniDogadjajID,
                DatumPrijave = DateTime.UtcNow,
                Ime = model.Ime,
                Prezime = model.Prezime,
                Email = model.Email
            };

            var response = await client.PostAsJsonAsync("/Prijave", novaPrijava);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public async Task<IActionResult> Index()
        {
            var clientPrijave = _httpClientFactory.CreateClient("PrijaveAPI");
            var clientDogadjaji = _httpClientFactory.CreateClient("EventsAPI");

            var svePrijave = await clientPrijave.GetFromJsonAsync<List<PrijavaDTO>>("/Prijave");
            var sviDogadjaji = await clientDogadjaji.GetFromJsonAsync<List<StrucniDogadjajDTO>>("/Dogadjaji");


            var listaPrijava = svePrijave.Select(p =>
            {
                var pronadjenDogadjaj = sviDogadjaji.FirstOrDefault(d => d.StrucniDogadjajID == p.StrucniDogadjajID);
                return new PrijavaViewModel
                {
                    Ucesnik = new UcesnikViewModel
                    {
                        UcesnikID = p.Ucesnik.UcesnikID,
                        Ime = p.Ucesnik.Ime,
                        Prezime = p.Ucesnik.Prezime,
                        Email = p.Ucesnik.Email
                    },

                    Dogadjaj = new DogadjajViewModel
                    {
                        StrucniDogadjajID = p.StrucniDogadjajID,
                        Naziv = pronadjenDogadjaj?.Naziv ?? "Nepoznat događaj", 
                        Agenda = pronadjenDogadjaj?.Agenda ?? "",
                        DatumVremeOdrzavanja = pronadjenDogadjaj?.DatumVremeOdrzavanja ?? DateTime.MinValue,
                        Lokacija = pronadjenDogadjaj?.Lokacija != null ? new Models.Lokacija.LokacijaViewModel
                        {
                            Naziv = pronadjenDogadjaj.Lokacija.Naziv,
                            Adresa = pronadjenDogadjaj.Lokacija.Adresa
                        } : null
                    },

                    Predavaci = pronadjenDogadjaj?.Predavaci?.Select(pred => new PredavacViewModel
                    {
                        Ime = pred.Ime,
                        Prezime = pred.Prezime
                    }).ToList() ?? new List<PredavacViewModel>()
                };
            }).ToList();

            return View(listaPrijava);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int ucesnikId, int dogadjajId)
        {
            var client = _httpClientFactory.CreateClient("PrijaveAPI");

            var prijava = await client.GetFromJsonAsync<PrijavaDTO>($"/Prijave/{ucesnikId}/{dogadjajId}");

            if(prijava == null)
            {
                return NotFound();
            }

            var model = new PrijavaCreateViewModel
            {
                UcesnikID = prijava.Ucesnik.UcesnikID, 
                StrucniDogadjajID = prijava.StrucniDogadjajID,
                Ime = prijava.Ucesnik.Ime,
                Prezime = prijava.Ucesnik.Prezime,
                Email = prijava.Ucesnik.Email
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(PrijavaCreateViewModel model)
        {
            var client = _httpClientFactory.CreateClient("PrijaveAPI");

            var dto = new PrijavaCreateDTO
            {
                StrucniDogadjajID = model.StrucniDogadjajID,
                Ime = model.Ime,
                Prezime = model.Prezime,
                Email = model.Email
            };

            var response = await client.PutAsJsonAsync("/Prijave", dto);

            return RedirectToAction("Index");
       
        }
    }
}
