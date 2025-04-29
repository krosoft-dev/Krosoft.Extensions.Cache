namespace Krosoft.Extensions.Cache.Distributed.Redis.Tests.Models;

public record PaysCache
{
    public string? Id { get; set; }
    public string? Libelle { get; set; }
}