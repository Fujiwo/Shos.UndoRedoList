# Shos.UndoRedoList リポジトリ レビュー

- レビュー日: 2026-10-09
- 対象: リポジトリ全体 (ライブラリ本体 / テスト / パフォーマンステスト / WPF サンプル / ビルド設定 / ドキュメント)
- 対象バージョン: 1.0.5 (`Shos.UndoRedoList.csproj`)

---

## 0. サマリー

全体として小さく読みやすい設計で、`IList<T>` をラップして Undo/Redo を付与し、リングバッファで履歴数を制限するという方針は明快です。`ActionScope` / `DisabledUndoScope` による `using` ベースの API も使いやすいものになっています。

ただし、**Undo/Redo 履歴の管理に正しさに関わるバグが 2 件**あり、通常の操作手順 (「すべて Undo してから新しい編集をする」「履歴上限を超えた後に Undo → Redo する」) で、**リストの内容が誤った状態になります**。さらに、既存のユニットテストの一部がこの誤った挙動を期待値として固定しています。最優先で修正することを推奨します。

| 重大度 | 件数 | 主な内容 |
|---|---|---|
| 🔴 High | 2 | 全 Undo 後の新規操作で Redo 履歴が破棄されない / 履歴が一周した後の Redo が誤った操作を実行する |
| 🟠 Medium | 6 | 例外発生時に Undo 履歴が壊れる、`Equals(object)` が例外を投げる、`default(ModuloArithmetic)` が使えない、など |
| 🟡 Low | 多数 | API 設計、命名、ビルド設定、テストの質、ドキュメントの古さ、など |

### 検証方法

- すべてのソースを精読しました。
- ライブラリのソースを作業用ディレクトリにコピーし、.NET SDK 10 (`net10.0`) で再現用プログラムを作成して、下記の各指摘を**実際に再現**しました。
- 既存テスト 37 件はすべて成功しました (`net10.0` で実行)。
- 🔴 の 2 件に対する修正案 (後述) を適用したところ、再現ケースはすべて正しく動作しました。既存テストのうち失敗したのは 1 件 (`UndoRedoRingBufferTest.Test`) のみで、これはバグのある挙動を期待値としているテストです。

> **補足 (環境)**: この作業ツリーは Dropbox の「オンラインのみ」ファイル (プレースホルダー) になっており、WSL から直接読むと全ファイルが 0x00 で埋まって見えました。`.git` 配下も同じ状態のため、WSL 上では `git` コマンドが動作しません。Windows 側でファイルを読み込んで内容を取得し、レビューしました。開発で使う場合は、Dropbox でこのフォルダーを「オフラインで使用可能」に設定することを推奨します。

---

## 1. 🔴 High — 正しさに関わるバグ

### H-1. すべて Undo した後に新しい操作をしても、Redo 履歴が破棄されない

**場所**: `Shos.UndoRedoList/UndoRedoRingBuffer.cs:17-23`

```csharp
public override void Add(TElement element)
{
    if (CurrentIndex.Next != TopIndex)
        RemoveAfter(CurrentIndex.Next);
    base.Add(element);
    CurrentIndex = BottomIndex;
}
```

`CurrentIndex` が無効値 (`Value == -1`、すなわち「すべて Undo 済み」) のとき、`CurrentIndex.Next` は `(−1 + 1) % Divisor = 0` になります。このとき処理は次のように分岐し、どちらも誤りです。

- `TopIndex == 0` の場合 (履歴がまだ一周していない): `Next == TopIndex` と判定されて `RemoveAfter` が呼ばれず、**取り消し済みのアクションがバッファに残ります**。
- `TopIndex != 0` の場合 (一周後): `RemoveAfter(0)` が呼ばれ、`BottomIndex = 0.Previous` という無関係な位置に設定されます。

その結果、取り消し済みのアクションが「Undo 可能」なまま残ります。

**再現 (実測)**:

