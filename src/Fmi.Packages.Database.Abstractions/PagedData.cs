namespace Fmi.Packages.Database.Abstractions;

/// <summary>
/// The paged response from the database.
/// </summary>
/// <param name="Count"></param>
/// <param name="Items"></param>
/// <typeparam name="TModel"></typeparam>
public record PagedData<TModel>(long Count, IEnumerable<TModel> Items) where TModel : IAggregateRoot;