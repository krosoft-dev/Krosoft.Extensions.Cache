using Krosoft.Extensions.Cache.Distributed.Redis.Interfaces;
using Krosoft.Extensions.Core.Models.Exceptions;
using Krosoft.Extensions.Core.Tools;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace Krosoft.Extensions.Cache.Distributed.Redis.Services;

internal class RedisConnectionFactory : IRedisConnectionFactory
{
    private readonly Lock _lock = new();
    private readonly ConfigurationOptions? _options;
    private volatile IConnectionMultiplexer? _connection;

    public RedisConnectionFactory(IConfiguration configuration)
    {
        Guard.IsNotNull(nameof(configuration), configuration);

        var connectionString = configuration["ConnectionStrings:Redis"];
        if (connectionString != null)
        {
            _options = ConfigurationOptions.Parse(connectionString);

            // Sauf surcharge explicite dans la chaîne de connexion, on laisse le multiplexer se
            // (re)connecter en tâche de fond plutôt que de lever dès la première tentative :
            // une indisponibilité passagère de Redis ne doit pas condamner l'application.
            if (!connectionString.Contains("abortConnect", StringComparison.OrdinalIgnoreCase))
            {
                _options.AbortOnConnectFail = false;
            }
        }
    }

    public IConnectionMultiplexer Connection
    {
        get
        {
            if (_options == null)
            {
                throw new KrosoftTechnicalException("Connection non disponible !");
            }

            var connection = _connection;
            if (connection != null)
            {
                return connection;
            }

            lock (_lock)
            {
                // Si la connexion échoue, _connection reste null et l'accès suivant réessaiera.
                // Un Lazy<T> aurait mémorisé l'exception et l'aurait rejouée jusqu'au redémarrage.
                return _connection ??= ConnectionMultiplexer.Connect(_options);
            }
        }
    }
}
