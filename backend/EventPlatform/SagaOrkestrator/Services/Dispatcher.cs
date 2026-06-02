using Microsoft.EntityFrameworkCore;
using SagaOrkestrator.Data;
using SagaOrkestrator.Entities;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace SagaOrkestrator.Services
{
    public static class Dispatcher
    {
        public static async Task DispatchOutboxMessages()
        {
            Console.WriteLine("[SAGA-DISPATCHER] Pokrenut pozadinski dispatcher...");
            while (true)
            {
                try
                {
                    using (var db = new SagaDbContext())
                    {
                        var outboxMessage = await db.SagaCommandOutboxMessages
                            .FirstOrDefaultAsync(x => x.Status == OutboxMessageStatus.ForProcessing);

                        if (outboxMessage != null)
                        {
                            Console.WriteLine($"[SAGA-DISPATCHER] Pronađena poruka za slanje u queue '{outboxMessage.QueueName}' za CorrelationId: {outboxMessage.CorrelationID}");

                            using (var bus = new RabbitMqBus())
                            {
                                await bus.Publish(outboxMessage.QueueName, outboxMessage.Payload);
                            }

                            outboxMessage.Status = OutboxMessageStatus.Processed;
                            db.SagaCommandOutboxMessages.Update(outboxMessage);

                            // Ažuriramo status Sage na osnovu queue-a na koji šaljemo
                            var sagaState = await db.SagaStates
                                .FirstOrDefaultAsync(x => x.CorrelationID == outboxMessage.CorrelationID);

                            if (sagaState != null)
                            {
                                if (outboxMessage.QueueName == "rezervisi-mesto")
                                {
                                    sagaState.Status = SagaStatus.Started;
                                    sagaState.TrenutniKorak = "Poslata komanda za rezervaciju mesta (rezervisi-mesto).";
                                }
                                else if (outboxMessage.QueueName == "naplati-kotizaciju")
                                {
                                    sagaState.Status = SagaStatus.CapacityReserved;
                                    sagaState.TrenutniKorak = "Poslata komanda za naplatu kotizacije (naplati-kotizaciju).";
                                }
                                else if (outboxMessage.QueueName == "potvrdi-prijavu")
                                {
                                    sagaState.Status = SagaStatus.Completed;
                                    sagaState.TrenutniKorak = "Poslata komanda za potvrdu prijave (potvrdi-prijavu).";
                                }
                                else if (outboxMessage.QueueName == "otkazi-prijavu")
                                {
                                    sagaState.Status = SagaStatus.Failed;
                                    sagaState.TrenutniKorak = "Poslata komanda za otkazivanje prijave (otkazi-prijavu).";
                                }
                                else if (outboxMessage.QueueName == "oslobodi-mesto")
                                {
                                    sagaState.Status = SagaStatus.Compensating;
                                    sagaState.TrenutniKorak = "Poslata kompenzacija za oslobađanje mesta (oslobodi-mesto).";
                                }
                                else if (outboxMessage.QueueName == "vrati-novac")
                                {
                                    sagaState.Status = SagaStatus.Compensating;
                                    sagaState.TrenutniKorak = "Poslata kompenzacija za povraćaj novca (vrati-novac).";
                                }

                                db.SagaStates.Update(sagaState);
                            }

                            await db.SaveChangesAsync();
                            Console.WriteLine($"[SAGA-DISPATCHER] Poruka uspešno poslata i označena kao procesirana.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SAGA-DISPATCHER ERROR] Greška u dispatcheru: {ex.Message}");
                }

                await Task.Delay(3000);
            }
        }
    }
}
