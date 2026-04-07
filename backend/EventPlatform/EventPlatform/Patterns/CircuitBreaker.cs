namespace EventPlatform.Patterns
{
    public enum CircuitBreakerState { 
        Closed, 
        Open, 
        HalfOpen 
    }
    public class CircuitBreaker
    {
        private object _lock = new object();
        private readonly int _failureThreshold; 
        private readonly TimeSpan _openDuration; 
        private DateTime _lastFailureTime = DateTime.MinValue;
        private int _failureCount;
        private CircuitBreakerState _state = CircuitBreakerState.Closed;

        public CircuitBreaker(int failureThreshold, TimeSpan openDuration)
        {
            _failureThreshold = failureThreshold;
            _openDuration = openDuration;
        }

        public CircuitBreakerState State
        {
            get
            {
                lock (_lock)
                {
                    if (_state == CircuitBreakerState.Open && (DateTime.UtcNow - _lastFailureTime) > _openDuration)
                    {
                        _state = CircuitBreakerState.HalfOpen;
                    }
                }
                return _state;
            }
        }

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
        {
            if(State == CircuitBreakerState.Open)
            {
                throw new Exception("CircuitBreaker je otvoren - nije dostupan");
            }

            try
            {
                var rezultat = await action();

                lock (_lock)
                {
                    _failureCount = 0;
                    _state = CircuitBreakerState.Closed;
                }
                return rezultat;


            }
            catch (Exception)
            {
                lock (_lock)
                {
                    _failureCount++;
                    _lastFailureTime = DateTime.UtcNow;
                }
                if(_state == CircuitBreakerState.HalfOpen || _failureCount > _failureThreshold)
                {
                    _state = CircuitBreakerState.Open;
                }
                throw;
            }
        }
    }
}
