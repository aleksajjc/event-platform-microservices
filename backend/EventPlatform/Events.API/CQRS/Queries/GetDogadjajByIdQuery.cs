using Events.API.CQRS.ReadModels;
using MediatR;

namespace Events.API.CQRS.Queries
{
    public class GetDogadjajByIdQuery : IRequest<DogadjajReadModel?>
    {
        public int StrucniDogadjajID { get; set; }
    }
}
