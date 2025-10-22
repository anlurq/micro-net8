using Microsoft.EntityFrameworkCore;
using ApiPagos.Data;
using ApiPagos.Models;
using BuildingBlocks.DTOs; // RegistroPagoDto
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Instrumentation.Http;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<PagosDbContext>(opt =>
    opt.UseSqlServer(
        builder.Configuration.GetConnectionString("Sql"),
        sql => sql.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null
        )
    )
);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var jaegerHost = builder.Configuration["Jaeger:Host"] ?? "localhost";
var jaegerPort = int.TryParse(builder.Configuration["Jaeger:Port"], out var p) ? p : 6831;

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService(serviceName: "ApiPagos")) // cambia a "ApiPagos" en ApiPagos
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddJaegerExporter(o =>
        {
            o.AgentHost = jaegerHost;
            o.AgentPort = jaegerPort;
        })
    );

// Serilog: lee configuración (JSON + env)
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog(Log.Logger);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Aplicar migraciones de EF en arranque (Pagos)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApiPagos.Data.PagosDbContext>();
    db.Database.Migrate();
}

app.MapPost("/pago", async (RegistroPagoDto dto, PagosDbContext db) =>
{
    // Validación mínima
    if (dto.IdCliente <= 0 || dto.IdPedido <= 0 || dto.MontoPago <= 0 || dto.FormaPago is < 1 or > 3)
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["IdCliente/IdPedido/MontoPago/FormaPago"] = new[] { "Valores inválidos." }
        });

    var pago = new Pago
    {
        IdCliente = dto.IdCliente,
        IdPedido = dto.IdPedido,
        MontoPago = dto.MontoPago,
        FormaPago = dto.FormaPago,
        FechaPago = DateTime.UtcNow
    };

    db.Pagos.Add(pago);
    await db.SaveChangesAsync();

    return Results.Ok(new { idPago = pago.IdPago });
})
.WithName("RegistrarPago");

app.Run();
