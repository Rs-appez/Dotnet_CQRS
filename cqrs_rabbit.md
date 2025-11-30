# User (5d126770-4135-4392-a87e-57e7652eb395) ───

how can i setup a dotnet cqrs project with clean architecture, using postgresql as write database and mongodb as read database, with rabbitmq for messaging?

# Copilot (43f5c8e2-55f5-435d-9bf8-bfe804521145) ───

Below is a practical, step‑by‑step blueprint for a .NET CQRS + Clean Architecture solution using:

- PostgreSQL → write model
- MongoDB → read model
- RabbitMQ → messaging / eventual consistency

This assumes .NET 8, Linux, and `dotnet` CLI.

---

## 1. Solution & Project Structure

From your project root:

```bash
mkdir CqrsSample
cd CqrsSample
dotnet new sln -n CqrsSample

# Core layers
dotnet new classlib -n CqrsSample.Domain
dotnet new classlib -n CqrsSample.Application
dotnet new classlib -n CqrsSample.Infrastructure
dotnet new webapi   -n CqrsSample.Api

dotnet sln add CqrsSample.Domain CqrsSample.Application CqrsSample.Infrastructure CqrsSample.Api
```

Add references (Clean Architecture style):

```bash
dotnet add CqrsSample.Application reference CqrsSample.Domain
dotnet add CqrsSample.Infrastructure reference CqrsSample.Application CqrsSample.Domain
dotnet add CqrsSample.Api reference CqrsSample.Application CqrsSample.Infrastructure
```

Projects purpose:

- `Domain` – Entities, value objects, domain events, aggregates, interfaces that are domain‑centric.
- `Application` – CQRS handlers (Commands/Queries), DTOs, use cases, interfaces for persistence/messaging.
- `Infrastructure` – EF Core (Postgres) implementation, MongoDB implementation, RabbitMQ implementation.
- `Api` – Controllers / minimal APIs, DI wiring, request/response mapping.

---

## 2. Install NuGet Packages

From each project directory:

### Application

```bash
cd CqrsSample.Application
dotnet add package MediatR
dotnet add package FluentValidation
cd ..
```

### Infrastructure

```bash
cd CqrsSample.Infrastructure
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package MongoDB.Driver
dotnet add package RabbitMQ.Client
dotnet add package MediatR.Extensions.Microsoft.DependencyInjection
cd ..
```

### Api

```bash
cd CqrsSample.Api
dotnet add package Swashbuckle.AspNetCore
cd ..
```

(You can also use `Carter`, `MinimalApis`, etc., if you prefer.)

---

## 3. Domain Layer (Write model)

Example domain: simple `Order` aggregate.

`CqrsSample.Domain/Entities/Order.cs`:

```csharp
namespace CqrsSample.Domain.Entities;

public class Order
{
    public Guid Id { get; private set; }
    public string CustomerName { get; private set; } = default!;
    public decimal Total { get; private set; }

    // Example domain event
    public List<IDomainEvent> DomainEvents { get; } = new();

    private Order() { }

    public Order(string customerName, decimal total)
    {
        Id = Guid.NewGuid();
        CustomerName = customerName;
        Total = total;

        DomainEvents.Add(new OrderCreatedDomainEvent(Id, CustomerName, Total));
    }
}
```

`CqrsSample.Domain/Events/IDomainEvent.cs`:

```csharp
namespace CqrsSample.Domain.Events;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
```

`CqrsSample.Domain/Events/OrderCreatedDomainEvent.cs`:

```csharp
namespace CqrsSample.Domain.Events;

public sealed class OrderCreatedDomainEvent : IDomainEvent
{
    public Guid OrderId { get; }
    public string CustomerName { get; }
    public decimal Total { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public OrderCreatedDomainEvent(Guid orderId, string customerName, decimal total)
    {
        OrderId = orderId;
        CustomerName = customerName;
        Total = total;
    }
}
```

---

## 4. Application Layer – CQRS Contracts

### Interfaces

