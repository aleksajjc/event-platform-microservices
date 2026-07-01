using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using EventPlatform.Models;
using Microsoft.AspNetCore.Mvc;

namespace EventPlatform.Controllers
{
    public class RacuniEventSourcingController : Controller
    {
        private readonly HttpClient _httpClient;

        public RacuniEventSourcingController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("EventsAPI");
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Lookup(int id)
        {
            var response = await _httpClient.GetAsync($"/RacuniEventSourcing/state/{id}");
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Details), new { id = id });
            }

            TempData["Error"] = "Račun sa unetim ID-jem nije pronađen.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var response = await _httpClient.GetAsync($"/RacuniEventSourcing/state/{id}");
            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var content = await response.Content.ReadAsStringAsync();
                var racun = JsonSerializer.Deserialize<RacunUcesnikaViewModel>(content, options);
                return View(racun);
            }

            return NotFound();
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateRacunViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var response = await _httpClient.PostAsJsonAsync("/RacuniEventSourcing/create", model);
            
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Račun uspešno kreiran.";
                return RedirectToAction(nameof(Details), new { id = model.UcesnikID });
            }
            
            TempData["Error"] = await response.Content.ReadAsStringAsync();
            return View(model);
        }

        public IActionResult Deposit(int id)
        {
            return View(new AmountRacunViewModel { UcesnikID = id });
        }

        [HttpPost]
        public async Task<IActionResult> Deposit(AmountRacunViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var response = await _httpClient.PostAsJsonAsync("/RacuniEventSourcing/deposit", model);
            
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Uplata uspešno obavljena.";
                return RedirectToAction(nameof(Details), new { id = model.UcesnikID });
            }
            
            TempData["Error"] = await response.Content.ReadAsStringAsync();
            return View(model);
        }

        public IActionResult Withdraw(int id)
        {
            return View(new AmountRacunViewModel { UcesnikID = id });
        }

        [HttpPost]
        public async Task<IActionResult> Withdraw(AmountRacunViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var response = await _httpClient.PostAsJsonAsync("/RacuniEventSourcing/withdraw", model);
            
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Isplata uspešno obavljena.";
                return RedirectToAction(nameof(Details), new { id = model.UcesnikID });
            }
            
            TempData["Error"] = await response.Content.ReadAsStringAsync();
            return View(model);
        }

        public IActionResult Block(int id)
        {
            return View(new BlockRacunViewModel { UcesnikID = id });
        }

        [HttpPost]
        public async Task<IActionResult> Block(BlockRacunViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var response = await _httpClient.PostAsJsonAsync("/RacuniEventSourcing/block", model);
            
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Račun uspešno blokiran.";
                return RedirectToAction(nameof(Details), new { id = model.UcesnikID });
            }
            
            TempData["Error"] = await response.Content.ReadAsStringAsync();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Unblock(int id)
        {
            var response = await _httpClient.PostAsync($"/RacuniEventSourcing/unblock/{id}", null);
            
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Račun uspešno odblokiran.";
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["Error"] = string.IsNullOrWhiteSpace(error) ? $"Greška: {response.StatusCode}" : error;
            }
            
            return RedirectToAction(nameof(Details), new { id = id });
        }

        public async Task<IActionResult> History(int id)
        {
            var response = await _httpClient.GetAsync($"/RacuniEventSourcing/history/{id}");
            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var content = await response.Content.ReadAsStringAsync();
                var history = JsonSerializer.Deserialize<List<EventHistoryViewModel>>(content, options);
                
                ViewBag.UcesnikID = id;
                return View(history);
            }

            return NotFound();
        }
    }
}
