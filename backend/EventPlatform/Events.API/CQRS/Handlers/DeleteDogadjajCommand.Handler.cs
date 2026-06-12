using Events.API.CQRS.Commands;
using Events.API.CQRS.Common;
using Events.API.CQRS.Repositories;
using Events.API.CQRS.Validation;
using Events.API.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Events.API.CQRS.Handlers
{
    public class DeleteDogadjajCommandHandler : IRequestHandler<DeleteDogadjajCommand, OperationResult>
    {
        public DeleteDogadjajCommandHandler(EventContext eventContext, IDogadjajWriteRepository dogadjajWriteRepository)
        {
            EventContext = eventContext;
            DogadjajWriteRepository = dogadjajWriteRepository;
        }

        public EventContext EventContext { get; }
        public IDogadjajWriteRepository DogadjajWriteRepository { get; }

        public async Task<OperationResult> Handle(DeleteDogadjajCommand command, CancellationToken cancellationToken)
        {
            var errors = DogadjajCommandValidator.Validate(command);
            if (errors.Count > 0)
            {
                return OperationResult.Failure("Validation failed.", errors.ToArray());
            }

            var postojiDogadjaj = await EventContext.StrucniDogadjaji
                .AnyAsync(sd => sd.StrucniDogadjajID == command.StrucniDogadjajID, cancellationToken);

            if (!postojiDogadjaj)
            {
                return OperationResult.NotFoundResult($"Dogadjaj {command.StrucniDogadjajID} ne postoji.");
            }

            try
            {
                var deleted = await DogadjajWriteRepository.DeleteDogadjajAsync(command.StrucniDogadjajID, cancellationToken);
                if (!deleted)
                {
                    return OperationResult.NotFoundResult("Dogadjaj nije pronađen za brisanje.");
                }

                return OperationResult.Success(command.StrucniDogadjajID, "Dogadjaj deleted.");
            }
            catch (Exception ex)
            {
                return OperationResult.Failure($"Database error: {ex.Message}");
            }
        }
    }
}