```csharp
var a = new UndoRedoList<int>();
a.Add(1); a.Add(2);
a.Undo(); a.Undo();       // [] (すべて Undo)
a.Add(3);                 // [3]
while (a.Undo()) ;        // 期待: Undo 1 回で []   実際: Undo が 3 回成功する
while (a.Redo()) ;        // 期待: [3]              実際: [3, 1, 2]  ← 内容が壊れる
```

上限 3 の `UndoRedoList<int>(3)` で一周させた後に同じ操作をした場合も、期待値は Undo 1 回ですが、実際には 3 回成功しました。

一般的なエディターの「すべて元に戻す → 新しく編集」という操作で発生するため、影響が大きいバグです。

### H-2. 履歴が一周した後、全 Undo から Redo すると誤ったアクションが実行される

**場所**: `Shos.UndoRedoList/UndoRedoRingBuffer.cs:42-48`

```csharp
public bool GoForward()
{
    if (!CanGoForward)
        return false;
    CurrentIndex++;          // 無効値(-1) → 0 になる。正しくは TopIndex
    return true;
}
```

無効値からの `++` は常にインデックス 0 に移動します。しかし、リングバッファが一周した後は最古の要素が `TopIndex` (≠ 0) にあるため、**最古ではない別のアクションを Redo してしまいます**。

**再現 (実測)**:

```csharp
var b = new UndoRedoList<int>(maximumUndoTimes: 2);
b.Add(100); b.Add(200); b.Add(300);
b.Undo(); b.Undo();       // [100]
b.Redo();                 // 期待: [100, 200]、CanRedo == true
                          // 実際: [100, 300]、CanRedo == false
```

`UndoRedoListTest.MaximumUndoTimes` (`Shos.UndoRedoList.Tests/UndoRedoListTest.cs:160-165`) で後半のアサーションがコメントアウトされているのは、この問題に当たったためと思われます。

### 修正案 (H-1 / H-2 共通)

「無効な `CurrentIndex` は *TopIndex の直前* を意味する」という前提を明示的に扱います。

```csharp
public override void Add(TElement element)
{
    if (!CurrentIndex.IsValid)
        Clear();                                  // すべて Undo 済み → 履歴を全破棄
    else if (CurrentIndex != BottomIndex)
        RemoveAfter(CurrentIndex.Next);           // Current より後ろの Redo 履歴を破棄
    base.Add(element);
    CurrentIndex = BottomIndex;
}

public bool GoForward()
{
    if (!CanGoForward)
        return false;
    CurrentIndex = CurrentIndex.IsValid ? CurrentIndex.Next : TopIndex;
    return true;
}
```

この修正で上記の再現ケースはすべて期待どおりに動作しました。あわせて、以下の対応も必要です。

- `UndoRedoRingBufferTest.Test` の 91〜98 行目は、バグのある挙動 (`Count == 3`) を期待値にしているため、`Count == 1`、`TopIndex == BottomIndex == CurrentIndex` に修正する。
- `UndoRedoListTest.MaximumUndoTimes` のコメントアウトを解除する。
- 上記の 2 つの再現シナリオを回帰テストとして追加する。

---

## 2. 🟠 Medium

### M-1. `Insert` で例外が発生しても Undo 履歴が記録されてしまう

**場所**: `UndoRedoList.cs:232-236` (および `Add(TElement)` 211-215 行)

```csharp
public void Insert(int index, TElement element)
{
    Add(new AddAction(...));   // 先に履歴を記録
    List.Insert(index, element); // ここで例外 → 履歴だけが残る
}
```

`Insert(-1, x)` を呼ぶと `ArgumentOutOfRangeException` が発生しますが、実行されていないアクションが履歴に積まれます。次の `Undo()` は、何も起きないのに `true` を返して 1 ステップ消費します (実測)。`TList` が読み取り専用の場合の `Add` も同様です。

**修正案**: 「実際の操作 → 成功したら記録」の順にします。`RemoveAt` と `this[index]` の set は、先に `List[index]` を読むため範囲外では記録前に例外になりますが、順序を統一しておくほうが安全です。

### M-2. `ModuloArithmetic.Equals(object)` が例外を投げる

