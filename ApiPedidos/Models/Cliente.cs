namespace ApiPedidos.Models;

public class Cliente
{
    public int IdCliente { get; set; }                 // PK
    public string NombreCliente { get; set; } = null!; // varchar(100)
    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
