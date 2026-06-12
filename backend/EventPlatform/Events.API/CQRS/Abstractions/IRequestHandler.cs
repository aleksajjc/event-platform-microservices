namespace Events.API.CQRS.Abstractions
{
    // Ručna verzija handler ugovora, istog oblika kao u sample projektu,
    // samo što ovde ne postoji mediator koji automatski dispatch-uje zahteve.
    public interface IRequestHandler<in TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
    }
}
