using System.Linq.Expressions;
using Fmi.Packages.ReflectionHelpers;
using MongoDB.Driver;

namespace Fmi.Packages.Database.Abstractions.Extensions;

public static class MongoAggregationExtensions
{
    public static IAggregateFluent<TNewResult> CountField<TResult, TResultPropertyType, TNewResult>(
        this IAggregateFluent<TResult> aggregate,
        Expression<Func<TResult, TResultPropertyType>> fieldToCount,
        Expression<Func<TNewResult, int>> destinationField
    ) =>
        aggregate.AppendStage<TNewResult>(
            $@"{{""$addFields"": {{""{destinationField.GetPropertyName()}"": {{""$size"":""${fieldToCount.GetPropertyName()}""}}}}}}"
        );

    public static async Task<PagedData<TResult>> PageItemsAsync<TResult>(
        this IAggregateFluent<TResult> aggregate,
        int skip,
        int take,
        SortDefinition<TResult>? sortDefinition = null) where TResult : IAggregateRoot
    {
        var countTask = aggregate.Sort(sortDefinition)
            .Count()
            .ToListAsync();

        var dataTask = aggregate.Sort(sortDefinition)
            .Skip(skip)
            .Limit(take)
            .ToListAsync();

        var count = await countTask;
        var data = await dataTask;

        var total = count.FirstOrDefault()?.Count ?? 0;

        return new PagedData<TResult>(total, data);
    }
}