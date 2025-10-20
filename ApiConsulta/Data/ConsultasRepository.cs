using ApiConsulta.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace ApiConsulta.Data;

public class ConsultasRepository
{
    private readonly IMongoCollection<ConsultaDoc> _col;

    public ConsultasRepository(IOptions<MongoSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        var db = client.GetDatabase(settings.Value.Database);
        _col = db.GetCollection<ConsultaDoc>(settings.Value.Collection);
    }

    public Task InsertAsync(ConsultaDoc doc) => _col.InsertOneAsync(doc);

    public async Task<List<ConsultaDoc>> ListAllAsync() =>
        await _col.Find(Builders<ConsultaDoc>.Filter.Empty).ToListAsync();
}
