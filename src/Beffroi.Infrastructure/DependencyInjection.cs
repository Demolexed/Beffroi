using System.Reflection;
using Beffroi.Core.Application.Abstractions.Messaging;
using Beffroi.Core.Application.Ports;
using Beffroi.Infrastructure.Adapters;
using Beffroi.Infrastructure.Adapters.InMemory;
using Beffroi.Infrastructure.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Beffroi.Infrastructure;

/// <summary>
/// Composition root de l'hexagone : c'est ici, et nulle part ailleurs, que les ports du cœur
/// sont branchés sur leurs adapters concrets.
/// </summary>
public static class DependencyInjection
{
    private static readonly Type[] HandlerContracts =
    [
        typeof(ICommandHandler<>),
        typeof(ICommandHandler<,>),
        typeof(IQueryHandler<,>)
    ];

    public static IServiceCollection AddBeffroiInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IDispatcher, Dispatcher>();
        services.AddSingleton<IClock, SystemClock>();

        // Persistance de dépannage : jeu de démonstration en mémoire, perdu à chaque redémarrage.
        services.AddSingleton<EntrepotInMemory>();
        services.AddSingleton<ICommuneRepository, CommuneRepositoryInMemory>();
        services.AddSingleton<IConseilMunicipalRepository, ConseilMunicipalRepositoryInMemory>();
        services.AddSingleton<ISeanceRepository, SeanceRepositoryInMemory>();
        services.AddSingleton<IBudgetRepository, BudgetRepositoryInMemory>();
        services.AddSingleton<IProgrammeRepository, ProgrammeRepositoryInMemory>();

        services.AddApplicationHandlers(typeof(IDispatcher).Assembly);

        return services;
    }

    /// <summary>
    /// Enregistre tous les handlers CQRS trouvés dans l'assembly du cœur applicatif.
    /// Évite d'avoir à déclarer chaque handler à la main à mesure que les features arrivent.
    /// </summary>
    private static void AddApplicationHandlers(this IServiceCollection services, Assembly assembly)
    {
        var implementations = assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false, ContainsGenericParameters: false });

        foreach (var implementation in implementations)
        {
            var contracts = implementation.GetInterfaces()
                .Where(contract => contract.IsGenericType
                                   && HandlerContracts.Contains(contract.GetGenericTypeDefinition()));

            foreach (var contract in contracts)
            {
                services.AddTransient(contract, implementation);
            }
        }
    }
}
