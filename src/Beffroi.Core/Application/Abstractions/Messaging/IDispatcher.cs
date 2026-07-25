namespace Beffroi.Core.Application.Abstractions.Messaging;

/// <summary>
/// Point d'entrée unique des adapters primaires (contrôleurs HTTP, jobs, CLI...) vers l'application.
/// L'implémentation vit dans Beffroi.Infrastructure : le cœur ne connaît que ce contrat.
/// </summary>
public interface IDispatcher
{
    /// <summary>Exécute une commande sans résultat.</summary>
    Task SendAsync(ICommand command, CancellationToken cancellationToken = default);

    /// <summary>Exécute une commande et retourne son résultat.</summary>
    Task<TResponse> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default);

    /// <summary>Exécute une requête de lecture.</summary>
    Task<TResponse> QueryAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default);
}
