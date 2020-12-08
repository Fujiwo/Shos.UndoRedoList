# Shos.UndoRedoList

List and [ObservableCollection](https://docs.microsoft.com/dotnet/api/system.collections.objectmodel.observablecollection-1) which support undo/redo.

## Projects

* Shos.UndoRedoList

Types for undo/redo support.

* Shos.UndoRedoList.Tests

Tests for Shos.UndoRedoList.

* Shos.UndoRedoList.SampleApp

Sample WPF app for UndoRedoObservableCollection.

## Types

### class [UndoRedoList<TElement, TList>](Shos.UndoRedoList/UndoRedoList) : IList<TElement> where TList : IList<TElement>, new()

List which supports undo/redo.

### class [UndoRedoObservableCollection<TElement>](Shos.UndoRedoList/UndoRedoObservableCollection) : UndoRedoList<TElement, ObservableCollection<TElement>>, INotifyCollectionChanged

[ObservableCollection](https://docs.microsoft.com/dotnet/api/system.collections.objectmodel.observablecollection-1) which supports undo/redo.

### class [RingBuffer<TElement>](Shos.UndoRedoList/RingBuffer.cs) : IEnumerable<TElement>

[Circular buffer - Wikipedia](https://en.wikipedia.org/wiki/Circular_buffer)

> In computer science, a circular buffer, circular queue, cyclic buffer or ring buffer is a data structure that uses a single, fixed-size buffer as if it were connected end-to-end. This structure lends itself easily to buffering data streams.

### class [UndoRedoRingBuffer<TElement>](Shos.UndoRedoList/UndoRedoRingBuffer) : RingBuffer<TElement>

Specialized RingBuffer for undo/redo.

### struct [ModuloArithmetic](Shos.UndoRedoList/ModuloArithmetic.cs) : IEquatable<ModuloArithmetic>

[Modular arithmetic - Wikipedia](https://en.wikipedia.org/wiki/Modular_arithmetic)

> In mathematics, modular arithmetic is a system of arithmetic for integers, where numbers "wrap around" when reaching a certain value, called the modulus.

## Samples

See [Shos.UndoRedoList.SampleApp](Shos.UndoRedoList.SampleApp).

## Author Info

Fujio Kojima: a software developer in Japan

* Microsoft MVP for Development Tools - Visual C# (Jul. 2005 - Dec. 2014)
* Microsoft MVP for .NET (Jan. 2015 - Oct. 2015)
* Microsoft MVP for Visual Studio and Development Technologies (Nov. 2015 - Jun. 2018)
* Microsoft MVP for Developer Technologies (Nov. 2018 - Jun. 2021)
* [MVP Profile](https://mvp.microsoft.com/en-us/PublicProfile/21482)
* [Blog (Japanese)](http://wp.shos.info)
* [Web Site (Japanese)](http://www.shos.info)
* [Web Site (Japanese)](http://www.shos.info)
* [Twitter](https://twitter.com/Fujiwo)
* [Instagram](https://www.instagram.com/fujiwo/)

## License

This library is under the MIT License.