**場所**: `ModuloArithmetic.cs:43`

```csharp
public override bool Equals(object item) => Value.Equals(((ModuloArithmetic)item).Value);
```

- `Equals(null)` を呼ぶと `NullReferenceException` が発生します (実測)。
- `Equals("x")` を呼ぶと `InvalidCastException` が発生します (実測)。

`Equals(object)` は例外を投げずに `false` を返すのが .NET の規約です。`ArrayList.Contains`、`Assert.AreEqual` (型が異なる場合) などで問題になります。

```csharp
public override bool Equals(object obj) => obj is ModuloArithmetic other && Equals(other);
```

### M-3. 等価性が `Divisor` を無視しており、`operator -` と一貫していない

`new ModuloArithmetic(3) == new ModuloArithmetic(5)` は `true` になります (テスト `Construct` でもこれを期待値としています)。一方で `operator -` は、`Divisor` が異なると `InvalidOperationException` を投げます。「等しいのに引き算できない」状態になり、型の意味が曖昧です。`Equals` / `GetHashCode` / `==` に `Divisor` を含めるか、少なくとも仕様として XML ドキュメントに明記してください。

### M-4. `default(ModuloArithmetic)` / `new ModuloArithmetic()` が壊れた値になる

構造体の `new ModuloArithmetic()` は、省略可能引数を持つコンストラクターではなく既定値を生成するため、`Divisor == 0` の値ができます。この値では `Next` / `Previous` / `++` / `Value` の set がすべて例外になります (実測)。テスト `ModuloArithmeticTest.MinusValue` は `new ModuloArithmetic { Value = -1 }` を使っており、意図とは**別の理由** (`Divisor == 0`) で例外になって成功しています。

**提案**:

- `readonly struct` にして、ミューテーター (`MoveNext` / `MovePrevious` / `Value` の set) を廃止する。現状は可変構造体なので、プロパティ経由で `x.TopIndex.MoveNext()` と書いてもコピーが変更されるだけで、元の値は変わりません。
- もしくは、`Divisor == 0` を「未初期化」として検出し、明示的に例外を投げる。
- そもそも、この型を `public` で公開する必要があるか検討する (L-1 参照)。

### M-5. `RingBuffer` のコンストラクターが負のサイズで `OverflowException` を投げる

**場所**: `RingBuffer.cs:26-31`

`new TElement[size]` が `ModuloArithmetic` による検証より先に実行されるため、`new UndoRedoList<int>(-1)` は、XML ドキュメントに記載された `ArgumentOutOfRangeException` ではなく `OverflowException` を投げます (実測)。検証をコンストラクターの先頭に移し、`ArgumentOutOfRangeException.ThrowIfLessThan(size, 2)` のように `paramName` 付きで投げることを推奨します。ほかの `throw new ArgumentOutOfRangeException()` / `InvalidOperationException()` も、すべて引数名やメッセージがありません。

### M-6. 内部の `List` を外部から直接変更できる

`public TList List { get; }` が公開されているため、利用者が `undoRedoList.List.Add(x)` のように直接変更すると、記録済みアクションのインデックスがずれて、Undo/Redo の結果が不正になります。`Action.Add` / `Remove` / `Exchange` は `IsValidIndex` が偽のとき**何もせずに終了する**ため、不整合が表に出ずに内容が少しずつ壊れていきます。

**提案**: `List` を `IReadOnlyList<TElement>` として公開するか、少なくとも XML ドキュメントで「直接変更しないこと」を明記してください。また、整合性が取れない状態では、黙って無視せず `Debug.Assert` や例外で検出することを推奨します。

---

## 3. 🟡 Low — 設計・API

