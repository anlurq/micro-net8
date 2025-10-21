using Microsoft.EntityFrameworkCore;
using ApiPedidos.Data;
using ApiPedidos.Models;
using BuildingBlocks.DTOs;                 // SolicitudProcesaPedidoDto, RegistroPagoDto
using BuildingBlocks.DTOs.Validators;     // SolicitudProcesaPedidoDtoValidator
using System.Net.Http.Json;
using MassTransit;
using BuildingBlocks.Contracts;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<PedidosDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Sql")));

// HttpClient para ApiPagos
builder.Services.AddHttpClient("ApiPagos", client =>
{
    var baseUrl = builder.Configuration["Apis:PagosBase"]
                  ?? throw new InvalidOperationException("Falta configurar Apis:PagosBase");
    client.BaseAddress = new Uri(baseUrl);   // <- IMPORTANTE
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var rabbitHost = builder.Configuration["RabbitMQ:Host"]!;
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(new Uri("amqp://guest:guest@localhost:5672"));
    });
});

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService(serviceName: "ApiPedidos")) // cambia a "ApiPagos" en ApiPagos
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddJaegerExporter(o =>
        {
            o.AgentHost = "localhost";
            o.AgentPort = 6831;
        })
    );

var app = builder.Build();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }

app.MapPost("/procesa", async (
    SolicitudProcesaPedidoDto dto,
    PedidosDbContext db,
    IHttpClientFactory httpFactory,
    IBus bus) =>
{
    // 1) Validar DTO con FluentValidation (definido en BuildingBlocks)
    var validation = new SolicitudProcesaPedidoDtoValidator().Validate(dto);
    if (!validation.IsValid)
    {
        var errors = validation.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
        return Results.ValidationProblem(errors);
    }

    // 2) Resolver/crear Cliente mínimo (para NombreCliente del evento y FK de Pedido)
    var cliente = await db.Clientes.FindAsync(dto.IdCliente);
    if (cliente is null)
    {
        cliente = new Cliente { IdCliente = dto.IdCliente, NombreCliente = $"Cliente {dto.IdCliente}" };
        db.Clientes.Add(cliente);
        await db.SaveChangesAsync();
    }

    // 3) Crear Pedido y guardar
    var pedido = new Pedido
    {
        IdCliente = dto.IdCliente,
        FechaPedido = DateTime.UtcNow,
        MontoPedido = dto.MontoPago,
        FormaPago = dto.FormaPago
    };
    db.Pedidos.Add(pedido);
    await db.SaveChangesAsync(); // Genera IdPedido

    // 4) Llamar ApiPagos /pago
    var client = httpFactory.CreateClient("ApiPagos");
    var pagoRequest = new RegistroPagoDto(dto.IdCliente, pedido.IdPedido, dto.MontoPago, dto.FormaPago);
    var resp = await client.PostAsJsonAsync("/pago", pagoRequest);
    if (!resp.IsSuccessStatusCode)
    {
        return Results.Problem("Fallo al registrar el pago en ApiPagos.", statusCode: 502);
    }
    var payload = await resp.Content.ReadFromJsonAsync<Dictionary<string, int>>() 
                 ?? new Dictionary<string, int>();
    payload.TryGetValue("idPago", out var idPago);

    // 5) (Paso 5 de la rúbrica) Publicar evento -> lo haremos en el Paso 6 con MassTransit
    await bus.Publish(new PagoProcesadoEvent(
        IdPedido: pedido.IdPedido,
        NombreCliente: cliente.NombreCliente,
        IdPago: idPago,
        MontoPago: dto.MontoPago,
        FormaPago: dto.FormaPago
    ));

    // 6) Respuesta mínima
    return Results.Accepted($"/procesa/{pedido.IdPedido}", new { pedido.IdPedido, idPago });
})
.WithName("ProcesarPedido");

app.Run();
