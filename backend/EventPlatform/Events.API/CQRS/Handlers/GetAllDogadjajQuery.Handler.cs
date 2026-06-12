using Events.API.CQRS.Queries;
using Events.API.CQRS.ReadModels;
using Events.API.CQRS.Repositories;
using MediatR;

namespace Events.API.CQRS.Handlers
{
    public class GetAllDogadjajQueryHandler : IRequestHandler<GetAllDogadjajQuery, List<DogadjajReadModel>>
    {
        public GetAllDogadjajQueryHandler(IDogadjajReadRepository dogadjajReadRepository)
        {
            DogadjajReadRepository = dogadjajReadRepository;
        }

        public IDogadjajReadRepository DogadjajReadRepository { get; }

        public async Task<List<DogadjajReadModel>> Handle(GetAllDogadjajQuery query, CancellationToken cancellationToken)
        {
            var dogadjaji = await DogadjajReadRepository.GetAllDogadjajiAsync(cancellationToken);
            return dogadjaji;
        }
    }
}