| # | 場所 | 指摘 |
|---|---|---|
| L-1 | `EnumerableExtensions`, `ModuloArithmetic`, `RingBuffer`, `UndoRedoRingBuffer` | 実装の詳細がすべて `public` になっています。特に `EnumerableExtensions.ForEach` は `Shos.Collections` 名前空間を `using` した利用者のすべての `IEnumerable<T>` に表示され、他のライブラリの `ForEach` と競合します。`internal` にすることを推奨します (破壊的変更なのでメジャーバージョンアップ時に)。 |
| L-2 | `UndoRedoList.cs:13` | 入れ子クラス名 `Action` が `System.Action` と衝突しており、読み手が混乱します。`UndoableAction` / `ICommand` 系の名前を推奨します。 |
| L-3 | `UndoRedoList.cs:217-224` | 空のリストに対する `Clear()` でも空の `ActionCollection` が積まれ、何も起きない Undo ステップができます (実測で `CanUndo == true`)。`Count == 0` のときは記録しないほうが自然です。 |
| L-4 | `UndoRedoList.cs:172-177` | `ActionScope` はネストできず、`InvalidOperationException` が発生します (実測)。ネスト深度のカウンターを導入すると、メソッドの合成がしやすくなります。少なくともドキュメントに明記してください。 |
| L-5 | `UndoRedoList.cs:112` | `ActionScope` 内で例外が発生した場合、`Dispose` で途中までのアクションが 1 ステップとして確定します。「ロールバックする」「そのまま確定する」のどちらが望ましいかを決めて、仕様として明記してください。 |
| L-6 | `UndoRedoList` | `CanUndo` / `CanRedo` の変化を通知する仕組み (`INotifyPropertyChanged` やイベント) がありません。そのため、サンプルアプリでは `CollectionChanged` で代用していますが、`ClearUndo()` や `DisabledUndoScope` 中の変更ではボタン状態が更新されないケースがあります。 |
| L-7 | `UndoRedoObservableCollection` | `INotifyPropertyChanged` (`Count`, `Item[]`) を中継していません。`ObservableCollection.Move` 相当の操作もありません。 |
| L-8 | `RingBuffer.cs` | `Clear` / `Remove` / `RemoveAfter` で配列スロットをクリアしないため、Undo 履歴から外れたアクション (とそれが保持する要素) が GC されずに残ります。大きなオブジェクトを扱う場合はメモリリークになります。 |
| L-9 | `RingBuffer.cs:20-23` | インデクサーが範囲外 (`TopIndex`〜`BottomIndex` の外) のスロットや無効値 (`-1` → `IndexOutOfRangeException`) を検証しません。 |
| L-10 | `RingBuffer` | `Count` を持っているので、`IReadOnlyCollection<T>` を実装できます。 |
| L-11 | 全体 | Nullable 参照型が無効です。`<Nullable>enable</Nullable>` を推奨します。 |
| L-12 | 全体 | スレッドセーフではありません (一般的な仕様なので問題ありませんが、XML ドキュメントに明記を推奨します)。 |
| L-13 | `UndoRedoList.cs:133` | `hasBeganStoringAction` は英語として誤り (`hasBegun...`) です。`ActionCollection.actions` は `readonly` にできます。`Action` の `Container` / `Element` / `Index` の setter も不要です。 |
| L-14 | `ModuloArithmetic.cs:28` | `Value + Divisor - 1` は `Value == 0` の分岐内なので、`Divisor - 1` と書けます。 |
| L-15 | `ModuloArithmetic.cs:49` | `InvalidItem` はインスタンスプロパティになっていますが、`Divisor` しか使っていないので、命名 (`AsInvalid()` など) を見直すと意図が伝わりやすくなります。 |

---

## 4. テスト

