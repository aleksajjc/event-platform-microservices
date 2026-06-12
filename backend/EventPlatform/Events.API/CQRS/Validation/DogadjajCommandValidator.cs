using Events.API.CQRS.Commands;

namespace Events.API.CQRS.Validation
{
    
    public static class DogadjajCommandValidator
    {
        public static List<string> Validate(AddDogadjajCommand command)
        {
            var errors = new List<string>();
            ValidateCommon(command.Naziv, command.Agenda, command.DatumVremeOdrzavanja, command.Trajanje, command.CenaKotizacije, command.LokacijaID, command.TipDogadjajaID, command.PredavaciIDs, errors);
            return errors;
        }

        public static List<string> Validate(EditDogadjajCommand command)
        {
            var errors = new List<string>();

            if (command.StrucniDogadjajID <= 0)
            {
                errors.Add("StrucniDogadjajID mora biti veći od nule.");
            }

            ValidateCommon(command.Naziv, command.Agenda, command.DatumVremeOdrzavanja, command.Trajanje, command.CenaKotizacije, command.LokacijaID, command.TipDogadjajaID, command.PredavaciIDs, errors);
            return errors;
        }

        public static List<string> Validate(DeleteDogadjajCommand command)
        {
            var errors = new List<string>();

            if (command.StrucniDogadjajID <= 0)
            {
                errors.Add("StrucniDogadjajID mora biti veći od nule.");
            }

            return errors;
        }

        private static void ValidateCommon(
            string naziv,
            string agenda,
            DateTime datumVremeOdrzavanja,
            double trajanje,
            double cenaKotizacije,
            int lokacijaID,
            int tipDogadjajaID,
            List<int> predavaciIDs,
            List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(naziv))
            {
                errors.Add("Naziv je obavezan.");
            }

            if (string.IsNullOrWhiteSpace(agenda))
            {
                errors.Add("Agenda je obavezna.");
            }

            if (datumVremeOdrzavanja == default)
            {
                errors.Add("Datum i vreme održavanja su obavezni.");
            }

            if (trajanje <= 0)
            {
                errors.Add("Trajanje mora biti veće od nule.");
            }

            if (cenaKotizacije < 0)
            {
                errors.Add("Cena kotizacije ne može biti negativna.");
            }

            if (lokacijaID <= 0)
            {
                errors.Add("LokacijaID mora biti veći od nule.");
            }

            if (tipDogadjajaID <= 0)
            {
                errors.Add("TipDogadjajaID mora biti veći od nule.");
            }

            if (predavaciIDs is null || predavaciIDs.Count == 0)
            {
                errors.Add("Mora biti izabran bar jedan predavač.");
            }
            else if (predavaciIDs.Distinct().Count() != predavaciIDs.Count)
            {
                errors.Add("PredavaciIDs ne smeju imati duplikate.");
            }
        }
    }
}
