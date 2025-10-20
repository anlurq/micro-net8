namespace BuildingBlocks.Contracts;

public record PagoProcesadoEvent(
    int IdPedido,
    string NombreCliente,
    int IdPago,
    decimal MontoPago,
    FormaPago FormaPago
);