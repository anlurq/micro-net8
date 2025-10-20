namespace ApiPagos.Models;

public class Pago
{
    public int IdPago { get; set; }            // PK
    public DateTime FechaPago { get; set; }    // datetime
    public int IdCliente { get; set; }
    public int FormaPago { get; set; }         // 1..3
    public int IdPedido { get; set; }          // FK lógico al IdPedido
    public decimal MontoPago { get; set; }     // decimal(9,2)
}
