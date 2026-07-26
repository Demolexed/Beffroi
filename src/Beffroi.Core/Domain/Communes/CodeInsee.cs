using Beffroi.Core.Domain.Common;

namespace Beffroi.Core.Domain.Communes;

/// <summary>
/// Code officiel géographique INSEE d'une commune : 5 caractères.
///
/// Ce n'est <b>pas</b> un entier : la Corse utilise 2A et 2B (2A004 = Ajaccio).
/// Le stocker en numérique perdrait aussi les zéros de tête (01001 = L'Abergement-Clémenciat).
/// </summary>
public sealed record CodeInsee
{
    private const int Longueur = 5;

    private CodeInsee(string valeur) => Valeur = valeur;

    public string Valeur { get; }

    public static CodeInsee Create(string valeur)
    {
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(valeur), "Le code INSEE est obligatoire.");

        var normalise = valeur.Trim().ToUpperInvariant();

        DomainException.ThrowIf(
            normalise.Length != Longueur,
            $"Un code INSEE fait {Longueur} caractères, reçu « {normalise} » ({normalise.Length}).");

        var prefixe = normalise[..2];
        var estCorse = prefixe is "2A" or "2B";

        DomainException.ThrowIf(
            !estCorse && !prefixe.All(char.IsAsciiDigit),
            $"Préfixe de département invalide dans « {normalise} ».");
        DomainException.ThrowIf(
            !normalise[2..].All(char.IsAsciiDigit),
            $"Les trois derniers caractères d'un code INSEE sont numériques : « {normalise} ».");

        return new CodeInsee(normalise);
    }

    public override string ToString() => Valeur;
}
