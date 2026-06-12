using Events.API.CQRS.Commands;

namespace Events.API.CQRS.Repositories
{
    public interface IDogadjajWriteRepository
    {
        Task<int> AddDogadjajAsync(AddDogadjajCommand command, int maksimalanKapacitet, CancellationToken cancellationToken);
        Task<bool> EditDogadjajAsync(EditDogadjajCommand command, int maksimalanKapacitet, CancellationToken cancellationToken);
        Task<bool> DeleteDogadjajAsync(int strucniDogadjajID, CancellationToken cancellationToken);
    }
}