| # | 指摘 |
|---|---|
| T-1 | **バグを期待値として固定しているテストがあります**: `UndoRedoRingBufferTest.Test` 91〜98 行 (H-1 参照)。 |
| T-2 | **コメントアウトされたアサーション**: `UndoRedoListTest.MaximumUndoTimes` 160〜165 行 (H-2 参照)。失敗するテストは、無効化せずに `[Ignore("理由")]` とイシューで管理することを推奨します。 |
| T-3 | `RingBufferTest.ForLoop` は `count >= 0` で `Count + 1` 回ループしており、最後の 1 回は先頭要素を再検査しているだけです (オフバイワン)。 |
| T-4 | `RingBufferTest.ForEachLoop` は列挙される `element` を一度も検証していません。`CollectionAssert.AreEqual(new[] {200, 400, 500}, target.ToArray())` のようにすべきです。 |
| T-5 | `ModuloArithmeticTest.MinusValue` は意図しない理由で成功しています (M-4 参照)。 |
| T-6 | 以下のテストが不足しています: Undo 後の新規操作 (H-1)、一周後の Redo (H-2)、例外発生時の履歴 (M-1)、`Insert` / `Remove(T)` / インデクサーの Undo/Redo、`ActionScope` 内での `Clear`、`UndoRedoObservableCollection` の通知内容 (`Action` 種別・インデックス)。 |
| T-7 | `ExpectedException` 属性は MSTest v3 系で非推奨、v4 で削除されています。`Assert.ThrowsException<T>` (v4 では `Assert.ThrowsExactly<T>`) への移行を推奨します。 |
| T-8 | 変数名 `dividor` → `divisor`、テスト名 `InitializeWidthOne` → `InitializeWithOne` の typo。 |
| T-9 | ランダムな操作列で `List<T>` のスナップショットと照合するプロパティベーステスト (例: FsCheck) があれば、H-1 / H-2 は早期に発見できたはずです。リングバッファのような境界の多いコードには効果的です。 |

---

## 5. パフォーマンステスト (`Shos.UndoRedoList.PerformanceTests`)

- 計測をしていません (`Stopwatch` も BenchmarkDotNet もない)。名前に反して、実行するだけのプログラムになっています。BenchmarkDotNet の導入を推奨します。
- 既定の `maximumUndoTimes` (100) のまま 100 万回 Undo/Redo しているため、実際に効いているのは 100 回だけで、残りはすべて `false` で即座に返っています。計測の意味が薄くなっています。
- `static Random random = new Random();` はシードが固定されていないため、再現性がありません。

---

## 6. サンプルアプリ (`Shos.UndoRedoList.SampleApp`)

| # | 指摘 |
|---|---|
| S-1 | `StaffList.Add` は `new` で基底クラスのメソッドを隠蔽しています。`IList<Staff>` や基底型経由で呼ばれると、名前の検証と ID の採番がバイパスされます。 |
| S-2 | `StaffList.staffId` が `static` なので、複数のインスタンス間で採番状態が共有されます。 |
| S-3 | `MainWindow.xaml` の 2 行目が `Height="auto"` なので、項目が増えても `ListBox` にスクロールバーが出ず、ウィンドウからはみ出します。`Height="*"` にしてください。 |
| S-4 | `Add` コマンドは追加後に `NewStaffName` をクリアしません。また、空の名前では何も起きず、何のフィードバックもありません。 |
| S-5 | `MainViewModel` に保存 / 読み込みのコードがコメントアウトされたまま残っています。不要なら削除し、必要ならイシューで管理してください。 |
| S-6 | 作業ツリーに `*_wpftmp.csproj` (`netcoreapp3.1` 時代の一時ファイル) が 2 つ残っています。`.gitignore` の対象なので Git には含まれていないはずですが、ローカルから削除して問題ありません。 |

---

## 7. ビルド・パッケージング

