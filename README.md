# Shos.UndoRedoList

List and [ObservableCollection](https://docs.microsoft.com/dotnet/api/system.collections.objectmodel.observablecollection-1) which support undo/redo.

## NuGet

You can install Shos.UndoRedoList to your project with [NuGet](https://www.nuget.org) on Visual Studio.

* [NuGet Gallery | Shos.UndoRedoList](https://www.nuget.org/packages/Shos.UndoRedoList/)

### Package Manager

    PM>Install-Package Shos.UndoRedoList -version 1.0.2

### .NET CLI

    >dotnet add package Shos.UndoRedoList --version 1.0.2

### PackageReference

    <PackageReference Include="Shos.UndoRedoList" Version="1.0.2" />

## Projects

* [Shos.UndoRedoList](Shos.UndoRedoList)

Types for collection which supports undo/redo.

* [Shos.UndoRedoList.Tests](Shos.UndoRedoList.Tests)

Tests for Shos.UndoRedoList.

* [Shos.UndoRedoList.PerformanceTests](Shos.UndoRedoList.PerformanceTests)

Performance tests for Shos.UndoRedoList.

* [Shos.UndoRedoList.SampleApp](Shos.UndoRedoList.SampleApp)

Sample WPF app for UndoRedoObservableCollection.

![Shos.UndoRedoList.SampleApp](https://raw.githubusercontent.com/Fujiwo/Shos.UndoRedoList/master/Images/Shos.UndoRedoList.SampleApp.png "Shos.UndoRedoList.SampleApp")

## Types

### class [UndoRedoList<TElement, TList>](Shos.UndoRedoList/UndoRedoList.cs) : IList<TElement> where TList : IList<TElement>, new()

[IList](https://docs.microsoft.com/dotnet/api/system.collections.ilist) implemented collection which supports undo/redo.


### class [UndoRedoList<TElement>](Shos.UndoRedoList/UndoRedoList.cs) : IList<TElement> where TList : IList<TElement>, new()

[List](https://docs.microsoft.com/dotnet/api/system.collections.generic.list-1) implemented collection which supports undo/redo.

### class [UndoRedoObservableCollection<TElement>](Shos.UndoRedoList/UndoRedoObservableCollection.cs) : UndoRedoList<TElement, ObservableCollection<TElement>>, INotifyCollectionChanged

[ObservableCollection](https://docs.microsoft.com/dotnet/api/system.collections.objectmodel.observablecollection-1) which supports undo/redo.

### class [RingBuffer<TElement>](Shos.UndoRedoList/RingBuffer.cs) : IEnumerable<TElement>

[Circular buffer - Wikipedia](https://en.wikipedia.org/wiki/Circular_buffer)

> In computer science, a circular buffer, circular queue, cyclic buffer or ring buffer is a data structure that uses a single, fixed-size buffer as if it were connected end-to-end. This structure lends itself easily to buffering data streams.

### class [UndoRedoRingBuffer<TElement>](Shos.UndoRedoList/UndoRedoRingBuffer.cs) : RingBuffer<TElement>

Specialized RingBuffer for undo/redo.

### struct [ModuloArithmetic](Shos.UndoRedoList/ModuloArithmetic.cs) : IEquatable<ModuloArithmetic>

[Modular arithmetic - Wikipedia](https://en.wikipedia.org/wiki/Modular_arithmetic)

> In mathematics, modular arithmetic is a system of arithmetic for integers, where numbers "wrap around" when reaching a certain value, called the modulus.

### static class [EnumerableExtensions](Shos.UndoRedoList/EnumerableExtensions.cs)

Extension methods for IEnumerable.

### Class diagram

*.puml files in [Shos.UndoRedoList/Documents/ClassDiagrams](Shos.UndoRedoList/Documents/ClassDiagrams) are class diagram for [PlantUML](https://plantuml.com).

The following image is a class diagram made from these *.puml files.

![Shos.UndoRedoList class diagram](https://raw.githubusercontent.com/Fujiwo/Shos.UndoRedoList/master/Shos.UndoRedoList/Documents/ClassDiagrams/UndoRedoList.all.small.png "Shos.UndoRedoList class diagram")

[Larger class diagram image is here.](https://raw.githubusercontent.com/Fujiwo/Shos.UndoRedoList/master/Shos.UndoRedoList/Documents/ClassDiagrams/UndoRedoList.all.png)

## Samples

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Shos.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;

    namespace Shos.UndoRedoList.Tests
    {
        [TestClass]
        public class SampleTest
        {
            [TestMethod]
            public void UndoRedoTest()
            {
                // list which support undo/redo.
                var list = new UndoRedoList<int, List<int>>();

                Assert.IsFalse(list.CanUndo);
                Assert.IsFalse(list.CanRedo);

                // Modify list
                list.Add(100);
                Assert.AreEqual(1, list.Count);
                Assert.AreEqual(100, list[0]);
                Assert.IsTrue (list.CanUndo);
                Assert.IsFalse(list.CanRedo);

                // Undo
                Assert.IsTrue(list.Undo());

                Assert.AreEqual(0, list.Count);
                Assert.IsFalse(list.CanUndo);
                Assert.IsTrue (list.CanRedo);

                // Redo
                Assert.IsTrue(list.Redo());

                Assert.AreEqual(1, list.Count);
                Assert.AreEqual(100, list[0]);
                Assert.IsTrue (list.CanUndo);
                Assert.IsFalse(list.CanRedo);
            }

            [TestMethod]
            public void ActionScopeTest()
            {
                // list which support undo/redo.
                var list = new UndoRedoList<int, List<int>>();
                Assert.IsFalse(list.CanUndo);
                Assert.IsFalse(list.CanRedo);

                // ActionScope
                using (var scope = new UndoRedoList<int, List<int>>.ActionScope(list)) {
                    // Modify list in ActionScope
                    list.Add(100);
                    list.Add(200);
                    list.Add(300);
                }

                Assert.AreEqual(3, list.Count);
                Assert.AreEqual(100, list[0]);
                Assert.AreEqual(200, list[1]);
                Assert.AreEqual(300, list[2]);
                Assert.IsTrue(list.CanUndo);

                // Undo
                Assert.IsTrue(list.Undo());
                // The 3 actions in ActionScope can undo in one time.
                Assert.AreEqual(0, list.Count);
                Assert.IsFalse(list.CanUndo);
            }

            [TestMethod]
            public void DisabledUndoScopeTest()
            {
                // list which support undo/redo.
                var list = new UndoRedoList<int, List<int>>();
                Assert.IsFalse(list.CanUndo);
                Assert.IsFalse(list.CanRedo);

                // DisabledUndoScope
                using (var scope = new UndoRedoList<int, List<int>>.DisabledUndoScope(list)) {
                    // Modify list in DisabledUndoScope
                    list.Add(100);
                }

                // You can't undo actions in DisabledUndoScope.
                Assert.IsFalse(list.CanUndo);
                Assert.IsFalse(list.CanRedo);
            }

            [TestMethod]
            public void UndoRedoObservableCollectionTest()
            {
                // ObservableCollection which support undo/redo.
                var observableCollection = new UndoRedoObservableCollection<int>();
                observableCollection.CollectionChanged += ObservableCollection_CollectionChanged;

                observableCollection.Add(100);

                // You can undo/redo also.
                Assert.IsTrue (observableCollection.CanUndo);
                Assert.IsFalse(observableCollection.CanRedo);

                Assert.IsTrue(observableCollection.Undo());
                Assert.IsTrue(observableCollection.Redo());

                // event handler
                static void ObservableCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
                { /* do domething */ }
            }
        }
    }

See also: [Shos.UndoRedoList.SampleApp](Shos.UndoRedoList.SampleApp)

## Related Articles

* [[C#][.NET] Shos.UndoRedoList (List and ObservableCollection which support undo/redo) | Programming C# | Sho's Software](https://wp.shos.info/2020/12/09/c-net-shos-undoredolist-list-and-observablecollection-which-support-undo-redo/)

## Author Info

Fujio Kojima: a software developer in Japan

* Microsoft MVP for Development Tools - Visual C# (Jul. 2005 - Dec. 2014)
* Microsoft MVP for .NET (Jan. 2015 - Oct. 2015)
* Microsoft MVP for Visual Studio and Development Technologies (Nov. 2015 - Jun. 2018)
* Microsoft MVP for Developer Technologies (Nov. 2018 - Jun. 2021)
* [MVP Profile](https://mvp.microsoft.com/en-us/PublicProfile/21482)
* [Blog (Japanese)](http://wp.shos.info)
* [Web Site (Japanese)](http://www.shos.info)
* [Twitter](https://twitter.com/Fujiwo)
* [Instagram](https://www.instagram.com/fujiwo/)

## License

This library is under the MIT License.
