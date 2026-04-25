namespace Prijave.API.Models
{
    internal sealed record LokacijaInfo(string Naziv, string Adresa);
    internal sealed record DogadjajDetaljiResponse(
        int DogadjajId,
        string Naziv,
        string Agenda,
        DateTime DatumOdrzavanja,
        LokacijaInfo Lokacija
        );
}
