using Events.API.CQRS.Queries;
using Events.API.CQRS.ReadModels;
using Events.API.CQRS.Repositories;
using MediatR;

namespace Events.API.CQRS.Handlers
{
    public class GetDogadjajByIdQueryHandler : IRequestHandler<GetDogadjajByIdQuery, DogadjajReadModel?>
    {
        public GetDogadjajByIdQueryHandler(IDogadjajReadRepository dogadjajReadRepository)
        {
            DogadjajReadRepository = dogadjajReadRepository;
        }

        public IDogadjajReadRepository DogadjajReadRepository { get; }

        public async Task<DogadjajReadModel?> Handle(GetDogadjajByIdQuery query, CancellationToken cancellationToken)
        {
            return await DogadjajReadRepository.GetDogadjajByIdAsync(query.StrucniDogadjajID, cancellationToken);
        }
    }
}