`CqrsSample.Application/Common/Interfaces/IWriteDbContext.cs`:

```csharp
using CqrsSample.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CqrsSample.Application.Common.Interfaces;

public interface IWriteDbContext
{
    DbSet<Order> Orders { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

`CqrsSample.Application/Common/Interfaces/IMessageBus.cs`:

```csharp
namespace CqrsSample.Application.Common.Interfaces;

public interface IMessageBus
{
    Task PublishAsync<T>(T message, string routingKey, CancellationToken ct = default);
}
```

`CqrsSample.Application/Common/Interfaces/IReadOrderRepository.cs`:

```csharp
using CqrsSample.Application.Orders.Queries;

namespace CqrsSample.Application.Common.Interfaces;

public interface IReadOrderRepository
{
    Task<OrderReadModel?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<OrderReadModel>> GetAllAsync(CancellationToken ct = default);
}
```

### Commands (Write side)

`CqrsSample.Application/Orders/Commands/CreateOrder/CreateOrderCommand.cs`:

```csharp
using MediatR;

namespace CqrsSample.Application.Orders.Commands.CreateOrder;

public sealed record CreateOrderCommand(
    string CustomerName,
    decimal Total
) : IRequest<Guid>;
```

`CqrsSample.Application/Orders/Commands/CreateOrder/CreateOrderCommandHandler.cs`:

```csharp
using CqrsSample.Application.Common.Interfaces;
using CqrsSample.Domain.Entities;
using MediatR;

namespace CqrsSample.Application.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly IWriteDbContext _context;

    public CreateOrderCommandHandler(IWriteDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        var order = new Order(request.CustomerName, request.Total);
        _context.Orders.Add(order);

        await _context.SaveChangesAsync(ct);

        // Domain event will be picked up in Infrastructure to publish to RabbitMQ
        return order.Id;
    }
}
```

### Queries (Read side)

Define a read model DTO; this is what MongoDB stores:

`CqrsSample.Application/Orders/Queries/OrderReadModel.cs`:

```csharp
namespace CqrsSample.Application.Orders.Queries;

public sealed class OrderReadModel
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = default!;
    public decimal Total { get; set; }
}
```

Query definitions:

```csharp
// GetOrderByIdQuery.cs
using MediatR;

namespace CqrsSample.Application.Orders.Queries;

public sealed record GetOrderByIdQuery(Guid Id) : IRequest<OrderReadModel?>;
```

Query handler using Mongo repo:

```csharp
// GetOrderByIdQueryHandler.cs
using CqrsSample.Application.Common.Interfaces;
using MediatR;

namespace CqrsSample.Application.Orders.Queries;

public sealed class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderReadModel?>
{
    private readonly IReadOrderRepository _repository;

    public GetOrderByIdQueryHandler(IReadOrderRepository repository)
    {
        _repository = repository;
    }

    public Task<OrderReadModel?> Handle(GetOrderByIdQuery request, CancellationToken ct)
        => _repository.GetByIdAsync(request.Id, ct);
}
```

---

## 5. Infrastructure – PostgreSQL (Write)

`CqrsSample.Infrastructure/Persistence/WriteDbContext.cs`:

```csharp
using CqrsSample.Application.Common.Interfaces;
using CqrsSample.Domain.Entities;
using CqrsSample.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace CqrsSample.Infrastructure.Persistence;