| # | 場所 | 指摘 |
|---|---|---|
| B-1 | `Directory.Build.props` と各 `.csproj` | `Roslynator.Analyzers` を両方で参照しており重複しています (NuGet の警告 NU1504 の原因)。`Directory.Build.props` に一本化してください。 |
| B-2 | `Directory.Build.props` | `AnalysisLevel` が `latest-Minimum` で、ほぼ何も検出しません。少なくとも `latest-Recommended` と `TreatWarningsAsErrors` (または CI でのみ有効化) を推奨します。 |
| B-3 | `Shos.UndoRedoList.csproj` | `GenerateDocumentationFile` が無効なので、丁寧に書かれた XML ドキュメントコメントが NuGet パッケージに含まれず、利用者の IntelliSense に表示されません。 |
| B-4 | `Shos.UndoRedoList.csproj` | `TargetFramework` が `net8.0` のみです。依存のない純粋なコレクションライブラリなので、`netstandard2.0` を追加すれば .NET Framework / Unity / Xamarin などからも使えるようになります (`=>` で書かれたタプル代入などは問題ありません)。逆に .NET 8 専用にするなら、今後のサポート期限 (.NET 8 は 2026 年 11 月に EOL) を踏まえて、`net8.0;net10.0` へのマルチターゲットを検討してください。 |
| B-5 | `Shos.UndoRedoList.csproj` | SourceLink、`IncludeSymbols` / `SymbolPackageFormat=snupkg`、`ContinuousIntegrationBuild`、`Deterministic` が未設定です。 |
| B-6 | `Shos.UndoRedoList.csproj` | `GeneratePackageOnBuild=true` のため、毎回のビルドで `.nupkg` が生成されます。CI の `dotnet pack` に任せるほうが一般的です。 |
| B-7 | 各 `.csproj` | `PackageTags` が `ovservablecollection` と typo しています。メタデータ (Authors, Copyright 等) が 4 つのプロジェクトで重複しているので、`Directory.Build.props` に集約できます。Copyright の年 (`2020-2024`) も更新が必要です。 |
| B-8 | `Shos.UndoRedoList.csproj:22` | `<Folder Include="Documents\ClassDiagrams\" />` は不要です。 |
| B-9 | `.vscode/launch.json` | `program` が `bin/Debug/netcoreapp3.1/...` を指しており、現在の `net8.0` では起動できません。 |
| B-10 | リポジトリ | CI (GitHub Actions など) がありません。ビルド・テスト・パックを自動化すれば、T-1 / T-2 のような状態に気付きやすくなります。 |
| B-11 | テストプロジェクト | `Microsoft.NET.Test.Sdk 17.9.0`、`MSTest 3.2.2`、`coverlet 6.0.2`、サンプルの `Prism.Core 8.1.97` などが古いので更新を推奨します (Dependabot / Renovate の導入も検討してください)。 |

---

## 8. ドキュメント

| # | 指摘 |
|---|---|
| D-1 | ~~README の画像リンクが `.../master/...` を指している~~ → **撤回**: 既定ブランチは `master` なので、リンクは正しいものでした (ファイルを読み込めるようになった後に確認)。 |
| D-2 | README のサンプルコードは、テストコードをそのまま貼り付けたものです。`[TestClass]` などのノイズが多いので、利用者向けの最小限のコード例 (例: WPF の ViewModel での使い方) を推奨します。 |
| D-3 | README で `UndoRedoRingBuffer<TElement>` だけバッククォートがなく、表記が揃っていません。`do domething` の typo (`SampleTest.cs:125` と README) もあります。 |
| D-4 | README に CHANGELOG / リリースノートがありません。H-1 / H-2 を修正したときには、挙動の変更点を明記してください。 |
| D-5 | README / XML ドキュメントに、次の仕様が書かれていません: `maximumUndoTimes` の最小値が 2 であること、`ActionScope` がネストできないこと、内部の `List` を直接変更してはいけないこと、スレッドセーフではないこと。 |
| D-6 | PlantUML のクラス図がコードと同期しているかを、自動で検証する仕組みがありません (将来の API 変更で古くなりがちです)。 |

---

## 9. 推奨する対応の優先順位

1. **H-1 / H-2 の修正**と、T-1 / T-2 のテスト修正、回帰テストの追加。パッチリリース (1.0.6) で出すことを推奨します。
2. **M-1 (例外発生時の履歴)、M-2 (`Equals(object)`)、M-5 (例外の型)** の修正。いずれも小さな変更で済みます。
3. **B-3 (XML ドキュメントのパッケージへの同梱)、B-1 (重複参照)、B-10 (CI)** の対応。
4. 次のメジャーバージョンで、**L-1 (実装詳細の非公開化)、M-4 (`readonly struct`)、M-6 (`List` の公開範囲)、L-6 (`CanUndo` / `CanRedo` の変更通知)** を検討する。

