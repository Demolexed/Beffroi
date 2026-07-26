namespace Beffroi.Core.Domain.Common;

/// <summary>
/// Référence au document d'origine dont un fait est tiré.
///
/// Invariant structurant du projet : aucun fait publié par Beffroi ne doit pouvoir exister
/// sans sa source. C'est pour cela que ce type est obligatoire à la construction des agrégats,
/// et non un champ optionnel ajouté après coup.
///
/// Les deux dates gardent leur nom français : ce sont des notions du CGCT, pas des horodatages
/// techniques.
/// </summary>
public sealed record Source
{
    private Source(Uri url, DateOnly datePublication, DateOnly? dateTeletransmission)
    {
        Url = url;
        DatePublication = datePublication;
        DateTeletransmission = dateTeletransmission;
    }

    /// <summary>Lien vers le document original, tel que publié par l'autorité.</summary>
    public Uri Url { get; }

    /// <summary>Date à laquelle le document a été rendu public.</summary>
    public DateOnly DatePublication { get; }

    /// <summary>
    /// Date de télétransmission au contrôle de légalité (art. L2131-1 CGCT), quand elle est connue.
    /// L'écart avec <see cref="DatePublication"/> est en soi une information d'intérêt public.
    /// </summary>
    public DateOnly? DateTeletransmission { get; }

    public static Source Create(Uri url, DateOnly datePublication, DateOnly? dateTeletransmission = null)
    {
        DomainException.ThrowIf(!url.IsAbsoluteUri, "L'URL de la source doit être absolue.");
        DomainException.ThrowIf(
            url.Scheme is not ("https" or "http"),
            $"Schéma d'URL non supporté pour une source : {url.Scheme}.");
        DomainException.ThrowIf(
            dateTeletransmission is not null && dateTeletransmission > datePublication,
            "Un acte ne peut pas être publié avant d'avoir été télétransmis.");

        return new Source(url, datePublication, dateTeletransmission);
    }
}
