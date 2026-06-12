using Events.API.CQRS.ReadModels;
using MediatR;

namespace Events.API.CQRS.Queries
{
    // Treći query za filtriranje podataka.
    public class GetDogadjajiByLokacijaQuery : IRequest<List<DogadjajReadModel>>
    {
        public int LokacijaID { get; set; }
    }
}
