using Beffroi.Core.Domain.Common;
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
    internal Siege(Personne titulaire, Fonction fonction, ListeElectorale? liste, Period periode)
    {
        Titulaire = titulaire;
        Fonction = fonction;
        Liste = liste;
        Periode = periode;
    }

    public Personne Titulaire { get; }

    public Fonction Fonction { get; }

    /// <summary>
    /// Liste d'élection. Nul sous 1 000 habitants : le scrutin y est majoritaire
    /// plurinominal, sans listes constituées.
    /// </summary>
    public ListeElectorale? Liste { get; }

    public Period Periode { get; private set; }

    public MotifDeFin? MotifDeFin { get; private set; }

    public bool EstEnCours => Periode.IsOpen;

    public bool EstOccupeAu(DateOnly date) => Periode.Contains(date);

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
