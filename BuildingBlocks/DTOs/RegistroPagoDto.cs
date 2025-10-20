namespace BuildingBlocks.DTOs;

public record RegistroPagoDto(
    int IdCliente,
    int IdPedido,
    decimal MontoPago,
    FormaPago FormaPago
);