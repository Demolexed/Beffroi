using System.Collections.Concurrent;
using Beffroi.Core.Application.Abstractions.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Beffroi.Infrastructure.Messaging;

/// <summary>
/// Implémentation du bus CQRS au-dessus du conteneur DI natif.
///
/// Problème résolu : à l'appel, on ne connaît statiquement que <c>IQuery&lt;TResponse&gt;</c>,
/// pas le type concret de la requête — impossible donc de résoudre
/// <c>IQueryHandler&lt;TQuery, TResponse&gt;</c> directement. On construit une fois par type de
/// message un « wrapper » générique fermé, mis en cache, qui fait la résolution typée.
/// La réflexion ne coûte donc qu'au premier appel de chaque message.
/// </summary>
internal sealed class Dispatcher(IServiceProvider provider) : IDispatcher
{
    private static readonly ConcurrentDictionary<Type, CommandWrapperBase> CommandWrappers = new();
    private static readonly ConcurrentDictionary<Type, object> ResultCommandWrappers = new();
    private static readonly ConcurrentDictionary<Type, object> QueryWrappers = new();

    public Task SendAsync(ICommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var wrapper = CommandWrappers.GetOrAdd(
            command.GetType(),
            static type => Create<CommandWrapperBase>(typeof(CommandWrapper<>), type));

        return wrapper.HandleAsync(command, provider, cancellationToken);
    }

    public Task<TResponse> SendAsync<TResponse>(
        ICommand<TResponse> command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var wrapper = (ResultCommandWrapperBase<TResponse>)ResultCommandWrappers.GetOrAdd(
            command.GetType(),
            static (type, response) => Create<object>(typeof(ResultCommandWrapper<,>), type, response),
            typeof(TResponse));

        return wrapper.HandleAsync(command, provider, cancellationToken);
    }

    public Task<TResponse> QueryAsync<TResponse>(
        IQuery<TResponse> query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var wrapper = (QueryWrapperBase<TResponse>)QueryWrappers.GetOrAdd(
            query.GetType(),
            static (type, response) => Create<object>(typeof(QueryWrapper<,>), type, response),
            typeof(TResponse));

        return wrapper.HandleAsync(query, provider, cancellationToken);
    }

    private static T Create<T>(Type openWrapper, params Type[] arguments)
        => (T)Activator.CreateInstance(openWrapper.MakeGenericType(arguments))!;

    // --- Wrappers ------------------------------------------------------------------------------
    // Le type de base n'expose que ce que l'appelant connaît (rien, ou la seule réponse).
    // Le type dérivé, lui, connaît le type concret du message et peut donc demander le handler
    // exact au conteneur.

    private abstract class CommandWrapperBase
    {
        public abstract Task HandleAsync(object command, IServiceProvider provider, CancellationToken ct);
    }

    private sealed class CommandWrapper<TCommand> : CommandWrapperBase
        where TCommand : ICommand
    {
        public override Task HandleAsync(object command, IServiceProvider provider, CancellationToken ct)
            => provider.GetRequiredService<ICommandHandler<TCommand>>()
                       .HandleAsync((TCommand)command, ct);
    }

    private abstract class ResultCommandWrapperBase<TResponse>
    {
        public abstract Task<TResponse> HandleAsync(object command, IServiceProvider provider, CancellationToken ct);
    }

    private sealed class ResultCommandWrapper<TCommand, TResponse> : ResultCommandWrapperBase<TResponse>
        where TCommand : ICommand<TResponse>
    {
        public override Task<TResponse> HandleAsync(object command, IServiceProvider provider, CancellationToken ct)
            => provider.GetRequiredService<ICommandHandler<TCommand, TResponse>>()
                       .HandleAsync((TCommand)command, ct);
    }

    private abstract class QueryWrapperBase<TResponse>
    {
        public abstract Task<TResponse> HandleAsync(object query, IServiceProvider provider, CancellationToken ct);
    }

    private sealed class QueryWrapper<TQuery, TResponse> : QueryWrapperBase<TResponse>
        where TQuery : IQuery<TResponse>
    {
        public override Task<TResponse> HandleAsync(object query, IServiceProvider provider, CancellationToken ct)
            => provider.GetRequiredService<IQueryHandler<TQuery, TResponse>>()
                       .HandleAsync((TQuery)query, ct);
    }
}
