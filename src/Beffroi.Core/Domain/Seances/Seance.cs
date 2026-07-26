using Beffroi.Core.Domain.Common;
using Beffroi.Core.Domain.Conseils;
using Beffroi.Core.Domain.Elus;
using Beffroi.Core.Domain.Seances.Enums;

namespace Beffroi.Core.Domain.Seances;

/// <summary>
/// Racine d'agrégat : une réunion du conseil municipal, ce qui y a été décidé et qui y était.
/// </summary>
public sealed class Seance
{
    private readonly List<Deliberation> _deliberations = [];
    private readonly List<PresenceEnSeance> _presences = [];

    private Seance(SeanceId id, ConseilMunicipalId conseil, DateOnly date, Source source)
    {
        Id = id;
        Conseil = conseil;
        Date = date;
        Source = source;
        ProcesVerbal = ProcesVerbal.Attendu();
    }

    public SeanceId Id { get; }

    /// <summary>Conseil qui s'est réuni. Référence par identité : autre agrégat.</summary>
    public ConseilMunicipalId Conseil { get; }

    public DateOnly Date { get; }

    /// <summary>Convocation ou ordre du jour d'où la séance est connue.</summary>
    public Source Source { get; }

    public ProcesVerbal ProcesVerbal { get; }

    public IReadOnlyList<Deliberation> Deliberations => _deliberations;

    public IReadOnlyList<PresenceEnSeance> Presences => _presences;

    public int NombreDePresents => _presences.Count(presence => presence.CompteDansLeQuorum);

    public static Seance Tenir(ConseilMunicipalId conseil, DateOnly date, Source source)
        => new(SeanceId.New(), conseil, date, source);

    /// <summary>Reconstitue une séance dont l'identité est déjà connue.</summary>
    public static Seance Tenir(SeanceId id, ConseilMunicipalId conseil, DateOnly date, Source source)
        => new(id, conseil, date, source);

    public Deliberation Inscrire(Deliberation deliberation)
    {
        DomainException.ThrowIf(
            _deliberations.Any(existante => existante.Numero == deliberation.Numero),
            $"La délibération {deliberation.Numero} est déjà inscrite à cette séance.");

        _deliberations.Add(deliberation);
        return deliberation;
    }

    public void ConstaterPresence(PresenceEnSeance presence)
    {
        DomainException.ThrowIf(
            _presences.Any(existante => existante.Elu == presence.Elu),
            "La présence de cet élu est déjà constatée pour cette séance.");

        if (presence.PouvoirDonneA is { } mandataire)
        {
            // Art. L2121-20 CGCT : un conseiller ne peut être porteur que d'un seul pouvoir.
            DomainException.ThrowIf(
                _presences.Count(existante => existante.PouvoirDonneA == mandataire) >= 1,
                "Un conseiller municipal ne peut détenir qu'un seul pouvoir (art. L2121-20 CGCT).");
        }

        _presences.Add(presence);
    }

    /// <summary>
    /// Quorum atteint : la majorité des membres en exercice est physiquement présente
    /// (art. L2121-17 CGCT). Les pouvoirs ne comptent pas.
    /// </summary>
    public bool QuorumAtteint(int membresEnExercice)
    {
        DomainException.ThrowIf(
            membresEnExercice < 1,
            "Le nombre de membres en exercice doit être renseigné pour calculer le quorum.");

        return NombreDePresents > membresEnExercice / 2.0;
    }

    public IReadOnlyList<Deliberation> DeliberationsNonUnanimes()
        => [.. _deliberations.Where(deliberation => deliberation.Vote is not null && !deliberation.EstUnanime)];

    public override string ToString()
        => $"Séance du {Date:d} — {_deliberations.Count} délibérations";
}

/// <summary>Identité technique d'une <see cref="Seance"/>.</summary>
public readonly record struct SeanceId(Guid Valeur)
{
    public static SeanceId New() => new(Guid.CreateVersion7());

    public override string ToString() => Valeur.ToString();
}
