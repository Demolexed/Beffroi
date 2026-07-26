namespace Beffroi.Core.Domain.Thematiques.Enums;

/// <summary>
/// Socle thématique de Beffroi : la grille de lecture commune à toutes les communes.
///
/// Volontairement fermé et court. C'est ce qui permet au lecteur de comparer deux communes,
/// et au code couleur de l'interface d'avoir un sens stable. Ajouter une valeur ici est une
/// décision éditoriale, pas une évolution technique.
///
/// Le rattachement des rubriques propres à chaque mairie (« AFFAIRES FINANCIÈRES ET
/// BUDGÉTAIRES », « ENFANCE », « VIE CITOYENNE »…) vers ces sept valeurs est un travail
/// distinct, à traiter côté ingestion : c'est là que se loge le risque de biais.
/// </summary>
public enum Thematique
{
    Education,
    TransportsEtVoirie,
    Environnement,
    Finances,
    Urbanisme,
    Securite,
    CultureEtSport
}
