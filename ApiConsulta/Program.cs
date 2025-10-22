using ApiConsulta.Data;
using ApiConsulta.Messaging;
using BuildingBlocks.Contracts;
using MassTransit;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Mongo
builder.Services.Configure<MongoSettings>(builder.Configuration.GetSection("Mongo"));
builder.Services.AddSingleton<ConsultasRepository>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// RabbitMQ + MassTransit
var rabbitHost = builder.Configuration["RabbitMQ:Host"] ?? "amqp://guest:guest@localhost:5672";
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<PagoProcesadoConsumer>();
    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(new Uri(rabbitHost));
        cfg.ReceiveEndpoint("api-consulta-pagos", e =>
        {
            e.ConfigureConsumer<PagoProcesadoConsumer>(ctx);
            e.Bind<PagoProcesadoEvent>();
        });
    });
});

// Serilog: lee configuración (JSON + env)
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog(Log.Logger);

var app = builder.Build();

// Swagger sólo en dev
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Si te molesta el warning de HTTPS, puedes comentar la siguiente línea en dev:
app.UseHttpsRedirection();

// Endpoints
app.MapGet("/consulta", async (ConsultasRepository repo) =>
{
    var docs = await repo.ListAllAsync();
    return Results.Ok(docs);
})
.WithName("GetConsultas");

app.Run();