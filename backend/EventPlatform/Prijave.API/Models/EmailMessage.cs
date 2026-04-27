namespace Prijave.API.Models
{
    public sealed record EmailMessage(
        string Email,
        string Naslov,
        string TekstPoruke
        );
}