public sealed class WriteDbContext : DbContext, IWriteDbContext
{
    public WriteDbContext(DbContextOptions<WriteDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(b =>
        {
            b.HasKey(o => o.Id);
            b.Property(o => o.CustomerName).IsRequired().HasMaxLength(200);
            b.Property(o => o.Total).HasColumnType("numeric(18,2)");
            b.Ignore(o => o.DomainEvents);
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = ChangeTracker.Entries()
            .SelectMany(e => (e.Entity as dynamic)?.DomainEvents ?? new List<IDomainEvent>())
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        // Raise domain events via an injected dispatcher, or leave them here to be
        // processed by an outbox pattern. For now, keep it simple:
        // (You will wire a separate service to publish them to RabbitMQ.)

        return result;
    }
}
```

Registration in Infrastructure:

`CqrsSample.Infrastructure/DependencyInjection.cs`:

```csharp
using CqrsSample.Application.Common.Interfaces;
using CqrsSample.Infrastructure.Messaging;
using CqrsSample.Infrastructure.Persistence;
using CqrsSample.Infrastructure.ReadModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CqrsSample.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // PostgreSQL
        services.AddDbContext<WriteDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("WriteDatabase"));
        });

        services.AddScoped<IWriteDbContext>(sp => sp.GetRequiredService<WriteDbContext>());

        // Mongo read model repository
        services.AddSingleton<IReadOrderRepository, MongoOrderReadRepository>();

        // RabbitMQ message bus
        services.AddSingleton<IMessageBus, RabbitMqMessageBus>();
        services.AddHostedService<OrderCreatedConsumer>(); // background consumer

        return services;
    }
}
```

---

## 6. Infrastructure – MongoDB (Read)

`CqrsSample.Infrastructure/ReadModel/MongoOrderReadRepository.cs`:

```csharp
using CqrsSample.Application.Common.Interfaces;
using CqrsSample.Application.Orders.Queries;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace CqrsSample.Infrastructure.ReadModel;

public sealed class MongoOrderReadRepository : IReadOrderRepository
{
    private readonly IMongoCollection<OrderReadModel> _collection;

    public MongoOrderReadRepository(IConfiguration config)
    {
        var connectionString = config.GetConnectionString("ReadDatabase");
        var client = new MongoClient(connectionString);
        var db = client.GetDatabase("cqrs_read");
        _collection = db.GetCollection<OrderReadModel>("orders");
    }

    public Task<OrderReadModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _collection.Find(x => x.Id == id).FirstOrDefaultAsync(ct)!;

    public Task<List<OrderReadModel>> GetAllAsync(CancellationToken ct = default)
        => _collection.Find(FilterDefinition<OrderReadModel>.Empty).ToListAsync(ct);
}
```

You will also have a separate consumer that writes to this collection (see RabbitMQ consumer below).

---

## 7. Infrastructure – RabbitMQ Messaging

### Publisher (used for domain events outbox or directly from service)

`CqrsSample.Infrastructure/Messaging/RabbitMqMessageBus.cs`:

```csharp
using System.Text;
using System.Text.Json;
using CqrsSample.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace CqrsSample.Infrastructure.Messaging;

