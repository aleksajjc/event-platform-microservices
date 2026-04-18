using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prijave.API.Data;

namespace Prijave.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DogadjajReferencesController : ControllerBase
    {
        private readonly PrijavaContext _context;
        public DogadjajReferencesController(PrijavaContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetDogadjajReferenceDTO>>> Get()
        {
            var referenceDTOs = await _context.DogadjajReferences
                .Select(dr => new GetDogadjajReferenceDTO
                {
                    StrucniDogadjajID = dr.DogadjajReferenceID,
                    Naziv = dr.Naziv
                })
                .ToListAsync();
            return Ok(referenceDTOs);
        }
    }
    public class GetDogadjajReferenceDTO
    {
        public int StrucniDogadjajID { get; set; }
        public string Naziv { get; set; } = string.Empty;
    }
}

