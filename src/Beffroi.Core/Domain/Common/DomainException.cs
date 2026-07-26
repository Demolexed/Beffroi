namespace Beffroi.Core.Domain.Common;

/// <summary>
/// Violation d'un invariant du domaine : l'objet ne peut pas exister dans cet état.
/// </summary>
/// <remarks>
/// Choix assumé et provisoire : on signale les invariants par exception plutôt que par un
/// <c>Result&lt;T&gt;</c>. La stratégie d'erreur globale de l'API n'est pas encore tranchée ;
/// si elle bascule vers un type Result, c'est ici que la migration commence.
/// </remarks>
public sealed class DomainException(string message) : Exception(message)
{
    /// <summary>Lève l'exception si la condition est vraie.</summary>
    public static void ThrowIf(bool condition, string message)
    {
        if (condition)
        {
            throw new DomainException(message);
        }
    }
}
