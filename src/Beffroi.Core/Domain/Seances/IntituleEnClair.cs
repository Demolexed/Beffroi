using Beffroi.Core.Domain.Common;

namespace Beffroi.Core.Domain.Seances;

/// <summary>
/// Reformulation en langage clair de l'objet d'une délibération.
///
/// Séparée de l'objet officiel, et jamais à sa place : l'objet officiel reste le verbatim
/// opposable, cette reformulation est un produit éditorial. La distinction est la condition
/// de la charte de neutralité — si les deux fusionnaient, plus personne ne pourrait vérifier
/// ce qui a été réécrit.
///
/// <see cref="EstRelu"/> matérialise la relecture humaine d'un texte généré : une reformulation
/// non relue peut exister en base, mais l'interface doit pouvoir la distinguer.
/// </summary>
public sealed record IntituleEnClair
{
    private IntituleEnClair(string texte, DateOnly? dateRelecture)
    {
        Texte = texte;
        DateRelecture = dateRelecture;
    }

    public string Texte { get; }

    public DateOnly? DateRelecture { get; }

    public bool EstRelu => DateRelecture is not null;

    /// <summary>Reformulation produite mais pas encore validée par un humain.</summary>
    public static IntituleEnClair Brouillon(string texte) => new(Valider(texte), null);

    public static IntituleEnClair Relu(string texte, DateOnly dateRelecture)
        => new(Valider(texte), dateRelecture);

    /// <summary>Valide la reformulation à une date donnée.</summary>
    public IntituleEnClair Relire(DateOnly dateRelecture)
    {
        DomainException.ThrowIf(EstRelu, "Cette reformulation est déjà relue.");
        return new IntituleEnClair(Texte, dateRelecture);
    }

    private static string Valider(string texte)
    {
        DomainException.ThrowIf(
            string.IsNullOrWhiteSpace(texte),
            "Une reformulation vide n'a pas de sens : ne pas en créer.");

        return texte.Trim();
    }

    public override string ToString() => Texte;
}
