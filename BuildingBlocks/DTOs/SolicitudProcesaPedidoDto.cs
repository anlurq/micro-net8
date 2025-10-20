namespace BuildingBlocks.DTOs;

public record SolicitudProcesaPedidoDto(
    int IdCliente,
    decimal MontoPago,
    int FormaPago
);