public sealed class RabbitMqMessageBus : IMessageBus, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _exchange = "cqrs.events";

    public RabbitMqMessageBus(IConfiguration configuration)
    {
        var factory = new ConnectionFactory
        {
            Uri = new Uri(configuration.GetConnectionString("RabbitMq")!)
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(exchange: _exchange, type: ExchangeType.Topic, durable: true);
    }

    public Task PublishAsync<T>(T message, string routingKey, CancellationToken ct = default)
    {
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        _channel.BasicPublish(
            exchange: _exchange,
            routingKey: routingKey,
            basicProperties: null,
            body: body);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
```

### Background Consumer – update Mongo read model

`CqrsSample.Infrastructure/Messaging/OrderCreatedConsumer.cs`:

```csharp
using System.Text;
using System.Text.Json;
using CqrsSample.Application.Orders.Queries;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CqrsSample.Infrastructure.Messaging;

public sealed class OrderCreatedConsumer : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly IMongoCollection<OrderReadModel> _orders;

    private const string Exchange = "cqrs.events";
    private const string Queue = "cqrs.orders.created";
    private const string RoutingKey = "order.created";

    public OrderCreatedConsumer(IConfiguration config)
    {
        var factory = new ConnectionFactory
        {
            Uri = new Uri(config.GetConnectionString("RabbitMq")!)
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(Exchange, ExchangeType.Topic, durable: true);
        _channel.QueueDeclare(Queue, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(Queue, Exchange, RoutingKey);

        var mongoClient = new MongoClient(config.GetConnectionString("ReadDatabase"));
        var db = mongoClient.GetDatabase("cqrs_read");
        _orders = db.GetCollection<OrderReadModel>("orders");
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (_, ea) =>
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            var model = JsonSerializer.Deserialize<OrderReadModel>(json);

            if (model != null)
            {
                await _orders.ReplaceOneAsync(
                    x => x.Id == model.Id,
                    model,
                    new ReplaceOptions { IsUpsert = true },
                    stoppingToken);
            }

            _channel.BasicAck(ea.DeliveryTag, multiple: false);
        };

        _channel.BasicConsume(Queue, autoAck: false, consumer: consumer);
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
```

### Where to publish `order.created`

Use an outbox table in Postgres for reliability ideally, but to keep it simple:

- In `WriteDbContext.SaveChangesAsync` you can collect `OrderCreatedDomainEvent` and inject `IMessageBus` in a domain-event-dispatcher service.
- Or call `IMessageBus` in the command handler after the transaction (simpler but less robust).

Example (simple, not fully robust):

```csharp
// In CreateOrderCommandHandler.Handle after SaveChangesAsync:
await _messageBus.PublishAsync(
    new OrderReadModel { Id = order.Id, CustomerName = order.CustomerName, Total = order.Total },
    routingKey: "order.created",
    ct);
```

(You’d inject `IMessageBus` in the handler constructor.)

---

## 8. API Layer – Wiring & Endpoints

In `CqrsSample.Api/Program.cs`:

```csharp
using CqrsSample.Application.Orders.Commands.CreateOrder;
using CqrsSample.Application.Orders.Queries;
using CqrsSample.Infrastructure;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(typeof(CreateOrderCommand));
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/orders", async (CreateOrderCommand command, IMediator mediator, CancellationToken ct) =>
{
    var id = await mediator.Send(command, ct);
    return Results.Created($"/orders/{id}", new { Id = id });
});

app.MapGet("/orders/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
{
    var result = await mediator.Send(new GetOrderByIdQuery(id), ct);
    return result is null ? Results.NotFound() : Results.Ok(result);
});

app.Run();
```

---

## 9. Configuration (Postgres, MongoDB, RabbitMQ)

`CqrsSample.Api/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "WriteDatabase": "Host=localhost;Port=5432;Database=cqrs_write;Username=postgres;Password=postgres",
    "ReadDatabase": "mongodb://localhost:27017",
    "RabbitMq": "amqp://guest:guest@localhost:5672/"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

Run dependencies with Docker (example):

```bash
docker run --name cqrs-postgres -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=cqrs_write -p 5432:5432 -d postgres:16
docker run --name cqrs-mongo -p 27017:27017 -d mongo:7
docker run --name cqrs-rabbit -p 5672:5672 -p 15672:15672 -d rabbitmq:3-management
```

Apply migrations for Postgres:

```bash
cd CqrsSample.Infrastructure
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL.Design
cd ..

dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate -p CqrsSample.Infrastructure -s CqrsSample.Api -o Persistence/Migrations
dotnet ef database update -p CqrsSample.Infrastructure -s CqrsSample.Api
```

---

## 10. Execution Flow Summary

1. `POST /orders`
   - API → MediatR → `CreateOrderCommandHandler`
   - Handler persists `Order` in Postgres (write DB).
   - After success, handler publishes `OrderReadModel` to RabbitMQ with routing key `order.created`.

2. `OrderCreatedConsumer`
   - Consumes `order.created` messages.
   - Upserts corresponding document in MongoDB (`cqrs_read.orders`).

3. `GET /orders/{id}`
   - API → MediatR → `GetOrderByIdQueryHandler`
   - Handler queries MongoDB via `IReadOrderRepository`.
   - Returns denormalized read model.

---

If you want, I can next:

- Add an outbox pattern for reliable RabbitMQ publishing.
- Show full domain-event dispatching instead of publishing in the handler.
