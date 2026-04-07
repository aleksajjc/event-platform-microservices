using DTO.Lokacije;
using DTO.Predavaci;
using DTO.StrucniDogadjaji;
using DTO.TipoviDogadjaja;
using EventPlatform.Data;
using EventPlatform.Domen;
using EventPlatform.Models.Dogadjaji;
using EventPlatform.Models.Lokacija;
using EventPlatform.Models.Predavac;
using EventPlatform.Models.TipDogadjaja;
using EventPlatform.Patterns;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Polly;
using System.Threading.Tasks;

namespace EventPlatform.Controllers
{
    public class DogadjajController : Controller
    {
        public Data.Context context { get; }
        public IHttpClientFactory httpClientFactory { get; }
        public CircuitBreaker _circuitBreaker { get; set; }
        public DogadjajController(Data.Context context, IHttpClientFactory httpClientFactory, CircuitBreaker circuitBreaker)
        {
            this.context = context;
            this.httpClientFactory = httpClientFactory;
            _circuitBreaker = circuitBreaker;
        }
        

        [HttpGet]
        public async Task<IActionResult> Create()  
        {
            var client = httpClientFactory.CreateClient("EventsAPI");
 
            var lokacije = await client.GetFromJsonAsync<List<LokacijaDTO>>("/Lokacije");
            var tipovi = await client.GetFromJsonAsync<List<TipDogadjajaDTO>>("/TipoviDogadjaja");
            var predavaci = await client.GetFromJsonAsync<List<PredavacDTO>>("/Predavaci");

            ViewBag.lokacijeList = new SelectList(lokacije.Select(l =>
                new { l.LokacijaID, Prikaz = l.Naziv + " (Kapacitet: " + l.Kapacitet + ")" }), "LokacijaID", "Prikaz");
            ViewBag.tipoviList = new SelectList(tipovi, "TipDogadjajaID", "NazivTipa");
            ViewBag.predavaciList = new MultiSelectList(predavaci.Select(p =>
                new { p.PredavacID, Prikaz = p.Ime + " " + p.Prezime + " - " + p.OblastStrucnosti }), "PredavacID", "Prikaz");
            
            return View(new DogadjajCreateViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(DogadjajCreateViewModel model)
        {
            var eventsHttpClient = httpClientFactory.CreateClient("EventsAPI");

            var noviDogadjaj = new StrucniDogadjajCreateDTO
            {
                Naziv = model.Naziv,
                Agenda = model.Agenda,
                DatumVremeOdrzavanja = model.DatumVremeOdrzavanja,
                Trajanje = model.Trajanje,
                CenaKotizacije = model.CenaKotizacije,
                LokacijaID = model.LokacijaID,
                PredavaciIDs = model.OdabraniPredavaciID,
                TipDogadjajaID = model.TipDogadjajaID
            };

            var response = await eventsHttpClient.PostAsJsonAsync("/Dogadjaji", noviDogadjaj);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            return View(model);
        }

        public async Task<IActionResult> Index()
        {
            var eventsHttpClient = httpClientFactory.CreateClient("EventsAPI");

            try
            {
                HttpResponseMessage? httpResponseMessage = null;

                var retryPolicy = Polly.Policy.Handle<HttpRequestException>()
                    .WaitAndRetryAsync(3, attemp => TimeSpan.FromMicroseconds(250));

                httpResponseMessage = await retryPolicy.ExecuteAsync<HttpResponseMessage>(async () =>
                {
                    httpResponseMessage = await eventsHttpClient.GetAsync("/Dogadjaji");

                    httpResponseMessage.EnsureSuccessStatusCode();

                    return httpResponseMessage;
                });

                var StrucniDogadjajDTOs = await httpResponseMessage.Content.ReadFromJsonAsync<List<StrucniDogadjajDTO>>();

                var dogadjajViewModels = StrucniDogadjajDTOs.Select(sd => new DogadjajViewModel
                {
                    StrucniDogadjajID = sd.StrucniDogadjajID,
                    Naziv = sd.Naziv,
                    Agenda = sd.Agenda,
                    DatumVremeOdrzavanja = sd.DatumVremeOdrzavanja,
                    Trajanje = sd.Trajanje,
                    CenaKotizacije = sd.CenaKotizacije,
                    Lokacija = new LokacijaViewModel
                    {
                        LokacijaID = sd.Lokacija.LokacijaID,
                        Naziv = sd.Lokacija.Naziv,
                        Adresa = sd.Lokacija.Adresa,
                        Kapacitet = sd.Lokacija.Kapacitet
                    },
                    Predavaci = sd.Predavaci.Select(p => new PredavacViewModel
                    {
                        PredavacID = p.PredavacID,
                        Ime = p.Ime,
                        Prezime = p.Prezime,
                        Titula = p.Titula,
                        OblastStrucnosti = p.OblastStrucnosti
                    }).ToList(),

                    TipDogadjaja = new TipDogadjajaViewModel
                    {
                        TipDogadjajaID = sd.TipDogadjaja.TipDogadjajaID,
                        NazivTipa = sd.TipDogadjaja.NazivTipa
                    }
                }).ToList();

                return View(dogadjajViewModels);
            }
            catch (TaskCanceledException ex)
            {
                ViewBag.ExceptionMessage = "Ne mo�emo u�itati dogadjaje, vreme isteklo";
                return View(new List<DogadjajViewModel>());
            }
            catch(HttpRequestException ex)
            {
                ViewBag.ExceptionMessage = "Ne mo�emo u�itati dogadjaje, dostignut je maksimalan broj poku�aja";
                return View(new List<DogadjajViewModel>());
            }

            /*
            var listaDogadjaja = context.StrucniDogadjaji
                                  .Include(sd => sd.Predavaci)
                                  .Include(sd => sd.TipDogadjaja)
                                  .Include(sd => sd.Lokacija)
                                  .Select(d => new DogadjajViewModel
                                  {
                                      StrucniDogadjajID = d.StrucniDogadjajID,
                                      Naziv = d.Naziv,
                                      Agenda = d.Agenda,
                                      DatumVremeOdrzavanja = d.DatumVremeOdrzavanja,
                                      Trajanje = d.Trajanje,
                                      CenaKotizacije = d.CenaKotizacije,
                                      Lokacija = new LokacijaViewModel { Naziv = d.Lokacija.Naziv, Adresa = d.Lokacija.Adresa },
                                      TipDogadjaja = new TipDogadjajaViewModel {NazivTipa = d.TipDogadjaja.NazivTipa},
                                      Predavaci = d.Predavaci.Select(p => new PredavacViewModel { Ime = p.Ime, Prezime = p.Prezime }).ToList()
                                  }).ToList();
            return View(listaDogadjaja); */
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int Id)
        {
            var client = httpClientFactory.CreateClient("EventsAPI");

            var dogadjaj = await client.GetFromJsonAsync<StrucniDogadjajDTO>($"/Dogadjaji/{Id}");

            if (dogadjaj == null) return NotFound();

            var model = new DogadjajCreateViewModel
            {
                StrucniDogadjajID = dogadjaj.StrucniDogadjajID,
                Naziv = dogadjaj.Naziv,
                Agenda = dogadjaj.Agenda,
                DatumVremeOdrzavanja = dogadjaj.DatumVremeOdrzavanja,
                Trajanje = dogadjaj.Trajanje,
                CenaKotizacije = dogadjaj.CenaKotizacije,
                LokacijaID = dogadjaj.Lokacija.LokacijaID,
                TipDogadjajaID = dogadjaj.TipDogadjaja.TipDogadjajaID,
                OdabraniPredavaciID = dogadjaj.Predavaci.Select(p => p.PredavacID).ToList()
            };

            var lokacije = await client.GetFromJsonAsync<List<LokacijaDTO>>("/Lokacije");
            var tipovi = await client.GetFromJsonAsync<List<TipDogadjajaDTO>>("/TipoviDogadjaja");
            var predavaci = await client.GetFromJsonAsync<List<PredavacDTO>>("/Predavaci");

            ViewBag.lokacijeList = new SelectList(lokacije.Select(l =>
                new { l.LokacijaID, Prikaz = l.Naziv + " (Kapacitet: " + l.Kapacitet + ")" }), "LokacijaID", "Prikaz");
            ViewBag.tipoviList = new SelectList(tipovi, "TipDogadjajaID", "NazivTipa");
            ViewBag.predavaciList = new MultiSelectList(predavaci.Select(p =>
                new { p.PredavacID, Prikaz = p.Ime + " " + p.Prezime + " - " + p.OblastStrucnosti }), "PredavacID", "Prikaz");

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(DogadjajCreateViewModel model)
        {
            var client = httpClientFactory.CreateClient("EventsAPI");

            var dogadjajDTO = new StrucniDogadjajCreateDTO
            {
                StrucniDogadjajID = model.StrucniDogadjajID,
                Naziv = model.Naziv,
                Agenda = model.Agenda,
                DatumVremeOdrzavanja = model.DatumVremeOdrzavanja,
                Trajanje = model.Trajanje,
                CenaKotizacije = model.CenaKotizacije,
                LokacijaID = model.LokacijaID,
                PredavaciIDs = model.OdabraniPredavaciID,
                TipDogadjajaID = model.TipDogadjajaID
            };

            var response = await client.PutAsJsonAsync("/Dogadjaji", dogadjajDTO);

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

            var response = await client.DeleteAsync($"/Dogadjaji/{Id}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            ViewBag.ExceptionMessage = "Greska pri brisanju dogadjaja";
            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            var client = httpClientFactory.CreateClient("EventsAPI");

            try
            {
                var response = await _circuitBreaker.ExecuteAsync(async () =>
                    {
                        var res = await client.GetAsync($"/Dogadjaji/{id}");
                        res.EnsureSuccessStatusCode();
                        return res;
                    });
                var dogadjaji = await response.Content.ReadFromJsonAsync<StrucniDogadjajDTO>();

                var listaDogadjaja = new DogadjajViewModel
                {
                    StrucniDogadjajID = dogadjaji.StrucniDogadjajID,
                    Naziv = dogadjaji.Naziv,
                    Agenda = dogadjaji.Agenda,
                    DatumVremeOdrzavanja = dogadjaji.DatumVremeOdrzavanja,
                    Trajanje = dogadjaji.Trajanje,
                    CenaKotizacije = dogadjaji.CenaKotizacije,
                    Lokacija = new LokacijaViewModel
                    {
                        Naziv = dogadjaji.Lokacija.Naziv,
                        Adresa = dogadjaji.Lokacija.Adresa,
                        Kapacitet = dogadjaji.Lokacija.Kapacitet
                    },
                    Predavaci = dogadjaji.Predavaci.Select(p => new PredavacViewModel
                    {
                        Ime = p.Ime,
                        Prezime = p.Prezime,
                        Titula = p.Titula,
                        OblastStrucnosti = p.OblastStrucnosti
                    }).ToList(),
                    TipDogadjaja = new TipDogadjajaViewModel
                    {
                        NazivTipa = dogadjaji.TipDogadjaja.NazivTipa
                    }
                };

                return View(listaDogadjaja);
            }
             catch (Exception ex) {
                ViewBag.Message = "Zao nam je, API trenutno nije dostupan. Pokusajte ponovo kasnije.";
                return View(new DogadjajViewModel()); 
            }
        }
    }
}