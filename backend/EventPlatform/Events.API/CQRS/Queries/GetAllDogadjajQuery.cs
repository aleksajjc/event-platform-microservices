using Events.API.CQRS.ReadModels;
using MediatR;

namespace Events.API.CQRS.Queries
{
    // Query je samo marker: njime tražimo listu događaja.
    public class GetAllDogadjajQuery : IRequest<List<DogadjajReadModel>>
    {
    }
}
