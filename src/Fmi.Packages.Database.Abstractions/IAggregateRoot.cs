namespace Fmi.Packages.Database.Abstractions;

/// <summary>
/// An aggregable entity (not a nested property).
/// </summary>
public interface IAggregateRoot
{
    /// <summary>
    /// The entity identifier.
    /// </summary>
    string? Id { get; set; }
}