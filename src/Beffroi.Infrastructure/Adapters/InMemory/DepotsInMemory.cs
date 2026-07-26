using Beffroi.Core.Application.Ports;
using Beffroi.Core.Domain.Budgets;
using Beffroi.Core.Domain.Communes;
using Beffroi.Core.Domain.Conseils;
using Beffroi.Core.Domain.Programmes;
using Beffroi.Core.Domain.Seances;

namespace Beffroi.Infrastructure.Adapters.InMemory;

/// <summary>
/// Entrepôt en mémoire, construit une fois au démarrage. Rien ne survit au redémarrage :
/// c'est un adapter de dépannage, à remplacer par une vraie persistance.
///
/// Deux jeux cohabitent, et c'est délibéré : <see cref="JeuSevres"/> ne contient que des faits
/// vérifiés sur une commune réelle, <see cref="JeuSaintAmand"/> est une commune ouvertement
/// fictive qui sert à illustrer ce que les documents de Sèvres ne disent pas.
/// </summary>
internal sealed class EntrepotInMemory
{
    private readonly Lazy<IReadOnlyList<Jeu>> _jeux = new(
        () => [JeuSevres.Construire(), JeuSaintAmand.Construire()],
        LazyThreadSafetyMode.ExecutionAndPublication);

    public IEnumerable<Commune> Communes => _jeux.Value.Select(jeu => jeu.Commune);

    public IEnumerable<ConseilMunicipal> Conseils => _jeux.Value.SelectMany(jeu => jeu.Conseils);

    public IEnumerable<Seance> Seances => _jeux.Value.SelectMany(jeu => jeu.Seances);

    public IEnumerable<Budget> Budgets => _jeux.Value.SelectMany(jeu => jeu.Budgets);

    public IEnumerable<Programme> Programmes => _jeux.Value.SelectMany(jeu => jeu.Programmes);
}

internal sealed class CommuneRepositoryInMemory(EntrepotInMemory entrepot) : ICommuneRepository
{
    public Task<Commune?> ObtenirParCodeAsync(CodeInsee code, CancellationToken cancellationToken)
        => Task.FromResult(entrepot.Communes.SingleOrDefault(commune => commune.Code == code));

    public Task<IReadOnlyList<Commune>> RechercherAsync(string? recherche, CancellationToken cancellationToken)
    {
        var resultats = entrepot.Communes;

        if (!string.IsNullOrWhiteSpace(recherche))
        {
            var terme = recherche.Trim();
            resultats = resultats.Where(commune =>
                commune.Nom.Contains(terme, StringComparison.OrdinalIgnoreCase)
                || commune.Code.Valeur.StartsWith(terme, StringComparison.OrdinalIgnoreCase));
        }

        IReadOnlyList<Commune> liste = [.. resultats.OrderBy(commune => commune.Nom, StringComparer.CurrentCulture)];
        return Task.FromResult(liste);
    }
}

internal sealed class ConseilMunicipalRepositoryInMemory(EntrepotInMemory entrepot) : IConseilMunicipalRepository
{
    public Task<ConseilMunicipal?> ObtenirAsync(CodeInsee commune, DateOnly au, CancellationToken cancellationToken)
        => Task.FromResult(entrepot.Conseils.FirstOrDefault(conseil =>
            conseil.Mandature.Commune == commune && conseil.Mandature.Periode.Contains(au)));

    public Task<ConseilMunicipal?> ObtenirParIdAsync(ConseilMunicipalId id, CancellationToken cancellationToken)
        => Task.FromResult(entrepot.Conseils.SingleOrDefault(conseil => conseil.Id == id));

    public Task<IReadOnlyList<ConseilMunicipal>> ListerMandaturesAsync(
        CodeInsee commune,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<ConseilMunicipal> mandatures =
        [
            .. entrepot.Conseils
                .Where(conseil => conseil.Mandature.Commune == commune)
                .OrderByDescending(conseil => conseil.Mandature.Periode.Start)
        ];

        return Task.FromResult(mandatures);
    }
}

internal sealed class SeanceRepositoryInMemory(EntrepotInMemory entrepot) : ISeanceRepository
{
    public Task<IReadOnlyList<Seance>> ListerAsync(
        CodeInsee commune,
        DateOnly? depuis,
        DateOnly? jusqua,
        CancellationToken cancellationToken)
    {
        var conseils = entrepot.Conseils
            .Where(conseil => conseil.Mandature.Commune == commune)
            .Select(conseil => conseil.Id)
            .ToHashSet();

        IReadOnlyList<Seance> resultats =
        [
            .. entrepot.Seances
                .Where(seance => conseils.Contains(seance.Conseil))
                .Where(seance => depuis is null || seance.Date >= depuis)
                .Where(seance => jusqua is null || seance.Date <= jusqua)
                .OrderByDescending(seance => seance.Date)
        ];

        return Task.FromResult(resultats);
    }

    public Task<Seance?> ObtenirAsync(SeanceId id, CancellationToken cancellationToken)
        => Task.FromResult(entrepot.Seances.SingleOrDefault(seance => seance.Id == id));

    public Task<Seance?> ObtenirParDeliberationAsync(DeliberationId deliberation, CancellationToken cancellationToken)
        => Task.FromResult(entrepot.Seances.FirstOrDefault(seance =>
            seance.Deliberations.Any(inscrite => inscrite.Id == deliberation)));
}

internal sealed class BudgetRepositoryInMemory(EntrepotInMemory entrepot) : IBudgetRepository
{
    public Task<IReadOnlyList<Budget>> ListerAsync(CodeInsee commune, CancellationToken cancellationToken)
    {
        IReadOnlyList<Budget> resultats =
        [
            .. entrepot.Budgets
                .Where(budget => budget.Commune == commune)
                .OrderByDescending(budget => budget.Exercice)
        ];

        return Task.FromResult(resultats);
    }

    public Task<Budget?> ObtenirAsync(CodeInsee commune, int exercice, CancellationToken cancellationToken)
        => Task.FromResult(entrepot.Budgets.FirstOrDefault(budget =>
            budget.Commune == commune && budget.Exercice == exercice));

    public Task<Budget?> ObtenirLePlusRecentAsync(CodeInsee commune, CancellationToken cancellationToken)
        => Task.FromResult(entrepot.Budgets
            .Where(budget => budget.Commune == commune)
            .MaxBy(budget => budget.Exercice));
}

internal sealed class ProgrammeRepositoryInMemory(EntrepotInMemory entrepot) : IProgrammeRepository
{
    public Task<IReadOnlyList<Programme>> ListerAsync(CodeInsee commune, CancellationToken cancellationToken)
    {
        IReadOnlyList<Programme> resultats = [.. entrepot.Programmes.Where(p => p.Commune == commune)];
        return Task.FromResult(resultats);
    }

    public Task<Programme?> ObtenirParEngagementAsync(EngagementId engagement, CancellationToken cancellationToken)
        => Task.FromResult(entrepot.Programmes.FirstOrDefault(programme =>
            programme.Engagements.Any(inscrit => inscrit.Id == engagement)));
}
