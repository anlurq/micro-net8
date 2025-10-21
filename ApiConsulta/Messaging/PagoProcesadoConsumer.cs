using Microsoft.Extensions.Logging;
using BuildingBlocks.Contracts;
using ApiConsulta.Data;
using ApiConsulta.Models;
using MassTransit;

namespace ApiConsulta.Messaging;

public class PagoProcesadoConsumer(ConsultasRepository repo, ILogger<PagoProcesadoConsumer> logger)
    : IConsumer<PagoProcesadoEvent>
{
    public async Task Consume(ConsumeContext<PagoProcesadoEvent> ctx)
    {
        logger.LogInformation("Consumiendo PagoProcesadoEvent: {@Message}", ctx.Message);

        var e = ctx.Message;
        var doc = new ConsultaDoc
        {
            IdPedido = e.IdPedido,
            NombreCliente = e.NombreCliente,
            IdPago = e.IdPago,
            MontoPago = e.MontoPago,
            FormaPago = e.FormaPago
        };

        await repo.InsertAsync(doc);
    }
}
