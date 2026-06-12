using Events.API.CQRS.Queries;
using Events.API.CQRS.ReadModels;
using Events.API.CQRS.Repositories;
using MediatR;

namespace Events.API.CQRS.Handlers
{
    public class GetDogadjajiByLokacijaQueryHandler : IRequestHandler<GetDogadjajiByLokacijaQuery, List<DogadjajReadModel>>
    {
        public GetDogadjajiByLokacijaQueryHandler(IDogadjajReadRepository dogadjajReadRepository)
        {
            DogadjajReadRepository = dogadjajReadRepository;
        }

        public IDogadjajReadRepository DogadjajReadRepository { get; }

        public async Task<List<DogadjajReadModel>> Handle(GetDogadjajiByLokacijaQuery query, CancellationToken cancellationToken)
        {
            return await DogadjajReadRepository.GetDogadjajiByLokacijaAsync(query.LokacijaID, cancellationToken);
        }
    }
}
