using Beffroi.Core.Domain.Budgets;
using Beffroi.Core.Domain.Communes;
using Beffroi.Core.Domain.Conseils;
using Beffroi.Core.Domain.Programmes;
using Beffroi.Core.Domain.Seances;

namespace Beffroi.Infrastructure.Adapters.InMemory;

/// <summary>
/// Contenu d'un jeu de démonstration, tel qu'une commune l'alimente.
/// </summary>
internal sealed record Jeu(
    Commune Commune,
    IReadOnlyList<ConseilMunicipal> Conseils,
    IReadOnlyList<Seance> Seances,
    IReadOnlyList<Budget> Budgets,
    IReadOnlyList<Programme> Programmes)
{
    public static Jeu Vide(Commune commune) => new(commune, [], [], [], []);
}
