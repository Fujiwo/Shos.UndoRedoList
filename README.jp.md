# Shos.UndoRedoList

[![NuGet](https://img.shields.io/nuget/v/Shos.UndoRedoList.svg)](https://www.nuget.org/packages/Shos.UndoRedoList/)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/Fujiwo/Shos.UndoRedoList/blob/master/LICENSE)

[English](https://github.com/Fujiwo/Shos.UndoRedoList/blob/master/README.md) | **日本語**

**元に戻す (Undo) / やり直し (Redo)** ができる .NET 用の `List<T>` と `ObservableCollection<T>` です。

`List<T>` を `UndoRedoList<T>` に (あるいは `ObservableCollection<T>` を `UndoRedoObservableCollection<T>` に) 置き換えるだけで、`Add`、`Insert`、`Remove`、`RemoveAt`、`Clear`、インデクサーによる代入のすべてを Undo / Redo できるようになります。

```csharp
var list = new UndoRedoList<string> { "apple", "banana" };
list[0] = "avocado";   // avocado, banana
list.Undo();           // apple, banana
list.Redo();           // avocado, banana
```

## 特長

- **そのまま置き換え可能**: `IList<T>` を実装しているので、既存のコードはそのまま動きます。
- **すべての変更を Undo / Redo**: 追加、挿入、削除、全削除、インデクサーによる要素の置き換えが対象です。
- **複数の変更をまとめて Undo**: `ActionScope` 内で行った複数の変更は、**1 回**の Undo / Redo でまとめて元に戻せます。
- **記録しない変更**: `DisabledUndoScope` 内で行った変更は記録されません (初期データの読み込みなどに便利です)。
- **履歴数の上限**: Undo できる回数には上限 (既定値: 100) があるため、メモリ使用量が際限なく増えることはありません。
- **WPF / MVVM に対応**: `UndoRedoObservableCollection<T>` は `CollectionChanged` を発行するので、そのまま `ItemsSource` にバインドできます。
- **依存関係なし**: .NET の基本クラスライブラリだけに依存する小さなライブラリです。

## 対応フレームワーク

| パッケージのバージョン | 対象フレームワーク |
|---|---|
| 1.0.6 | .NET 8.0、.NET 10.0 |
| 1.0.5 | .NET 8.0 |

## インストール

NuGet から [Shos.UndoRedoList](https://www.nuget.org/packages/Shos.UndoRedoList/) をインストールしてください。

```console
dotnet add package Shos.UndoRedoList
```

Visual Studio のパッケージ マネージャー コンソールの場合:

```console
PM> Install-Package Shos.UndoRedoList
```

プロジェクト ファイルに直接書く場合:

```xml
<PackageReference Include="Shos.UndoRedoList" Version="1.0.6" />
```

すべての型は `Shos.Collections` 名前空間にあります。

```csharp
using Shos.Collections;
```

## クイック スタート

```csharp
using Shos.Collections;

var list = new UndoRedoList<string>();

list.Add("apple");
list.Add("banana");
list[0] = "avocado";
list.RemoveAt(1);
// list: avocado

list.Undo();   // list: avocado, banana   (RemoveAt を元に戻す)
list.Undo();   // list: apple, banana     (list[0] = ... を元に戻す)
list.Redo();   // list: avocado, banana   (list[0] = ... をやり直す)

Console.WriteLine(list.CanUndo); // True
Console.WriteLine(list.CanRedo); // True
```

元に戻す (やり直す) 操作がないとき、`Undo()` (`Redo()`) は何もせずに `false` を返します。Undo / Redo ボタンの有効・無効の切り替えには `CanUndo` / `CanRedo` を使ってください。

## 使い方

### 複数の変更をまとめて元に戻す: `ActionScope`

`ActionScope` 内で行った変更は、**1 回分**の Undo として記録されます。

```csharp
var numbers = new UndoRedoList<int> { 1, 2, 3 };

using (new UndoRedoList<int>.ActionScope(numbers)) {
    numbers.Add(4);
    numbers.RemoveAt(0);
    numbers[0] = 20;
}
// numbers: 20, 3, 4

numbers.Undo();
// numbers: 1, 2, 3   (3 つの変更がまとめて元に戻る)
```

> `ActionScope` は入れ子にできません。同じリストに対して `ActionScope` が有効な間に 2 つ目の `ActionScope` を開始すると、`InvalidOperationException` が発生します。

### 元に戻せない変更を行う: `DisabledUndoScope`

`DisabledUndoScope` 内で行った変更は記録されません。初期データの読み込みなどに使います。

```csharp
var items = new UndoRedoList<string>();

using (new UndoRedoList<string>.DisabledUndoScope(items)) {
    items.Add("loaded 1");
    items.Add("loaded 2");
}

Console.WriteLine(items.CanUndo); // False
```

`UndoEnabled` プロパティを直接設定することもできます。

> **注意**: 記録された操作は、変更した要素のインデックスを覚えています。すでに記録された変更が**ある状態で**記録しない変更を行った場合は、その後に `ClearUndo()` を呼んでください。呼ばないと、次の Undo が誤った位置に適用されることがあります。

### Undo できる回数を制限する

コンストラクターに `maximumUndoTimes` を渡します (既定値: 100、最小値: 2)。上限を超えると、最も古い操作から破棄されます。

```csharp
var list = new UndoRedoList<int>(maximumUndoTimes: 3);
for (var i = 1; i <= 5; i++)
    list.Add(i);

// 元に戻せるのは最後の 3 回の追加だけ
while (list.Undo())
    ;
// list: 1, 2
```

### Undo / Redo の履歴を消去する

```csharp
list.ClearUndo(); // CanUndo と CanRedo が false になります。要素は変わりません。
```

### 新しい変更を行うと Redo の履歴は破棄される

一般的なエディターと同じように、Undo した後に新しい変更を行うと、やり直せる操作は破棄されます。

```csharp
var list = new UndoRedoList<int> { 1, 2 };
list.Undo();      // list: 1        CanRedo: True
list.Add(3);      // list: 1, 3     CanRedo: False ("Add(2)" はもうやり直せない)
```

### WPF / MVVM: `UndoRedoObservableCollection<T>`

`UndoRedoObservableCollection<T>` は、`Undo()` や `Redo()` による変更も含めて、すべての変更で `CollectionChanged` を発行します。`ObservableCollection<T>` と同じように `ItemsSource` にバインドしてください。

`CollectionChanged` が発行された時点で `CanUndo` と `CanRedo` はすでに更新されているので、そこでコマンドの状態を更新できます。

```csharp
using Prism.Commands; // 任意の MVVM ライブラリを使えます (CommunityToolkit.Mvvm など)。
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

`ClearUndo()` の呼び出しや `UndoEnabled` の変更では `CollectionChanged` は発行されません。これらの後は、コマンドの状態を自分で更新してください。

WPF の完全なサンプルは [Shos.UndoRedoList.SampleApp](https://github.com/Fujiwo/Shos.UndoRedoList/tree/master/Shos.UndoRedoList.SampleApp) を参照してください。

![Shos.UndoRedoList.SampleApp](https://raw.githubusercontent.com/Fujiwo/Shos.UndoRedoList/master/Images/Shos.UndoRedoList.SampleApp.png "Shos.UndoRedoList.SampleApp")

### 内部のリストに別の `IList<T>` を使う: `UndoRedoList<TElement, TList>`

`UndoRedoList<T>` は内部で `List<T>` を使っています。別のコレクションを使いたい場合は、`TList` に指定してください。`IList<TElement>` を実装し、引数なしのコンストラクターを持つ型である必要があります。

```csharp
var list = new UndoRedoList<int, ObservableCollection<int>>();
list.Add(1);

ObservableCollection<int> inner = list.List; // 内部のコレクションを読み取る
```

## API

### 型

| 型 | 説明 |
|---|---|
| `UndoRedoList<TElement>` | Undo / Redo ができる `List<TElement>`。 |
| `UndoRedoObservableCollection<TElement>` | Undo / Redo ができる `ObservableCollection<TElement>`。`INotifyCollectionChanged` を実装しています。 |
| `UndoRedoList<TElement, TList>` | 基底クラス。任意の `TList : IList<TElement>, new()` に Undo / Redo の機能を付けます。 |
| `UndoRedoList<TElement, TList>.ActionScope` | `IDisposable`。スコープ内の変更を 1 回分の Undo にまとめます。 |
| `UndoRedoList<TElement, TList>.DisabledUndoScope` | `IDisposable`。スコープ内の変更を記録しません。 |

### `UndoRedoList<TElement, TList>` のメンバー

`IList<TElement>` のメンバーに加えて、次のメンバーがあります。

| メンバー | 説明 |
|---|---|
| `UndoRedoList(int maximumUndoTimes = 100)` | コンストラクター。`maximumUndoTimes` は Undo できる最大回数 (2 以上) です。 |
| `bool Undo()` | 直前の変更を元に戻します。元に戻す操作がなければ `false` を返します。 |
| `bool Redo()` | 直前に元に戻した変更をやり直します。やり直す操作がなければ `false` を返します。 |
| `bool CanUndo` | `Undo()` ができるかどうか。 |
| `bool CanRedo` | `Redo()` ができるかどうか。 |
| `bool UndoEnabled` | `false` の間は変更が記録されません。既定値は `true` です。 |
| `void ClearUndo()` | Undo / Redo の履歴を消去します。 |
| `void BeginActions()` / `void EndActions()` | `ActionScope` の低レベル版です。通常は `ActionScope` を使ってください。 |
| `TList List` | 内部のコレクション。**読み取り専用として使ってください。** [注意事項](#注意事項)を参照してください。 |

## 注意事項

- **記録されるのはコレクションへの変更だけです。** 要素のプロパティの変更 (`list[0].Name = "x"` など) は記録されません。このような変更を元に戻せるようにするには、要素自体を置き換えてください: `list[0] = newItem`
- **`List` を直接変更しないでください。** 内部のコレクションを通じた変更は記録されず、履歴との整合性が崩れます。
- **空のリストに対する `Clear()` は記録されません。**
- **スレッドセーフではありません。** 1 つのスレッド (UI スレッドなど) から使うか、自分で排他制御を行ってください。
- `ActionScope` 内で例外が発生した場合、例外の前に行われた変更は 1 回分の Undo として記録されます。

## しくみ

それぞれの変更は、自分自身を元に戻したりやり直したりできる「操作」として記録されます (追加 ⇔ 削除、置き換え ⇔ 元の要素への置き換え)。操作はサイズ `maximumUndoTimes` のリングバッファに格納されるため、履歴が使うメモリ量は一定で、いっぱいになると最も古い操作から上書きされます。

クラス図 ([PlantUML](https://plantuml.com) のソースは [Shos.UndoRedoList/Documents/ClassDiagrams](https://github.com/Fujiwo/Shos.UndoRedoList/tree/master/Shos.UndoRedoList/Documents/ClassDiagrams) にあります):

![Shos.UndoRedoList のクラス図](https://raw.githubusercontent.com/Fujiwo/Shos.UndoRedoList/master/Shos.UndoRedoList/Documents/ClassDiagrams/UndoRedoList.all.small.png "Shos.UndoRedoList のクラス図")

[大きなクラス図はこちら](https://raw.githubusercontent.com/Fujiwo/Shos.UndoRedoList/master/Shos.UndoRedoList/Documents/ClassDiagrams/UndoRedoList.all.png)

## リポジトリの構成

| プロジェクト | 説明 |
|---|---|
| [Shos.UndoRedoList](https://github.com/Fujiwo/Shos.UndoRedoList/tree/master/Shos.UndoRedoList) | ライブラリ本体。 |
| [Shos.UndoRedoList.Tests](https://github.com/Fujiwo/Shos.UndoRedoList/tree/master/Shos.UndoRedoList.Tests) | 単体テスト (MSTest)。 |
| [Shos.UndoRedoList.SampleApp](https://github.com/Fujiwo/Shos.UndoRedoList/tree/master/Shos.UndoRedoList.SampleApp) | `UndoRedoObservableCollection<T>` を使った WPF のサンプル アプリ。 |
| [Shos.UndoRedoList.PerformanceTests](https://github.com/Fujiwo/Shos.UndoRedoList/tree/master/Shos.UndoRedoList.PerformanceTests) | 大量の操作を実行するコンソール アプリ。 |

ビルドとテスト (.NET 10 SDK が必要です。.NET 8.0 向けのテストを実行するには .NET 8 ランタイムも必要です):

```console
dotnet build
dotnet test Shos.UndoRedoList.Tests
```

## リリース ノート

### 1.0.6

- .NET 10.0 に対応しました (.NET 8.0 と .NET 10.0 が対象)。
- 修正: すべての変更を元に戻した後に新しい変更を行っても、元に戻した変更が破棄されず、それらを Undo / Redo できてしまってリストの内容が壊れる問題。
- 修正: Undo の履歴が `maximumUndoTimes` に達した後、すべて元に戻してから Redo すると、誤った変更がやり直される問題。
- 修正: 例外で失敗した変更 (不正なインデックスでの `Insert` など) が履歴に記録される問題。
- 修正: `Undo()` の実行中、`CollectionChanged` ハンドラー内で `CanUndo` / `CanRedo` が古い値になっていた問題。
- 修正: `ModuloArithmetic.Equals(object)` が `null` や別の型に対して例外を投げる問題。
- 変更: 空のリストに対する `Clear()` を記録しないようにしました。
- 変更: 不正なサイズを指定したとき、引数名付きの `ArgumentOutOfRangeException` を投げるようにしました。
- NuGet パッケージに XML ドキュメント (IntelliSense 用) を含めるようにしました。

### 1.0.5 以前

[コミット履歴](https://github.com/Fujiwo/Shos.UndoRedoList/commits/master)を参照してください。

## 関連記事

* [[C#][.NET] Shos.UndoRedoList (List and ObservableCollection which support undo/redo) | Programming C# | Sho's Software](https://wp.shos.info/2020/12/09/c-net-shos-undoredolist-list-and-observablecollection-which-support-undo-redo/)

## 作者

Fujio Kojima: 日本のソフトウェア開発者

* Microsoft MVP for Development Tools - Visual C# (2005 年 7 月 - 2014 年 12 月)
* Microsoft MVP for .NET (2015 年 1 月 - 2015 年 10 月)
* Microsoft MVP for Visual Studio and Development Technologies (2015 年 11 月 - 2018 年 6 月)
* Microsoft MVP for Developer Technologies (2018 年 11 月 - 2025 年 6 月)
* [MVP プロフィール](https://mvp.microsoft.com/en-US/mvp/profile/4185d172-3c9a-e411-93f2-9cb65495d3c4)
* [ブログ](http://wp.shos.info)
* [Web サイト](http://www.shos.info)
* [Twitter](https://twitter.com/Fujiwo)
* [Instagram](https://www.instagram.com/fujiwo/)

## ライセンス

このライブラリは [MIT ライセンス](https://github.com/Fujiwo/Shos.UndoRedoList/blob/master/LICENSE)で提供されています。
