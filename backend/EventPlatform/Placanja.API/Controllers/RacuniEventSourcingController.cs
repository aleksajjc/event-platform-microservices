using Microsoft.AspNetCore.Mvc;
using Placanja.API.Models.EventSourcing;
using Placanja.API.Services;

namespace Placanja.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RacuniEventSourcingController : ControllerBase
    {
        private readonly EventStoreRepository _repository;

        public RacuniEventSourcingController(EventStoreRepository repository)
        {
            _repository = repository;
        }

        public class CreateAccountRequest
        {
            public int UcesnikID { get; set; }
            public string Ime { get; set; }
            public string Prezime { get; set; }
            public string Email { get; set; }
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateAccountRequest request)
        {
            var existing = await _repository.LoadAsync<RacunUcesnikaAggregate>(request.UcesnikID);
            if (existing != null)
                return BadRequest("Racun vec postoji.");

            var racun = RacunUcesnikaAggregate.Create(request.UcesnikID, request.Ime, request.Prezime, request.Email);
            await _repository.SaveAsync(racun);
            
            return Ok($"Racun kreiran. Trenutno stanje: {racun.StanjeNaRacunu}");
        }

        public class AmountRequest
        {
            public int UcesnikID { get; set; }
            public double Iznos { get; set; }
        }

        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] AmountRequest request)
        {
            var racun = await _repository.LoadAsync<RacunUcesnikaAggregate>(request.UcesnikID);
            if (racun == null) return NotFound("Racun nije pronadjen.");

            try
            {
                racun.Deposit(request.Iznos);
                await _repository.SaveAsync(racun);
                return Ok($"Uplata uspesna. Novo stanje: {racun.StanjeNaRacunu}");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("withdraw")]
        public async Task<IActionResult> Withdraw([FromBody] AmountRequest request)
        {
            var racun = await _repository.LoadAsync<RacunUcesnikaAggregate>(request.UcesnikID);
            if (racun == null) return NotFound("Racun nije pronadjen.");

            try
            {
                racun.Withdraw(request.Iznos);
                await _repository.SaveAsync(racun);
                return Ok($"Isplata uspesna. Novo stanje: {racun.StanjeNaRacunu}");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public class BlockRequest
        {
            public int UcesnikID { get; set; }
            public string Razlog { get; set; }
        }

        [HttpPost("block")]
        public async Task<IActionResult> Block([FromBody] BlockRequest request)
        {
            var racun = await _repository.LoadAsync<RacunUcesnikaAggregate>(request.UcesnikID);
            if (racun == null) return NotFound("Racun nije pronadjen.");

            racun.Block(request.Razlog);
            await _repository.SaveAsync(racun);
            return Ok("Racun je uspesno blokiran.");
        }

        [HttpPost("unblock/{id}")]
        public async Task<IActionResult> Unblock(int id)
        {
            var racun = await _repository.LoadAsync<RacunUcesnikaAggregate>(id);
            if (racun == null) return NotFound("Racun nije pronadjen.");

            racun.Unblock();
            await _repository.SaveAsync(racun);
            return Ok("Racun je uspesno odblokiran.");
        }

        [HttpGet("state/{id}")]
        public async Task<IActionResult> GetState(int id)
        {
            var racun = await _repository.LoadAsync<RacunUcesnikaAggregate>(id);
            if (racun == null) return NotFound("Racun nije pronadjen.");

            return Ok(new
            {
                racun.ID,
                racun.Ime,
                racun.Prezime,
                racun.Email,
                racun.StanjeNaRacunu,
                racun.JeBlokiran,
                racun.Version
            });
        }

        [HttpGet("history/{id}")]
        public async Task<IActionResult> GetHistory(int id)
        {
            var history = await _repository.GetHistoryAsync(id, nameof(RacunUcesnikaAggregate));
            if (!history.Any()) return NotFound("Nema dogadjaja za ovaj racun.");

            return Ok(history.Select(e => new
            {
                EventType = e.GetType().Name,
                EventData = e,
                OccurredOn = e.OccurredOn
            }));
        }
    }
}
