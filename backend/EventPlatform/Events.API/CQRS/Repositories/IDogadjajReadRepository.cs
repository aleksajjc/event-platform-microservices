using Events.API.CQRS.ReadModels;

namespace Events.API.CQRS.Repositories
{
    public interface IDogadjajReadRepository
    {
        Task<List<DogadjajReadModel>> GetAllDogadjajiAsync(CancellationToken cancellationToken);
        Task<DogadjajReadModel?> GetDogadjajByIdAsync(int strucniDogadjajID, CancellationToken cancellationToken);
        Task<List<DogadjajReadModel>> GetDogadjajiByLokacijaAsync(int lokacijaID, CancellationToken cancellationToken);
    }
}
