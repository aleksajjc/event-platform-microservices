using Events.API.CQRS.Commands;
using Events.API.CQRS.Common;
using Events.API.CQRS.Repositories;
using Events.API.CQRS.Validation;
using Events.API.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Events.API.CQRS.Handlers
{
    public class AddDogadjajCommandHandler : IRequestHandler<AddDogadjajCommand, OperationResult>
    {
        public AddDogadjajCommandHandler(EventContext eventContext, IDogadjajWriteRepository dogadjajWriteRepository)
        {
            EventContext = eventContext;
            DogadjajWriteRepository = dogadjajWriteRepository;
        }

        public EventContext EventContext { get; }
        public IDogadjajWriteRepository DogadjajWriteRepository { get; }

        public async Task<OperationResult> Handle(AddDogadjajCommand command, CancellationToken cancellationToken)
        {
            var errors = DogadjajCommandValidator.Validate(command);
            if (errors.Count > 0)
            {
                return OperationResult.Failure("Validation failed.", errors.ToArray());
            }

            
            var lokacija = await EventContext.Lokacije
                .FirstOrDefaultAsync(l => l.LokacijaID == command.LokacijaID, cancellationToken);

            if (lokacija is null)
            {
                return OperationResult.NotFoundResult($"Lokacija {command.LokacijaID} ne postoji.");
            }

            var tipDogadjajaExists = await EventContext.TipoviDogadjaja
                .AnyAsync(t => t.TipDogadjajaID == command.TipDogadjajaID, cancellationToken);

            if (!tipDogadjajaExists)
            {
                return OperationResult.NotFoundResult($"Tip događaja {command.TipDogadjajaID} ne postoji.");
            }

            var distinctPredavaci = command.PredavaciIDs.Distinct().ToList();
            var validPredavaciCount = await EventContext.Predavaci
                .CountAsync(p => distinctPredavaci.Contains(p.PredavacID), cancellationToken);

            if (validPredavaciCount != distinctPredavaci.Count)
            {
                return OperationResult.NotFoundResult("Jedan ili više predavača ne postoji.");
            }

            try
            {
                var id = await DogadjajWriteRepository.AddDogadjajAsync(command, lokacija.Kapacitet, cancellationToken);
                return OperationResult.Success(id, "Dogadjaj created.");
            }
            catch (Exception ex)
            {
                return OperationResult.Failure($"Database error: {ex.Message}");
            }
        }
    }
}
