using Beffroi.Core.Domain.Common;

namespace Beffroi.Core.Domain.Conseils;

/// <summary>
/// Liste ayant obtenu des sièges au scrutin municipal.
///
/// C'est le seul rattachement politique que Beffroi enregistre, et c'est délibéré :
/// une liste est un fait électoral, vérifiable dans les résultats du scrutin.
/// « Majorité » et « opposition », elles, ne figurent dans aucun registre officiel — le CGCT
/// ne les définit qu'en creux (art. L2121-27-1 : « conseillers n'appartenant pas à la majorité
/// municipale »). Les enregistrer comme attribut d'une personne produirait une affirmation
/// éditoriale non sourçable. Elles sont donc <b>dérivées</b>, jamais stockées :
/// voir <see cref="ConseilMunicipal.ListeMajoritaireAu"/>.
///
/// Sous 1 000 habitants le scrutin est majoritaire plurinominal : il n'y a pas de liste,
/// et le rattachement d'un siège est alors nul.
/// </summary>
public sealed class ListeElectorale
{
    internal ListeElectorale(string nom, int nombreDeSieges)
    {
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(nom), "Le nom de la liste est obligatoire.");
        DomainException.ThrowIf(nombreDeSieges < 0, "Un nombre de sièges ne peut pas être négatif.");

        Nom = nom.Trim();
        NombreDeSieges = nombreDeSieges;
    }

    public string Nom { get; }

    /// <summary>Nombre de sièges attribués à cette liste à l'issue du scrutin.</summary>
    public int NombreDeSieges { get; }

    public override string ToString() => Nom;
}
