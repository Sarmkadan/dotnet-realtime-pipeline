# HealthCheckServiceExtensions

`HealthCheckServiceExtensions` adds convenience methods for registering several health checks and retrieving focused results from a [`HealthCheckService`](HealthCheckService.md).

The query methods each call `PerformCompleteHealthCheckAsync`. They therefore run every registered component check and request the current pipeline health before returning their result. If several query methods are called in sequence, each call performs a separate complete check.

## API

### `Task<SystemHealth> GetOverallHealthAsync()`

Performs a complete health check and returns the report's `OverallStatus` value.

- Throws `ArgumentNullException` when the service is `null`.

### `Task<ComponentHealth?> GetComponentHealthAsync(string componentName)`

Performs a complete health check and returns the health result stored under `componentName`. It returns `null` when the report does not contain that name. Component lookup uses the report dictionary's exact key matching, so callers should use the same name and casing supplied during registration.

- Throws `ArgumentNullException` when the service is `null`.
- Throws `ArgumentException` when `componentName` is `null` or empty.

### `Task<string> GetHealthSummaryAsync()`

Performs a complete health check and returns an invariant-culture summary in this form:

```text
Overall: Healthy, Pipeline: Running, Throughput: 125.50 items/s, SuccessRate: 99.25%
```

The method treats the report's `SuccessRate` as a percentage value, divides it by 100, and formats it with the `P2` percentage format. Throughput is formatted with two decimal places.

- Throws `ArgumentNullException` when the service is `null`.

### `void RegisterComponents(IEnumerable<(string Name, Func<Task<ComponentHealth>> HealthCheck)> components)`

Registers each named asynchronous delegate by calling `HealthCheckService.RegisterComponent`. Registration preserves the enumeration order.

- Throws `ArgumentNullException` when the service, `components`, or a health-check delegate is `null`.
- Throws `ArgumentException` when the collection is empty or a component name is `null` or empty.

The collection is enumerated once to check that it is not empty and again to register its entries. Use a repeatable collection, such as an array or list, when the source cannot safely be enumerated more than once. Registrations completed before an invalid later entry are not rolled back.

## Example

The following example assumes `healthCheckService` has been constructed or resolved from dependency injection:

```csharp
using System;
using System.Threading.Tasks;
using DotNetRealtimePipeline.Monitoring;

HealthCheckService service = healthCheckService;

service.RegisterComponents(new[]
{
    (
        Name: "event-store",
        HealthCheck: (Func<Task<ComponentHealth>>)(() => Task.FromResult(
            new ComponentHealth
            {
                IsHealthy = true,
                Message = "Event store is reachable"
            }))
    ),
    (
        Name: "publisher",
        HealthCheck: (Func<Task<ComponentHealth>>)(() => Task.FromResult(
            new ComponentHealth
            {
                IsHealthy = true,
                Message = "Publisher is connected"
            }))
    )
});

SystemHealth overall = await service.GetOverallHealthAsync();
ComponentHealth? eventStore =
    await service.GetComponentHealthAsync("event-store");
string summary = await service.GetHealthSummaryAsync();

Console.WriteLine($"Overall: {overall}");
Console.WriteLine(eventStore?.Message ?? "Component is not registered");
Console.WriteLine(summary);
```

For the underlying service, report types, and direct registration API, see [`HealthCheckService`](HealthCheckService.md).
