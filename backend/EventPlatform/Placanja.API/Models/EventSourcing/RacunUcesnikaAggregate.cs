using System;
using Placanja.API.Models.EventSourcing.Events;

namespace Placanja.API.Models.EventSourcing
{
    public class RacunUcesnikaAggregate : AggregateRoot
    {
        public string Ime { get; private set; }
        public string Prezime { get; private set; }
        public string Email { get; private set; }
        public double StanjeNaRacunu { get; private set; }
        public bool JeBlokiran { get; private set; }

        public RacunUcesnikaAggregate()
        {
        }

        public static RacunUcesnikaAggregate Create(int id, string ime, string prezime, string email)
        {
            var racun = new RacunUcesnikaAggregate();

            var @event = new RacunKreiran
            {
                UcesnikID = id,
                Ime = ime,
                Prezime = prezime,
                Email = email
            };

            racun.RaiseEvent(@event);
            return racun;
        }

        public void Deposit(double iznos)
        {
            if (JeBlokiran) throw new InvalidOperationException("Racun je blokiran, uplate nisu moguce.");
            if (iznos <= 0) throw new ArgumentException("Iznos mora biti veci od nule.");

            var @event = new SredstvaUplacena { Iznos = iznos };
            RaiseEvent(@event);
        }

        public void Withdraw(double iznos)
        {
            if (JeBlokiran) throw new InvalidOperationException("Racun je blokiran, isplate nisu moguce.");
            if (iznos <= 0) throw new ArgumentException("Iznos mora biti veci od nule.");
            if (StanjeNaRacunu < iznos) throw new InvalidOperationException("Nedovoljno sredstava na racunu.");

            var @event = new SredstvaSkinuta { Iznos = iznos };
            RaiseEvent(@event);
        }

        public void Block(string razlog)
        {
            if (JeBlokiran) return;
            var @event = new RacunBlokiran { Razlog = razlog };
            RaiseEvent(@event);
        }

        public void Unblock()
        {
            if (!JeBlokiran) return;
            var @event = new RacunOdmrznut();
            RaiseEvent(@event);
        }

        public override AggregateSnapshot CreateSnapshot()
        {
            return new RacunUcesnikaSnapshot
            {
                ID = ID,
                Version = Version,
                Ime = Ime,
                Prezime = Prezime,
                Email = Email,
                StanjeNaRacunu = StanjeNaRacunu,
                JeBlokiran = JeBlokiran
            };
        }

        public override void RestoreSnapshot(AggregateSnapshot snapshot)
        {
            if (snapshot is not RacunUcesnikaSnapshot s)
                throw new InvalidOperationException($"Invalid snapshot type: {snapshot.GetType().Name}");

            ID = s.ID;
            Version = s.Version;
            Ime = s.Ime;
            Prezime = s.Prezime;
            Email = s.Email;
            StanjeNaRacunu = s.StanjeNaRacunu;
            JeBlokiran = s.JeBlokiran;
        }

        protected override void Apply(DomainEvent @event)
        {
            switch (@event)
            {
                case RacunKreiran e:
                    ID = e.UcesnikID;
                    Ime = e.Ime;
                    Prezime = e.Prezime;
                    Email = e.Email;
                    StanjeNaRacunu = 0;
                    JeBlokiran = false;
                    break;
                case SredstvaUplacena e:
                    StanjeNaRacunu += e.Iznos;
                    break;
                case SredstvaSkinuta e:
                    StanjeNaRacunu -= e.Iznos;
                    break;
                case RacunBlokiran e:
                    JeBlokiran = true;
                    break;
                case RacunOdmrznut e:
                    JeBlokiran = false;
                    break;
                default:
                    throw new InvalidOperationException($"Unknown event type: {@event.GetType().Name}");
            }
        }
    }
}
