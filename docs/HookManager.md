# HookManager

`HookManager` is a small publish-style extension point. Code registers delegates under a string hook name, and the pipeline (or any other caller) later runs every delegate registered under that name. It lives in `src/Plugins/ExtensionSystem.cs`, in the `DotNetRealtimePipeline.Plugins` namespace, next to `IPipelinePlugin`, `PluginManager`, `PluginConfiguration` and `PluginRegistry`.

```csharp
public sealed class HookManager
```

Handlers are stored in a private `Dictionary<string, List<Delegate>>`. The manager does not know what a hook means; the hook name is just a key, and the arguments are passed through to the handlers unchanged.

## Constructor

```csharp
public HookManager(ILogger<HookManager> logger)
```

The logger is required. Passing `null` throws `ArgumentNullException`. The manager logs registrations at Debug level and handler failures at Error level.

## RegisterHook

```csharp
public void RegisterHook(string hookName, Delegate handler)
```

Appends `handler` to the list for `hookName`. Several handlers can share one name; they are kept in registration order and run in that order.

- **Throws `ArgumentException`** if `hookName` is `null`, empty or whitespace (`ArgumentException.ThrowIfNullOrWhiteSpace`).
- **Throws `ArgumentNullException`** if `handler` is `null`.
- Logs at Debug level: `Hook registered: {Hook}`.

Notes:

- Hook names are case-sensitive, because the dictionary uses the default string comparer. `"Stage-Completed"` and `"stage-completed"` are different hooks.
- The same delegate instance can be registered more than once. It then runs once per registration.
- There is no unregister method. Handlers stay registered for the life of the `HookManager` instance.
- Any `Delegate` type is accepted: `Action`, `Func<..., Task>`, `Func<..., T>`, or a delegate created from a method group.

## ExecuteHookAsync

```csharp
public async Task ExecuteHookAsync(string hookName, params object[] args)
```

Runs every handler registered under `hookName`, one after another, in registration order.

- **Throws `ArgumentException`** if `hookName` is `null`, empty or whitespace.
- If no handler is registered under `hookName`, the method returns without doing anything. An unknown hook is not an error.
- Each handler is invoked with `Delegate.DynamicInvoke(args)`, so `args` is passed as the handler's parameter list, positionally. The `params` array form means `ExecuteHookAsync("name", a, b)` and `ExecuteHookAsync("name", new object[] { a, b })` are equivalent.

### Delegate invocation semantics

For each handler, in order:

1. The handler is invoked with `DynamicInvoke(args)`.
2. If the returned value is a `Task` (this includes `Task<T>`), it is awaited before the next handler starts. Exceptions from that task are caught the same way as synchronous exceptions.
3. If the returned value is anything else (including `null` from a `void` delegate), it is ignored.

Because handlers run sequentially, a slow asynchronous handler delays every handler registered after it. Handlers are not run in parallel.

`DynamicInvoke` is used rather than a typed `Invoke`, so the arguments are checked at run time, not compile time. The delegate's parameter types and count must match `args`. Mismatches are not reported to the caller; they are treated as handler failures (see below).

### Error handling

Failures inside a handler never propagate out of `ExecuteHookAsync`, and they never stop later handlers:

- **Synchronous exceptions** and **faulted tasks** are caught by a single `catch (Exception)` around each invocation. The manager logs them at Error level with the message `Error executing hook: {Hook}` and continues with the next handler.
- **Argument mismatches** (wrong number or types of arguments) are caught the same way. `DynamicInvoke` throws before the handler body runs, so the handler does not execute.
- Because of `DynamicInvoke`, the exception passed to the logger is usually a `TargetInvocationException` whose `InnerException` is the real error. Look at the inner exception when reading the log.

The method only throws for an invalid `hookName`. Errors from the handler code itself are logged, not thrown.

Two cases fall outside that protection:

- **`async void` handlers.** An `async void` lambda converts to `Action`, so `DynamicInvoke` returns `null` immediately. The manager does not see the returned work, cannot await it, and cannot catch its exceptions. Use `Func<..., Task>` for asynchronous handlers.
- **Registering a hook from inside a handler.** Adding a handler to the same hook name while `ExecuteHookAsync` is iterating its list changes the list during enumeration. The `foreach` is outside the `try`, so the resulting `InvalidOperationException` is thrown to the caller. Do not register hooks from inside a running hook.

## Thread safety

`HookManager` does not synchronize access to its dictionary or lists. Finish all `RegisterHook` calls before executing hooks, or coordinate registration and execution externally when they happen on different threads.

## Example

The example uses `NullLogger`, so it runs without logging setup. The handlers show an asynchronous handler, a synchronous handler that throws on bad input, and an unknown hook.

```csharp
using DotNetRealtimePipeline.Plugins;
using Microsoft.Extensions.Logging.Abstractions;

var hooks = new HookManager(NullLogger<HookManager>.Instance);

// Asynchronous handler: the manager awaits the Task before running the next handler.
hooks.RegisterHook("stage-completed", new Func<string, int, Task>(async (stage, count) =>
{
    await Task.Delay(10);
    Console.WriteLine($"{stage}: {count} items");
}));

// Synchronous handler: its void return value is ignored. Exceptions are logged, not rethrown.
hooks.RegisterHook("stage-completed", new Action<string, int>((stage, count) =>
{
    if (count == 0)
    {
        throw new InvalidOperationException("Empty batch");
    }

    Console.WriteLine($"Sync handler saw {stage}");
}));

// Runs both handlers, in registration order.
await hooks.ExecuteHookAsync("stage-completed", "Parse", 128);
// Output:
// Parse: 128 items
// Sync handler saw Parse

// The async handler prints. The sync handler throws; the manager logs the
// TargetInvocationException at Error level and returns normally.
await hooks.ExecuteHookAsync("stage-completed", "Parse", 0);
// Output:
// Parse: 0 items

// No handlers registered under this name: returns without doing anything.
await hooks.ExecuteHookAsync("no-such-hook", "Parse", 1);
```

## Related

- [ExtensionSystem](ExtensionSystem.md) - overview of the plugin, hook and registry system.
- [PluginConfiguration](PluginConfiguration.md) - plugin settings and the registry that stores them.
