## Microservices System
A platform based on **Microservices Architecture** intended for the organization of events such as seminars, workshops, lectures and the like. It is built in the .NET environment, with a focus on **advanced distributed patterns and high resilience to network crashes.**
## System Architecture
The system consists of a main MVC application and two independent microservices:
1. **Events.API (Microservice)** - Responsible for managing professional events, locations, event types, and speakers.
2. **Prijave.API (Microservice)** - Responsible for registering participants and their applications to available events.
3. **EventPlatform (MVC)** - A web application that serves as the user interface for interacting with the system.
### Inter-service Communication
* **Synchronous Communication:** Implemented via the HTTP protocol for communication between the MVC client and the microservices.
* **Asynchronous Communication:** Implemented using the **RabbitMQ** message broker for interaction between the microservices themselves.
### Network Fault Tolerance Mechanisms (Resiliency)
* **Retry Pattern:** Using the *Polly* library, the system automatically retries failed HTTP requests (configured for the event list view).
* **Timeout:** Configured globally at the HttpClient level to cut off excessively long waits for a response (max 10 seconds).
* **Circuit Breaker:** Implemented to suspend further calls to a service in the event of frequent communication failures with that service.
### Distributed Transactions and Messaging Patterns
* **Outbox Pattern:** Implemented in Events.API. Before publishing a message to RabbitMQ, it is saved to the database along with the domain within the same transaction, ensuring consistency (At-Least-Once delivery).
* **Dead Letter Queue (DLX/DLQ):** Configured in Prijave.API to safely route messages that the system failed to process.
* **Request-Reply Pattern:** Implemented for two-way asynchronous communication.
### Advanced Asynchronous Tasks
* **Email Rate Limiter:** A dedicated background Consumer in the Prijave.API service responsible for sending confirmation emails.
## Technologies
* **ASP.NET Core Web API & MVC**
* **Entity Framework Core**
* **HTML / CSS / JavaScript (Bootstrap)**
* **SQL Server**
* **RabbitMQ**