# BenchmarkApp

A headless benchmark and memory-leak test runner for Avalonia.Controls.Maui. It creates a real MAUI window backed by Avalonia, exercises controls, and reports pass/fail results with metrics. Designed to run in CI or locally without user interaction.

## Projects

- **BenchmarkApp** -- Shared library containing all test definitions, the runner infrastructure, and JUnit XML output.
- **BenchmarkApp.Desktop** -- Desktop host that launches the Avalonia window and runs tests.

### CLI Options

| Flag | Description |
|---|---|
| `--list` | Print all registered test names and descriptions, then exit. |
| `--test <name>` | Run a single test (case-insensitive). |
| `--run-all` | Run every registered test in order. |
| `--iterations <n>` | Repeat the test N times (default 1). |
| `--output <path>` | Write JUnit XML results to the given file path. |
| `--keep-open` | Keep the window open after completion instead of exiting. |

## Current Tests

### Memory Leak Tests

These tests create controls, connect handlers, tear them down, force GC, and verify that all tracked objects are collected. A test fails if any `WeakReference` target survives.

| Test Name | What It Covers |
|---|---|
| `HandlerDisconnectLeak` | Button, Label, Entry after handler disconnect |
| `PageNavigationLeak` | Complex nested control tree after simulated navigation |
| `RepeatedCreationLeak` | 50 batches of 10 controls, checks for slow leaks |
| `ShellNavigationLeak` | Pages created during Shell-style navigation cycles |
| `ImageSourceLeak` | Image controls after source changes and disconnect |
| `BindableLayoutLeak` | BindableLayout templated items after source repopulation |
| `ScrollViewLeak` | ScrollView with children after removal and disconnect |
| `GestureRecognizerLeak` | Controls with Tap, Swipe, Pan gesture recognizers |
| `InputControlLeak` | Editor, SearchBar, Picker, Switch, CheckBox, Slider with events |
| `CollectionViewLeak` | CollectionView with header, footer, and item templates |
| `TabbedPageLeak` | TabbedPage with child ContentPages |
| `WebViewMiscLeak` | WebView handler teardown with HTML source, cookies, and user-agent state |

### Performance Tests

These tests measure timing and always pass. They establish baselines for regression detection.

| Test Name | What It Measures |
|---|---|
| `ButtonCreation` | Time to create 100 buttons |
| `ControlCreationPerformance` | Time to create 50 instances of 10 control types (500 total) |

## Adding a New Test

### 1. Create the test class

Add a new file in `BenchmarkApp/Tests/`. The class must:

- Extend `BenchmarkTestPage`
- Have a `[BenchmarkTest]` attribute with a unique name
- Override `RunAsync`

```csharp
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

[BenchmarkTest("MyNewTest", Description = "What this test verifies")]
public class MyNewTestBenchmark : BenchmarkTestPage
{
    public override async Task<BenchmarkResult> RunAsync(
        Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        // --- Set up controls ---
        var layout = new VerticalStackLayout();
        Content = layout;

        var button = new Button { Text = "Test" };
        layout.Children.Add(button);

        var weakRef = new WeakReference<object>(button);

        await Task.Delay(50, cancellationToken);

        // --- Tear down ---
        button.Handler?.DisconnectHandler();
        layout.Children.Clear();
        button = null;
        Content = new Label { Text = "Done" };
        layout = null;

        // --- Force GC ---
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        await Task.Delay(100, cancellationToken);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var memAfter = MemorySnapshot.Capture(forceGC: false);
        var memoryDelta = memAfter.Compare(memBefore);

        // --- Check results ---
        bool leaked = weakRef.TryGetTarget(out _);
        var metrics = new Dictionary<string, object>
        {
            ["Leaked"] = leaked,
        };
        foreach (var (key, value) in memoryDelta.ToMetrics())
            metrics[key] = value;

        if (leaked)
            return BenchmarkResult.Fail("Object leaked", metrics);

        return BenchmarkResult.Pass(metrics);
    }
}
```

### 2. Register in BenchmarkRegistry

Add a `Register<T>()` call in the static constructor of `BenchmarkApp/BenchmarkRegistry.cs`:

```csharp
Register<MyNewTestBenchmark>();
```
