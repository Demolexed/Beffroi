using Beffroi.Core.Domain.Budgets;
using Beffroi.Core.Domain.Communes;
using Beffroi.Core.Domain.Conseils;
using Beffroi.Core.Domain.Programmes;
using Beffroi.Core.Domain.Seances;

namespace Beffroi.Core.Application.Ports;

/// <summary>
/// Ports secondaires de lecture. Le cœur déclare ce dont il a besoin ;
/// Beffroi.Infrastructure fournit les adapters (aujourd'hui en mémoire, demain une base
/// ou un client HTTP vers un portail de mairie).
/// </summary>
public interface ICommuneRepository
{
    Task<Commune?> ObtenirParCodeAsync(CodeInsee code, CancellationToken cancellationToken);

    Task<IReadOnlyList<Commune>> RechercherAsync(string? recherche, CancellationToken cancellationToken);
}

public interface IConseilMunicipalRepository
{
    /// <summary>Conseil dont la mandature couvre la date demandée.</summary>
    Task<ConseilMunicipal?> ObtenirAsync(CodeInsee commune, DateOnly au, CancellationToken cancellationToken);

    Task<ConseilMunicipal?> ObtenirParIdAsync(ConseilMunicipalId id, CancellationToken cancellationToken);

    /// <summary>Toutes les mandatures connues pour la commune, de la plus récente à la plus ancienne.</summary>
    Task<IReadOnlyList<ConseilMunicipal>> ListerMandaturesAsync(
        CodeInsee commune,
        CancellationToken cancellationToken);
}

public interface IBudgetRepository
{
    Task<IReadOnlyList<Budget>> ListerAsync(CodeInsee commune, CancellationToken cancellationToken);

    Task<Budget?> ObtenirAsync(CodeInsee commune, int exercice, CancellationToken cancellationToken);

    /// <summary>Budget de l'exercice le plus récent connu pour la commune.</summary>
    Task<Budget?> ObtenirLePlusRecentAsync(CodeInsee commune, CancellationToken cancellationToken);
}

public interface IProgrammeRepository
{
    Task<IReadOnlyList<Programme>> ListerAsync(CodeInsee commune, CancellationToken cancellationToken);

    Task<Programme?> ObtenirParEngagementAsync(EngagementId engagement, CancellationToken cancellationToken);
}

public interface ISeanceRepository
{
    Task<IReadOnlyList<Seance>> ListerAsync(
        CodeInsee commune,
        DateOnly? depuis,
        DateOnly? jusqua,
        CancellationToken cancellationToken);

    Task<Seance?> ObtenirAsync(SeanceId id, CancellationToken cancellationToken);

    /// <summary>Séance à laquelle une délibération a été examinée.</summary>
    Task<Seance?> ObtenirParDeliberationAsync(DeliberationId deliberation, CancellationToken cancellationToken);
}
