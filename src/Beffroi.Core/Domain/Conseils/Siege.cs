using Beffroi.Core.Domain.Common;
using Beffroi.Core.Domain.Conseils.Enums;
using Beffroi.Core.Domain.Elus;

namespace Beffroi.Core.Domain.Conseils;

/// <summary>
/// Occupation d'un siège du conseil par une personne, pendant une période donnée.
///
/// C'est la pièce centrale du modèle. Un conseil municipal n'est pas une liste de personnes
/// mais un ensemble de sièges datés : en cours de mandat, un élu démissionne et le suivant
/// de liste le remplace (art. L270 du code électoral). Sans période de validité, la question
/// « qui siégeait le jour de cette délibération ? » devient impossible à trancher —
/// or c'est exactement ce qu'une plateforme de vérifiabilité doit savoir répondre.
///
/// Un siège ne se modifie que par son agrégat, <see cref="ConseilMunicipal"/>.
/// </summary>
public sealed class Siege
{
    private readonly List<Delegation> _delegations = [];

    internal Siege(SiegeId id, Personne titulaire, Fonction fonction, ListeElectorale? liste, Period periode)
    {
        Id = id;
        Titulaire = titulaire;
        Fonction = fonction;
        Liste = liste;
        Periode = periode;
    }

    public SiegeId Id { get; }

    public Personne Titulaire { get; }

    public Fonction Fonction { get; }

    /// <summary>
    /// Liste d'élection. Nul sous 1 000 habitants : le scrutin y est majoritaire
    /// plurinominal, sans listes constituées.
    /// </summary>
    public ListeElectorale? Liste { get; }

    public Period Periode { get; private set; }

    public MotifDeFin? MotifDeFin { get; private set; }

    /// <summary>
    /// Délégations confiées au titulaire. Le plus souvent zéro (conseiller sans délégation)
    /// ou une, mais un adjoint peut en cumuler plusieurs.
    /// </summary>
    public IReadOnlyList<Delegation> Delegations => _delegations;

    public bool EstEnCours => Periode.IsOpen;

    public bool EstOccupeAu(DateOnly date) => Periode.Contains(date);

    public IReadOnlyList<Delegation> DelegationsAu(DateOnly date)
        => [.. _delegations.Where(delegation => delegation.EstActiveAu(date))];

    internal void ConfierDelegation(Delegation delegation)
    {
        DomainException.ThrowIf(
            !delegation.Periode.IsWithin(Periode),
            $"La délégation ({delegation.Periode}) déborde l'occupation du siège ({Periode}).");

        DomainException.ThrowIf(
            _delegations.Any(existante => existante.Thematique == delegation.Thematique
                                          && existante.Periode.Overlaps(delegation.Periode)),
            $"Une délégation « {delegation.Thematique} » couvre déjà cette période sur ce siège.");

        _delegations.Add(delegation);
    }

    internal void Clore(DateOnly fin, MotifDeFin motif)
    {
        DomainException.ThrowIf(
            !Periode.IsOpen,
            $"Le siège de {Titulaire} est déjà clos ({Periode}).");

        Periode = Periode.Close(fin);
        MotifDeFin = motif;
    }

    public override string ToString() => $"{Titulaire} — {Fonction.GetType().Name} — {Periode}";
}

/// <summary>Identité technique d'un <see cref="Siege"/>.</summary>
public readonly record struct SiegeId(Guid Valeur)
{
    public static SiegeId New() => new(Guid.CreateVersion7());

    public override string ToString() => Valeur.ToString();
}
