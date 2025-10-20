namespace BuildingBlocks.DTOs;

public record SolicitudProcesaPedidoDto(
    int IdCliente,
    decimal MontoPago,
    FormaPago FormaPago
);