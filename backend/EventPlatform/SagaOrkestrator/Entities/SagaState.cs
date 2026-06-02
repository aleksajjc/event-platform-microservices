using System;

namespace SagaOrkestrator.Entities
{
    public enum SagaStatus
    {
        Started,
        CapacityReserved,
        Completed,
        Compensating,
        Failed
    }

    public class SagaState
    {
        public int ID { get; set; }
        public Guid CorrelationID { get; set; }
        public int StrucniDogadjajID { get; set; }
        public int UcesnikID { get; set; }
        public double CenaKotizacije { get; set; }
        public SagaStatus Status { get; set; }
        public string TrenutniKorak { get; set; } = string.Empty;
        public string? Greska { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
