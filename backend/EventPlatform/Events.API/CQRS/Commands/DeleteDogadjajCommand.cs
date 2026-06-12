using Events.API.CQRS.Common;
using MediatR;

namespace Events.API.CQRS.Commands
{
    // Komanda za brisanje koristi samo identifikator reda.
    public class DeleteDogadjajCommand : IRequest<OperationResult>
    {
        public int StrucniDogadjajID { get; set; }
    }
}
