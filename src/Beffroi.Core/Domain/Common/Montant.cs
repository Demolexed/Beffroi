namespace Beffroi.Core.Domain.Common;

/// <summary>
/// Montant en euros. Toutes les données budgétaires communales sont libellées en euros ;
/// le type existe pour éviter qu'un <c>decimal</c> nu se retrouve mélangé à un nombre d'habitants
/// ou à un décompte de voix.
/// </summary>
public sealed record Montant
{
    private Montant(decimal euros) => Euros = euros;

    public decimal Euros { get; }

    public static Montant EnEuros(decimal euros)
    {
        DomainException.ThrowIf(euros < 0, "Un montant de délibération ne peut pas être négatif.");
        return new Montant(euros);
    }

    public override string ToString() => $"{Euros:N2} €";
}