---

## 10. 対応状況 (ブランチ `fix/review-findings`、未コミット)

| 指摘 | 状態 | 内容 |
|---|---|---|
| H-1 / H-2 | ✅ 修正 | `UndoRedoRingBuffer.Add` / `GoForward` で無効な `CurrentIndex` (= `TopIndex` の直前) を正しく扱うようにした。 |
| M-1 | ✅ 修正 | 引数を**履歴に記録する前に**検証するようにした (`Insert` のインデックス、読み取り専用の場合の `NotSupportedException`)。※「操作してから記録する」順序にすると、`UndoRedoObservableCollection` の `CollectionChanged` ハンドラー内で `CanUndo` が古い値になり、サンプルアプリの Undo ボタンが有効にならない。そのため、記録してから操作する順序は維持した。 |
| M-2 | ✅ 修正 | `Equals(object)` が `null` や別の型に対して `false` を返すようにした。 |
| M-5 | ✅ 修正 | `RingBuffer` のコンストラクターで、サイズを先に検証するようにした。例外に `paramName` とメッセージを付けた。 |
| M-3 / M-4 / M-6 | ⏸ 保留 | 動作や API の互換性に影響するため、次のメジャーバージョンで検討する。 |
| L-3 | ✅ 修正 | 空のリストに対する `Clear()` は履歴に記録しないようにした。 |
| T-1 / T-2 / T-3 / T-4 / T-5 / T-8 | ✅ 修正 | 誤った期待値の修正、コメントアウトの解除、オフバイワン、列挙内容の検証、テスト名・変数名の typo。 |
| T-6 | ✅ 一部 | 回帰テストを 10 件追加した (全 Undo 後の追加、一周後の Redo、失敗した操作が記録されないこと、`Insert` / `Remove` / インデクサー、`Equals(object)`、負のサイズ、`CollectionChanged` 内での `CanUndo`)。 |
| B-1 / B-3 / B-7 / B-8 / B-9 | ✅ 修正 | Roslynator の参照を `Directory.Build.props` に一本化した。XML ドキュメントをパッケージに同梱するようにした (あわせて、不正な XML コメント (CS1570 / CS1584) と `<summary>` の欠落を修正した)。`PackageTags` の typo、不要な `<Folder>`、`launch.json` のパスも修正した。 |
| D-3 | ✅ 修正 | README の表記揺れと typo (`do domething`) を修正した。 |
| その他 | ⏸ 未着手 | L-1〜L-15 (L-3 を除く)、T-7、T-9、パフォーマンステスト、サンプルアプリ、B-2 / B-4 / B-5 / B-6 / B-10 / B-11、D-2 / D-4〜D-6。 |

検証: テスト 47 件がすべて成功 (net8.0 / ロールフォワードで .NET 10 ランタイム上で実行)。修正前の `UndoRedoRingBuffer.cs` に戻すと、回帰テストを含む 6 件が失敗することを確認した。

## 付録: 再現結果 (修正前)

```text
1) after undo-all + Add(3): [3] CanUndo=True CanRedo=False
   undo count=3 list=[]
   redo all -> [3,1,2] (expected [3])
2) wrap: after 2 undos [100]
   after 1 redo [100,300] (expected [100,200]) CanRedo=False (expected True)
3) Insert(-1): ArgumentOutOfRangeException
   Undo()=True list=[1] (expected list [] after one undo)
4a) Equals(null): NullReferenceException
4b) Equals("x"): InvalidCastException
4c) Divisor ignored: new(3)==new(5) -> True
5) new ModuloArithmetic().Next: ArgumentOutOfRangeException
6) new UndoRedoList<int>(-1): OverflowException
7) empty Clear -> CanUndo=True
8) nested ActionScope: InvalidOperationException
9) wrap (max 3, 4 adds) undo-all then Add(9): undo count=3 (expected 1)
```

修正案の適用後は、1) が `undo count=1` / `[3]`、2) が `[100,200]` / `CanRedo=True`、9) が `undo count=1` となり、期待どおりに動作しました。
