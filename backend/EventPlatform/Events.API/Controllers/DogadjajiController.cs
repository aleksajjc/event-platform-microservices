using DTO.Lokacije;
using DTO.Predavaci;
using DTO.StrucniDogadjaji;
using DTO.TipoviDogadjaja;
using Events.API.CQRS.Commands;
using Events.API.CQRS.Queries;
using Events.API.CQRS.ReadModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Events.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DogadjajiController : ControllerBase
    {
        public DogadjajiController(IMediator mediator)
        {
            Mediator = mediator;
        }

        public IMediator Mediator { get; }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StrucniDogadjajDTO>>> Get(CancellationToken cancellationToken)
        {
            var dogadjaji = await Mediator.Send(new GetAllDogadjajQuery(), cancellationToken);
            var dto = dogadjaji.Select(MapToDto).ToList();
            return Ok(dto);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StrucniDogadjajDTO>> GetById(int id, CancellationToken cancellationToken)
        {
            var dogadjaj = await Mediator.Send(new GetDogadjajByIdQuery
            {
                StrucniDogadjajID = id
            }, cancellationToken);

            if (dogadjaj is null)
            {
                return NotFound();
            }

            return Ok(MapToDto(dogadjaj));
        }

        [HttpGet("by-lokacija/{lokacijaId}")]
        public async Task<ActionResult<IEnumerable<StrucniDogadjajDTO>>> GetByLokacija(int lokacijaId, CancellationToken cancellationToken)
        {
            var dogadjaji = await Mediator.Send(new GetDogadjajiByLokacijaQuery
            {
                LokacijaID = lokacijaId
            }, cancellationToken);

            var dto = dogadjaji.Select(MapToDto).ToList();
            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create([FromBody] StrucniDogadjajCreateDTO request, CancellationToken cancellationToken)
        {
            var command = new AddDogadjajCommand
            {
                Naziv = request.Naziv,
                Agenda = request.Agenda,
                DatumVremeOdrzavanja = request.DatumVremeOdrzavanja,
                Trajanje = request.Trajanje,
                CenaKotizacije = request.CenaKotizacije,
                LokacijaID = request.LokacijaID,
                PredavaciIDs = request.PredavaciIDs ?? new List<int>(),
                TipDogadjajaID = request.TipDogadjajaID
            };

            var result = await Mediator.Send(command, cancellationToken);
            if (!result.IsSuccess)
            {
                return result.NotFound ? NotFound(result) : BadRequest(result);
            }

            return Ok(result.EntityId);
        }

        [HttpPut]
        public async Task<ActionResult> Edit([FromBody] StrucniDogadjajCreateDTO request, CancellationToken cancellationToken)
        {
            var command = new EditDogadjajCommand
            {
                StrucniDogadjajID = request.StrucniDogadjajID,
                Naziv = request.Naziv,
                Agenda = request.Agenda,
                DatumVremeOdrzavanja = request.DatumVremeOdrzavanja,
                Trajanje = request.Trajanje,
                CenaKotizacije = request.CenaKotizacije,
                LokacijaID = request.LokacijaID,
                PredavaciIDs = request.PredavaciIDs ?? new List<int>(),
                TipDogadjajaID = request.TipDogadjajaID
            };

            var result = await Mediator.Send(command, cancellationToken);
            if (!result.IsSuccess)
            {
                return result.NotFound ? NotFound(result) : BadRequest(result);
            }

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new DeleteDogadjajCommand
            {
                StrucniDogadjajID = id
            }, cancellationToken);

            if (!result.IsSuccess)
            {
                return result.NotFound ? NotFound(result) : BadRequest(result);
            }

            return Ok();
        }

        private static StrucniDogadjajDTO MapToDto(DogadjajReadModel dogadjaj)
        {
            var lokacija = dogadjaj.Lokacija ?? new LokacijaReadModel();
            var tipDogadjaja = dogadjaj.TipDogadjaja ?? new TipDogadjajaReadModel();

            return new StrucniDogadjajDTO
            {
                StrucniDogadjajID = dogadjaj.StrucniDogadjajID,
                Naziv = dogadjaj.Naziv,
                Agenda = dogadjaj.Agenda,
                DatumVremeOdrzavanja = dogadjaj.DatumVremeOdrzavanja,
                Trajanje = dogadjaj.Trajanje,
                CenaKotizacije = dogadjaj.CenaKotizacije,
                Lokacija = new LokacijaDTO
                {
                    LokacijaID = lokacija.LokacijaID,
                    Naziv = lokacija.Naziv,
                    Adresa = lokacija.Adresa,
                    Kapacitet = lokacija.Kapacitet
                },
                Predavaci = dogadjaj.Predavaci.Select(p => new PredavacDTO
                {
                    PredavacID = p.PredavacID,
                    Ime = p.Ime,
                    Prezime = p.Prezime,
                    Titula = p.Titula,
                    OblastStrucnosti = p.OblastStrucnosti
                }).ToList(),
                TipDogadjaja = new TipDogadjajaDTO
                {
                    TipDogadjajaID = tipDogadjaja.TipDogadjajaID,
                    NazivTipa = tipDogadjaja.NazivTipa
                }
            };
        }
    }
}
