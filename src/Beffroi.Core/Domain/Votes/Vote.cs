using Beffroi.Core.Domain.Common;
using Beffroi.Core.Domain.Elus;
using Beffroi.Core.Domain.Votes.Enums;

namespace Beffroi.Core.Domain.Votes;

/// <summary>
/// Résultat chiffré d'un vote sur une délibération.
///
/// Deux niveaux de précision, et l'ordre compte :
/// le <b>décompte par groupe</b> est la primitive fiable — c'est ce que les procès-verbaux
/// donnent systématiquement. Les <b>positions individuelles</b> sont facultatives : les PV ne
/// nomment les votants que par intermittence (à Sèvres, seules les abstentions le sont).
/// Les rendre obligatoires reviendrait à fabriquer une donnée que la source ne contient pas.
/// </summary>
public sealed class Vote
{
    private readonly List<DecompteParGroupe> _decomptes;
    private readonly List<PositionIndividuelle> _positions;

    private Vote(List<DecompteParGroupe> decomptes, List<PositionIndividuelle> positions)
    {
        _decomptes = decomptes;
        _positions = positions;
    }

    public IReadOnlyList<DecompteParGroupe> Decomptes => _decomptes;

    /// <summary>Positions nominatives, quand la source les donne. Souvent partielles.</summary>
    public IReadOnlyList<PositionIndividuelle> Positions => _positions;

    public int TotalPour => _decomptes.Sum(decompte => decompte.Pour);

    public int TotalContre => _decomptes.Sum(decompte => decompte.Contre);

    public int TotalAbstentions => _decomptes.Sum(decompte => decompte.Abstentions);

    public int TotalExprimes => TotalPour + TotalContre + TotalAbstentions;

    /// <summary>Vrai si aucune voix contre ni abstention n'a été exprimée.</summary>
    public bool EstUnanime => TotalPour > 0 && TotalContre == 0 && TotalAbstentions == 0;

    public static Vote Create(
        IEnumerable<DecompteParGroupe> decomptes,
        IEnumerable<PositionIndividuelle>? positions = null)
    {
        var listeDecomptes = decomptes.ToList();
        var listePositions = positions?.ToList() ?? [];

        DomainException.ThrowIf(
            listeDecomptes.Count == 0,
            "Un vote comporte au moins un décompte de voix.");

        DomainException.ThrowIf(
            listeDecomptes.Select(decompte => decompte.Groupe.Valeur).Distinct(StringComparer.OrdinalIgnoreCase).Count()
            != listeDecomptes.Count,
            "Un même groupe ne peut apparaître deux fois dans les décomptes d'un vote.");

        DomainException.ThrowIf(
            listePositions.Select(position => position.Elu).Distinct().Count() != listePositions.Count,
            "Un élu ne peut exprimer qu'une position par vote.");

        var vote = new Vote(listeDecomptes, listePositions);
        vote.VerifierCoherenceDesPositions();
        return vote;
    }

    /// <summary>
    /// Les positions nominatives connues ne peuvent pas excéder le décompte global :
    /// on ne peut pas nommer quatre votes contre si le procès-verbal en compte trois.
    /// L'inverse est normal — les positions sont souvent partielles.
    /// </summary>
    private void VerifierCoherenceDesPositions()
    {
        VerifierPlafond(PositionDeVote.Pour, TotalPour);
        VerifierPlafond(PositionDeVote.Contre, TotalContre);
        VerifierPlafond(PositionDeVote.Abstention, TotalAbstentions);
    }

    private void VerifierPlafond(PositionDeVote position, int plafond)
    {
        var nommes = _positions.Count(individuelle => individuelle.Position == position);

        DomainException.ThrowIf(
            nommes > plafond,
            $"{nommes} élus nommés en « {position} » alors que le décompte n'en compte que {plafond}.");
    }
}

/// <summary>Voix d'un groupe sur une délibération, telles que portées au procès-verbal.</summary>
public sealed record DecompteParGroupe
{
    private DecompteParGroupe(NomDeGroupe groupe, int pour, int contre, int abstentions)
    {
        Groupe = groupe;
        Pour = pour;
        Contre = contre;
        Abstentions = abstentions;
    }

    public NomDeGroupe Groupe { get; }

    public int Pour { get; }

    public int Contre { get; }

    public int Abstentions { get; }

    public static DecompteParGroupe Create(NomDeGroupe groupe, int pour, int contre, int abstentions)
    {
        DomainException.ThrowIf(
            pour < 0 || contre < 0 || abstentions < 0,
            "Un décompte de voix ne peut pas être négatif.");
        DomainException.ThrowIf(
            pour + contre + abstentions == 0,
            $"Le groupe « {groupe} » n'a exprimé aucune voix : ne pas créer de décompte vide.");

        return new DecompteParGroupe(groupe, pour, contre, abstentions);
    }

    public override string ToString() => $"{Groupe} — {Pour} pour, {Contre} contre, {Abstentions} abstentions";
}

/// <summary>Position nominative d'un élu, quand la source la donne.</summary>
public sealed record PositionIndividuelle(PersonneId Elu, PositionDeVote Position);
