using Events.API.CQRS.Common;
using MediatR;

namespace Events.API.CQRS.Commands
{
    public class AddDogadjajCommand : IRequest<OperationResult>
    {
        public string Naziv { get; set; } = string.Empty;
        public string Agenda { get; set; } = string.Empty;
        public DateTime DatumVremeOdrzavanja { get; set; }
        public double Trajanje { get; set; }
        public double CenaKotizacije { get; set; }
        public int LokacijaID { get; set; }
        public List<int> PredavaciIDs { get; set; } = new();
        public int TipDogadjajaID { get; set; }
    }
}
