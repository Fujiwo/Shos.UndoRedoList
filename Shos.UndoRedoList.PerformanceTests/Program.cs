using System;
using System.Collections.Generic;
using Shos.Collections;

namespace Shos.UndoRedoList.PerformanceTests
{
    class Program
    {
        const int times = 1000000;
        static Random random = new Random();

        static void Test()
        {
            var list = new UndoRedoList<int, List<int>>();
            AddTest     (list);
            RemoveTest  (list);
            ExchangeTest(list);
        }

        static void AddTest(UndoRedoList<int, List<int>> list)
        {
            for (var count = 1; count <= times; count++)
                list.Add(count);

            for (var count = 1; count <= times; count++)
                list.Undo();

            for (var count = 1; count <= times; count++)
                list.Redo();
        }

        static void RemoveTest(UndoRedoList<int, List<int>> list)
        {
            for (var count = 1; count <= times; count++)
                list.RemoveAt(times - count);

            for (var count = 1; count <= times; count++)
                list.Undo();

            for (var count = 1; count <= times; count++)
                list.Redo();
        }

        static void ExchangeTest(UndoRedoList<int, List<int>> list)
        {
            for (var count = 1; count <= times; count++)
                list.Add(count);

            for (var count = 1; count <= times; count++)
                list[random.Next(times)] = count;

            for (var count = 1; count <= times; count++)
                list.Undo();

            for (var count = 1; count <= times; count++)
                list.Redo();
        }

        static void Main() => Test();
    }
}
