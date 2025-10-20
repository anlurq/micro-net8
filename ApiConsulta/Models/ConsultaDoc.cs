using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ApiConsulta.Models;

public class ConsultaDoc
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? IdConsulta { get; set; }  // Puedes usar ObjectId como PK (o GUID/Int si prefieres)

    public int IdPedido { get; set; }
    public string NombreCliente { get; set; } = null!;
    public int IdPago { get; set; }
    public int FormaPago { get; set; }       // 1..3
    public decimal MontoPago { get; set; }   // (no hay tipo decimal nativo en BSON; se serializa como Decimal128 si configuras, o double)
}
