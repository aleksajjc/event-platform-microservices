namespace Events.API.CQRS.Common
{
    // rezultat za komande: kaže da li je operacija uspela,
    // da li je nešto pronađeno i, po potrebi, koji je ID novog reda.
    public sealed record OperationResult
    {
        public bool IsSuccess { get; init; }
        public bool NotFound { get; init; }
        public int? EntityId { get; init; }
        public string Message { get; init; } = string.Empty;
        public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

        public static OperationResult Success(int? entityId = null, string message = "Success")
            => new()
            {
                IsSuccess = true,
                EntityId = entityId,
                Message = message
            };

        public static OperationResult Failure(string message, params string[] errors)
            => new()
            {
                Message = message,
                Errors = errors
            };

        public static OperationResult NotFoundResult(string message)
            => new()
            {
                NotFound = true,
                Message = message
            };
    }
}
