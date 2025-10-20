namespace ApiPedidos.Models;

public class Pedido
{
    public int IdPedido { get; set; }          // PK
    public DateTime FechaPedido { get; set; }  // datetime
    public int IdCliente { get; set; }         // FK -> Cliente
    public decimal MontoPedido { get; set; }   // decimal(9,2)
    public int FormaPago { get; set; }         // 1 Efectivo, 2 TDC, 3 TDD

    public Cliente? Cliente { get; set; }
}
