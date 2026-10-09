# Shos.UndoRedoList

[![NuGet](https://img.shields.io/nuget/v/Shos.UndoRedoList.svg)](https://www.nuget.org/packages/Shos.UndoRedoList/)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/Fujiwo/Shos.UndoRedoList/blob/master/LICENSE)

**English** | [日本語](https://github.com/Fujiwo/Shos.UndoRedoList/blob/master/README.jp.md)

A `List<T>` and an `ObservableCollection<T>` with **undo/redo** support for .NET.

Replace your list with `UndoRedoList<T>` (or your `ObservableCollection<T>` with `UndoRedoObservableCollection<T>`), and every `Add`, `Insert`, `Remove`, `RemoveAt`, `Clear` and indexer assignment can be undone and redone.

```csharp
var list = new UndoRedoList<string> { "apple", "banana" };
list[0] = "avocado";   // avocado, banana
list.Undo();           // apple, banana
list.Redo();           // avocado, banana
```

## Features

- **Drop-in replacement**: implements `IList<T>`, so existing code keeps working.
- **Undo/redo for every change**: covers add, insert, remove, clear, and replacing an element via the indexer.
- **Grouped actions**: several changes inside an `ActionScope` are undone and redone as **one step**.
- **Untracked changes**: changes inside a `DisabledUndoScope` are not recorded (e.g. loading initial data).
- **Bounded history**: the number of undo steps is limited (default: 100), so memory use doesn't grow without limit.
- **WPF / MVVM friendly**: `UndoRedoObservableCollection<T>` raises `CollectionChanged`, so you can bind it to `ItemsSource` directly.
- **No dependencies**: a small library that depends only on the .NET base class library.

## Supported frameworks

| Package version | Target frameworks |
|---|---|
| 1.0.6 | .NET 8.0, .NET 10.0 |
| 1.0.5 | .NET 8.0 |

## Installation

Install [Shos.UndoRedoList](https://www.nuget.org/packages/Shos.UndoRedoList/) from NuGet.

```console
dotnet add package Shos.UndoRedoList
```

Or, in the Package Manager Console of Visual Studio:

```console
PM> Install-Package Shos.UndoRedoList
```

Or, in your project file:

```xml
<PackageReference Include="Shos.UndoRedoList" Version="1.0.6" />
```

All types are in the `Shos.Collections` namespace.

```csharp
using Shos.Collections;
```

## Quick start

```csharp
using Shos.Collections;

var list = new UndoRedoList<string>();

list.Add("apple");
list.Add("banana");
list[0] = "avocado";
list.RemoveAt(1);
// list: avocado

list.Undo();   // list: avocado, banana   (RemoveAt is undone)
list.Undo();   // list: apple, banana     (list[0] = ... is undone)
list.Redo();   // list: avocado, banana   (list[0] = ... is redone)

Console.WriteLine(list.CanUndo); // True
Console.WriteLine(list.CanRedo); // True
```

`Undo()` and `Redo()` return `false` (and do nothing) when there is nothing to undo or redo. Use `CanUndo` / `CanRedo` to enable or disable your Undo/Redo buttons.

## Usage

### Undo several changes at once: `ActionScope`

All the changes made inside an `ActionScope` are recorded as **one** undo step.

```csharp
var numbers = new UndoRedoList<int> { 1, 2, 3 };

using (new UndoRedoList<int>.ActionScope(numbers)) {
    numbers.Add(4);
    numbers.RemoveAt(0);
    numbers[0] = 20;
}
// numbers: 20, 3, 4

numbers.Undo();
// numbers: 1, 2, 3   (all three changes are undone at once)
```

> `ActionScope` can't be nested. Starting a second `ActionScope` on the same list while one is active throws `InvalidOperationException`.

### Make changes that can't be undone: `DisabledUndoScope`

Changes made inside a `DisabledUndoScope` are not recorded. This is useful for loading initial data.

```csharp
var items = new UndoRedoList<string>();

using (new UndoRedoList<string>.DisabledUndoScope(items)) {
    items.Add("loaded 1");
    items.Add("loaded 2");
}

Console.WriteLine(items.CanUndo); // False
```

You can also set the `UndoEnabled` property directly.

> **Note**: Recorded actions remember the indexes of the elements they changed. If you make untracked changes **after** some changes have been recorded, call `ClearUndo()` afterwards. Otherwise, the next undo may be applied to the wrong position.

### Limit the number of undo steps

Pass `maximumUndoTimes` to the constructor (default: 100, minimum: 2). When the limit is exceeded, the oldest step is discarded.

```csharp
var list = new UndoRedoList<int>(maximumUndoTimes: 3);
for (var i = 1; i <= 5; i++)
    list.Add(i);

// Only the last 3 additions can be undone.
while (list.Undo())
    ;
// list: 1, 2
```

### Clear the undo/redo history

```csharp
list.ClearUndo(); // CanUndo and CanRedo become false. The elements are not changed.
```

### Redo history is discarded by a new change

Like most editors, making a new change after undoing discards the steps that could be redone.

```csharp
var list = new UndoRedoList<int> { 1, 2 };
list.Undo();      // list: 1        CanRedo: True
list.Add(3);      // list: 1, 3     CanRedo: False ("Add(2)" can no longer be redone)
```

### WPF / MVVM: `UndoRedoObservableCollection<T>`

`UndoRedoObservableCollection<T>` raises `CollectionChanged` for every change, including those made by `Undo()` and `Redo()`. Bind it to `ItemsSource` just like an `ObservableCollection<T>`.

`CanUndo` and `CanRedo` are already up to date when `CollectionChanged` is raised, so you can refresh your commands there.

```csharp
using Prism.Commands; // Any MVVM library works (e.g. CommunityToolkit.Mvvm).
using Shos.Collections;

public class MainViewModel
{
    public UndoRedoObservableCollection<string> Items { get; } = new();

    public DelegateCommand UndoCommand { get; }
    public DelegateCommand RedoCommand { get; }

    public MainViewModel()
    {
        UndoCommand = new DelegateCommand(() => Items.Undo(), () => Items.CanUndo);
        RedoCommand = new DelegateCommand(() => Items.Redo(), () => Items.CanRedo);

        Items.CollectionChanged += (_, _) => {
            UndoCommand.RaiseCanExecuteChanged();
            RedoCommand.RaiseCanExecuteChanged();
        };
    }
}
```

```xml
<ListBox ItemsSource="{Binding Items}" />
<Button Content="Undo" Command="{Binding UndoCommand}" />
<Button Content="Redo" Command="{Binding RedoCommand}" />
```

`CollectionChanged` is not raised by `ClearUndo()` or by changing `UndoEnabled`. Refresh your commands yourself after calling them.

See [Shos.UndoRedoList.SampleApp](https://github.com/Fujiwo/Shos.UndoRedoList/tree/master/Shos.UndoRedoList.SampleApp) for a complete WPF sample.

![Shos.UndoRedoList.SampleApp](https://raw.githubusercontent.com/Fujiwo/Shos.UndoRedoList/master/Images/Shos.UndoRedoList.SampleApp.png "Shos.UndoRedoList.SampleApp")

### Use another `IList<T>` as the inner list: `UndoRedoList<TElement, TList>`

`UndoRedoList<T>` uses `List<T>` internally. To use another collection, specify it as `TList`. It must implement `IList<TElement>` and have a parameterless constructor.

```csharp
var list = new UndoRedoList<int, ObservableCollection<int>>();
list.Add(1);

ObservableCollection<int> inner = list.List; // Read the inner collection.
```

## API

### Types

| Type | Description |
|---|---|
| `UndoRedoList<TElement>` | A `List<TElement>` that supports undo/redo. |
| `UndoRedoObservableCollection<TElement>` | An `ObservableCollection<TElement>` that supports undo/redo. Implements `INotifyCollectionChanged`. |
| `UndoRedoList<TElement, TList>` | The base class. Adds undo/redo to any `TList : IList<TElement>, new()`. |
| `UndoRedoList<TElement, TList>.ActionScope` | `IDisposable`. Groups the changes made in the scope into one undo step. |
| `UndoRedoList<TElement, TList>.DisabledUndoScope` | `IDisposable`. Changes made in the scope are not recorded. |

### Members of `UndoRedoList<TElement, TList>`

In addition to the `IList<TElement>` members:

| Member | Description |
|---|---|
| `UndoRedoList(int maximumUndoTimes = 100)` | Constructor. `maximumUndoTimes` is the maximum number of undo steps (2 or more). |
| `bool Undo()` | Undoes the last change. Returns `false` if there is nothing to undo. |
| `bool Redo()` | Redoes the last undone change. Returns `false` if there is nothing to redo. |
| `bool CanUndo` | Whether `Undo()` can be performed. |
| `bool CanRedo` | Whether `Redo()` can be performed. |
| `bool UndoEnabled` | When `false`, changes are not recorded. Default: `true`. |
| `void ClearUndo()` | Clears the undo/redo history. |
| `void BeginActions()` / `void EndActions()` | Low-level versions of `ActionScope`. Prefer `ActionScope`. |
| `TList List` | The inner collection. **Read only from it.** See [Notes](#notes). |

## Notes

- **Only changes to the collection are recorded.** Changes to the properties of an element (e.g. `list[0].Name = "x"`) are not recorded. To make such a change undoable, replace the element: `list[0] = newItem`.
- **Don't modify `List` directly.** Changes made through the inner collection are not recorded and make the history inconsistent.
- **`Clear()` on an empty list is not recorded.**
- **Not thread-safe.** Use it from one thread (e.g. the UI thread), or synchronize access yourself.
- If an exception is thrown inside an `ActionScope`, the changes made before the exception are recorded as one undo step.

## How it works

Each change is recorded as an action that knows how to undo and redo itself (add ⇔ remove, replace ⇔ replace back). Actions are stored in a ring buffer of size `maximumUndoTimes`, so the history uses a fixed amount of memory, and the oldest actions are overwritten when it is full.

The class diagram ([PlantUML](https://plantuml.com) sources are in [Shos.UndoRedoList/Documents/ClassDiagrams](https://github.com/Fujiwo/Shos.UndoRedoList/tree/master/Shos.UndoRedoList/Documents/ClassDiagrams)):

![Shos.UndoRedoList class diagram](https://raw.githubusercontent.com/Fujiwo/Shos.UndoRedoList/master/Shos.UndoRedoList/Documents/ClassDiagrams/UndoRedoList.all.small.png "Shos.UndoRedoList class diagram")

[Larger class diagram image is here.](https://raw.githubusercontent.com/Fujiwo/Shos.UndoRedoList/master/Shos.UndoRedoList/Documents/ClassDiagrams/UndoRedoList.all.png)

## Repository structure

| Project | Description |
|---|---|
| [Shos.UndoRedoList](https://github.com/Fujiwo/Shos.UndoRedoList/tree/master/Shos.UndoRedoList) | The library. |
| [Shos.UndoRedoList.Tests](https://github.com/Fujiwo/Shos.UndoRedoList/tree/master/Shos.UndoRedoList.Tests) | Unit tests (MSTest). |
| [Shos.UndoRedoList.SampleApp](https://github.com/Fujiwo/Shos.UndoRedoList/tree/master/Shos.UndoRedoList.SampleApp) | A WPF sample app using `UndoRedoObservableCollection<T>`. |
| [Shos.UndoRedoList.PerformanceTests](https://github.com/Fujiwo/Shos.UndoRedoList/tree/master/Shos.UndoRedoList.PerformanceTests) | A console app that runs many operations. |

To build and test (requires the .NET 10 SDK, plus the .NET 8 runtime to run the tests for .NET 8.0):

```console
dotnet build
dotnet test Shos.UndoRedoList.Tests
```

## Release notes

### 1.0.6

- Added support for .NET 10.0 (targets .NET 8.0 and .NET 10.0).
- Fixed: after undoing all changes, a new change didn't discard the undone changes, so they could still be undone/redone and corrupted the list.
- Fixed: after the undo history reached `maximumUndoTimes`, undoing all changes and then redoing redid the wrong change.
- Fixed: a change that failed with an exception (e.g. `Insert` with an invalid index) was recorded in the history.
- Fixed: `CanUndo` / `CanRedo` were out of date in `CollectionChanged` handlers during `Undo()`.
- Fixed: `ModuloArithmetic.Equals(object)` threw an exception for `null` or other types.
- Changed: `Clear()` on an empty list is no longer recorded.
- Changed: invalid sizes now throw `ArgumentOutOfRangeException` with the parameter name.
- The NuGet package now includes XML documentation (IntelliSense).

### 1.0.5 and earlier

See the [commit history](https://github.com/Fujiwo/Shos.UndoRedoList/commits/master).

## Related articles

* [[C#][.NET] Shos.UndoRedoList (List and ObservableCollection which support undo/redo) | Programming C# | Sho's Software](https://wp.shos.info/2020/12/09/c-net-shos-undoredolist-list-and-observablecollection-which-support-undo-redo/)

## Author info

Fujio Kojima: a software developer in Japan

* Microsoft MVP for Development Tools - Visual C# (Jul. 2005 - Dec. 2014)
* Microsoft MVP for .NET (Jan. 2015 - Oct. 2015)
* Microsoft MVP for Visual Studio and Development Technologies (Nov. 2015 - Jun. 2018)
* Microsoft MVP for Developer Technologies (Nov. 2018 - Jun. 2025)
* [MVP Profile](https://mvp.microsoft.com/en-US/mvp/profile/4185d172-3c9a-e411-93f2-9cb65495d3c4)
* [Blog (Japanese)](http://wp.shos.info)
* [Web Site (Japanese)](http://www.shos.info)
* [Twitter](https://twitter.com/Fujiwo)
* [Instagram](https://www.instagram.com/fujiwo/)

## License

This library is under the [MIT License](https://github.com/Fujiwo/Shos.UndoRedoList/blob/master/LICENSE).
