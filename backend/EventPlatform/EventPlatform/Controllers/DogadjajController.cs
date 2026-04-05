using DTO.StrucniDogadjaji;
using EventPlatform.Data;
using EventPlatform.Domen;
using EventPlatform.Models.Dogadjaji;
using EventPlatform.Models.Lokacija;
using EventPlatform.Models.Predavac;
using EventPlatform.Models.TipDogadjaja;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Polly;
using System.Threading.Tasks;

namespace EventPlatform.Controllers
{
    public class DogadjajController : Controller
    {

        public DogadjajController(Data.Context context, IHttpClientFactory httpClientFactory)
        {
            this.context = context;
            this.httpClientFactory = httpClientFactory;
        }
        public Data.Context context { get; }
        public IHttpClientFactory httpClientFactory { get; }

        [HttpGet]
        public IActionResult Create()  
        {
            var lokacije = context.Lokacije.ToList();
            ViewBag.lokacijeList = new SelectList(lokacije.Select
            (l => new { l.LokacijaID, Prikaz = l.Naziv + " (Kapacitet: " + l.Kapacitet + ")" }), "LokacijaID", "Prikaz");

            var tipovi = context.TipoviDogadjaja.ToList();
            ViewBag.tipoviList = new SelectList(tipovi, "TipDogadjajaID", "NazivTipa");

            var predavaci = context.Predavaci.ToList();
            ViewBag.predavaciList = new MultiSelectList(predavaci.Select
           (p => new { p.PredavacID, Prikaz = p.Ime + " " + p.Prezime + " - " + p.OblastStrucnosti }), "PredavacID", "Prikaz");

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
                ViewBag.ExceptionMessage = "Ne možemo uèitati dogadjaje, vreme isteklo";
                return View(new List<DogadjajViewModel>());
            }
            catch(HttpRequestException ex)
            {
                ViewBag.ExceptionMessage = "Ne možemo uèitati dogadjaje, dostignut je maksimalan broj pokušaja";
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
        public IActionResult Edit(int Id)
        {
            var odabranDogadjaj = context.StrucniDogadjaji
                                    .Include(sd => sd.Predavaci)
                                    .FirstOrDefault(sd => sd.StrucniDogadjajID == Id);
            if(odabranDogadjaj == null)
            {
                return NotFound();
            }

            var model = new DogadjajCreateViewModel
            {
                StrucniDogadjajID = odabranDogadjaj.StrucniDogadjajID,
                Naziv = odabranDogadjaj.Naziv,
                Agenda = odabranDogadjaj.Agenda,
                DatumVremeOdrzavanja = odabranDogadjaj.DatumVremeOdrzavanja,
                Trajanje = odabranDogadjaj.Trajanje,
                CenaKotizacije = odabranDogadjaj.CenaKotizacije,
                LokacijaID = odabranDogadjaj.LokacijaID,
                OdabraniPredavaciID = odabranDogadjaj.Predavaci.Select(p => p.PredavacID).ToList(),
                TipDogadjajaID = odabranDogadjaj.TipDogadjajaID
            };
            var lokacije = context.Lokacije.ToList();
            ViewBag.lokacijeList = new SelectList(lokacije.Select(l => new { l.LokacijaID, Prikaz = l.Naziv + " (Kapacitet: " + l.Kapacitet + ")" }), "LokacijaID", "Prikaz");
            ViewBag.tipoviList = new SelectList(context.TipoviDogadjaja.ToList(), "TipDogadjajaID", "NazivTipa");
            var predavaci = context.Predavaci.ToList();
            ViewBag.predavaciList = new MultiSelectList(predavaci.Select(p => new { p.PredavacID, Prikaz = p.Ime + " " + p.Prezime + " - " + p.OblastStrucnosti }), "PredavacID", "Prikaz");

            return View(model);
        }
        [HttpPost]
        public IActionResult Edit(DogadjajCreateViewModel model)
        {
            var dogadjajAzuriran = context.StrucniDogadjaji
                            .Include(sd => sd.Predavaci)
                            .FirstOrDefault(sd => sd.StrucniDogadjajID == model.StrucniDogadjajID);
            if(dogadjajAzuriran == null)
            {
                return NotFound();
            }

            dogadjajAzuriran.Naziv = model.Naziv;
            dogadjajAzuriran.Agenda = model.Agenda;
            dogadjajAzuriran.DatumVremeOdrzavanja = model.DatumVremeOdrzavanja;
            dogadjajAzuriran.Trajanje = model.Trajanje;
            dogadjajAzuriran.CenaKotizacije = model.CenaKotizacije;
            dogadjajAzuriran.LokacijaID = model.LokacijaID;
            dogadjajAzuriran.TipDogadjajaID = model.TipDogadjajaID;

            dogadjajAzuriran.Predavaci.Clear();
            dogadjajAzuriran.Predavaci = context.Predavaci
                                         .Where(p => model.OdabraniPredavaciID.Contains(p.PredavacID))
                                         .ToList();
            context.SaveChanges();

            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult Delete(int Id)
        {
            var dogadjaj = context.StrucniDogadjaji
                           .Include(sd => sd.Predavaci)
                           .FirstOrDefault(sd => sd.StrucniDogadjajID == Id);
            if(dogadjaj == null)
            {
                return NotFound();
            }
            dogadjaj.Predavaci.Clear();
            context.StrucniDogadjaji.Remove(dogadjaj);
            context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
