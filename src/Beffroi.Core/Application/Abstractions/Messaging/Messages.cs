namespace Beffroi.Core.Application.Abstractions.Messaging;

/// <summary>
/// Intention d'écriture qui ne retourne pas de résultat métier.
/// Marqueur : la seule façon d'exécuter une commande est de la passer à <see cref="IDispatcher"/>.
/// </summary>
public interface ICommand;

/// <summary>
/// Intention d'écriture qui retourne un résultat (identifiant généré, statut, ...).
/// </summary>
/// <typeparam name="TResponse">Type du résultat renvoyé par le handler.</typeparam>
public interface ICommand<out TResponse>;

/// <summary>
/// Intention de lecture. Ne doit jamais modifier l'état du système.
/// </summary>
/// <typeparam name="TResponse">Type de la vue de lecture renvoyée par le handler.</typeparam>
public interface IQuery<out TResponse>;
