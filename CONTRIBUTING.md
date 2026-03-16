# Contributing to Avalonia.Controls.Maui

PRs are always welcomed from everyone. Following this guide will help us get your PR reviewed and merged as quickly as possible.

For this guide we're going to split PRs into two types: bug fixes and features; the requirements for each are slightly different.

## Bug Fixes

A bug fix will ideally be accompanied by tests. There are two types of tests:

- **Unit tests** are for issues that aren't related to rendering. These tests are located in `tests/Avalonia.Controls.Maui.Tests`, categorised by the handler they're testing.
- **Render tests** are for issues with visual output. These tests are located in `tests/Avalonia.Controls.Maui.RenderTests` and compare rendered PNG output against reference images in the `Assets/` folder.

It's not always feasible to accompany a bug fix with a test, but doing so will speed up the review process.

The commits in a bug fix PR **should follow this pattern**:

- A commit with a failing unit test; followed by
- A commit that fixes the issue

In this way the reviewer can check out the commit with the failing test and confirm the problem, then confirm the fix.

## Features

**Features should be discussed with the core team before opening a PR.** Please open an issue to discuss the feature before starting work, to ensure the core team are onboard.

Features should always include unit tests or render tests where possible.

Features that introduce new control handlers should consider the following:

- Check [STATUS.md](STATUS.md) and update it to reflect the new implementation status.
- The handler should map all properties listed in `IMyControl` to the platform view.
- If a suitable Avalonia control does not exist, a minimal custom platform control may be added under `src/Avalonia.Controls.Maui/Controls/` — keep it `internal` unless there is a clear reason to expose it.
- Consider writing an `AutomationPeer` if the control introduces interactive behaviour.

## General Guidance

### PR description

- The PR template contains sections to fill in. These are discretionary and are intended to provide guidance rather than being prescriptive: feel free to delete sections that do not apply, or add additional sections.
- **Please** provide a good description of the PR. Not doing so **will** delay review at a minimum, or may cause it to be closed.
- Link any fixed issues with a `Fixes #1234` comment.

### Breaking changes

- Carefully consider behavioral breaking changes and point them out in the PR description.
- Do not change the public API surface of existing handlers without prior discussion.

### Commits

- Rebase your changes to remove extraneous commits. Ideally the commit history should tell a clean story of how the PR was implemented.
- Provide meaningful commit comments.
- **Do not** change code unrelated to the bug fix/feature.
- **Do not** introduce spurious formatting or whitespace changes.

While it's tempting to fix style issues you encounter, don't do it:

- It causes the reviewer to get distracted by unrelated changes.
- It makes finding the cause of any later issue more difficult (blame/bisect).
- As the code churns, style issues will be resolved anyway.

### Style

- The codebase uses [.NET core](https://github.com/dotnet/runtime/blob/master/docs/coding-guidelines/coding-style.md) coding style.
- Try to keep lines of code around 120 characters in length or less, though this is not a hard limit.
- Public methods should have XML documentation.
- Prefer terseness to verbosity but don't try to be too clever.
- **DO NOT USE #REGIONS**.

Test methods should be named in a sentence style, separated by underscores, describing in English what the test is testing:

```csharp
void IndicatorView_Position_Updates_PipsPager_SelectedIndex()
```

Render tests should describe what the produced image shows:

```csharp
void Render_IndicatorView_MiddleSelected()
```

### Handler structure

Each control follows a consistent two-file pattern:

```
src/Avalonia.Controls.Maui/
├── Handlers/MyControlHandler.cs        # Mapper + static Map*() methods
└── Extensions/MyControlExtensions.cs  # Extension methods on the platform view
```

- The **handler file** defines `Mapper`, `CommandMapper`, constructors, `CreatePlatformView`, `ConnectHandler`, `DisconnectHandler`, and static `Map*()` methods that delegate to extensions.
- The **extensions file** contains all mutation logic as `static` extension methods on the platform view type. This keeps them reusable and independently testable.

### Render tests — reference images

When adding a new render test:

1. Write the test — it will fail on first run because no reference image exists yet.
2. Run the test once; the actual output is saved as `ClassName_TestName.out.png` next to the test binary.
3. Review the output visually to confirm it is correct.
4. Copy it to `tests/Avalonia.Controls.Maui.RenderTests/Assets/ClassName_TestName.expected.png`.
5. Commit both the test and the reference image.

When an intentional visual change causes existing render tests to fail, update the reference images by repeating steps 2–5 and explain the change in the PR description.

## Building

See [docs/build.md](docs/build.md) for full build instructions, including how to build against local Avalonia or MAUI source checkouts.

```bash
dotnet workload restore Avalonia.Controls.Maui.slnx
dotnet run --project build/_build.csproj
```

## Code of Conduct

This project has adopted the code of conduct defined by the Contributor Covenant to clarify expected behavior in our community. For more information see the [Code of Conduct](CODE_OF_CONDUCT.md).
