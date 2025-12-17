// Benchmark.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ScottPlot;
using static SkiaSharp.HarfBuzz.SKShaper;

namespace Lab6
{
    public class Benchmark
    {
        public class HashBenchmarkResult
        {
            public string Label { get; }
            public List<double> Searches { get; set; } = new List<double>();
            public List<double> Adds { get; set; } = new List<double>();
            public List<double> Removes { get; set; } = new List<double>();
            public List<int> Elems { get; set; } = new List<int>();

            public HashBenchmarkResult(string label)
            {
                Label = label;
            }
        }

        private static double Median(IList<double> values)
        {
            var sorted = values.OrderBy(x => x).ToList();
            int count = sorted.Count;

            if (count == 0) return 0;

            if (count % 2 == 1)
            {
                return sorted[count / 2];
            }
            else
            {
                return (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;
            }
        }

        public static List<string> GenerateUniqueKeys(int count)
        {
            var keys = new HashSet<string>();
            var random = new Random();

            while (keys.Count < count)
            {
                if (count > 50000)
                {
                    keys.Add(string.Concat(Enumerable.Repeat($"Key_{keys.Count}_{random.Next(1000, 999999)}", random.Next(100))));
                }
                else
                {
                    string randomStr = System.IO.Path.GetRandomFileName().Replace(".", "");
                    keys.Add($"{randomStr}_{random.Next(1, 1000)}");
                }
            }

            return keys.ToList();
        }

        public static void GenerateHeatmaps(
            Dictionary<string, ChainingHashTable<string, int>.HashFunction> hashFunctions,
            uint tableSize,
            int elementsCount)
        {
            Console.WriteLine("\n--- Генерация Heatmap графиков распределения ---");

            var keys = GenerateUniqueKeys(elementsCount);

            int side = (int)Math.Ceiling(Math.Sqrt(tableSize));

            foreach (var kvp in hashFunctions)
            {
                string name = kvp.Key;
                var hashFunc = kvp.Value;

                var table = new ChainingHashTable<string, int>(tableSize, hashFunc);
                var numTable = new ChainingHashTable<string, int>(tableSize, hashFunc);
                
                foreach (var key in keys)
                {
                    table.Add(key, 0);
                }
                
                for (int i = 0; i < elementsCount; i++)
                {
                    numTable.Add(i.ToString(), 0);
                }

                double[,] heatmapData = new double[side, side];
                double[,] numHeatmapData = new double[side, side];

                for (int i = 0; i < tableSize; i++)
                {

                    int chainLength = 0;
                    int numChainLength = 0;
                    var node = table.table[i];
                    var numNode = numTable.table[i];
                    
                    while (node != null)
                    {
                        chainLength++;
                        node = node.Next;
                    }
                    
                    while (numNode != null)
                    {
                        numChainLength++;
                        numNode = numNode.Next;
                    }

                    int row = i / side;
                    int col = i % side;

                    heatmapData[row, col] = chainLength;
                    numHeatmapData[row, col] = numChainLength;
                }



                Plot myPlot = new();

                var hm = myPlot.Add.Heatmap(heatmapData);
                var hm2 = myPlot.Add.Heatmap(numHeatmapData);

                hm.Colormap = new ScottPlot.Colormaps.Blues().Reversed();
                hm2.Colormap = new ScottPlot.Colormaps.Blues().Reversed();

                myPlot.Add.ColorBar(hm);

                hm.ManualRange = new(0, 20);
                hm2.ManualRange = new(0, 20);
                
                hm.Position = new(0, 65, 0, 100);
                hm2.Position = new(100, 165, 0, 100);

                myPlot.Add.Text("Случайные значения", 0, 110);
                myPlot.Add.Text("Последовательные значения", 100, 110);

                myPlot.Layout.Frameless();
                myPlot.Axes.Margins(0, 0);

                myPlot.Title($"Heatmap: {name} (N={elementsCount}, M={tableSize})");

                string safeName = name.Replace(" ", "_").Replace(".", "").Replace("(", "").Replace(")", "");
                string filename = $"heatmap_{safeName}.png";

                myPlot.SavePng(filename, 1000, 500);
                Console.WriteLine($"Сохранен график: {filename}");
            }
        }

        public static void AddingPlot(
            Dictionary<string, ChainingHashTable<string, int>.HashFunction> hashFunctions,
            uint tableSize,
            int startElements,
            int endElements,
            int stepElements,
            int repetitions = 1)
        {
            var stopwatch = new Stopwatch();
            var random = new Random();
            var label = "";
            var allResults = new List<HashBenchmarkResult>();
            HashBenchmarkResult result;

            Console.WriteLine("--- Запуск бенчмарка для добавления элемента в хеш-функцию ---");
            Console.WriteLine($"Фиксированный размер таблицы (M): {tableSize}");

            int maxKeys = endElements;
            var allKeys = GenerateUniqueKeys(maxKeys);

            foreach (var kvp in hashFunctions)
            {
                label = kvp.Key;
                var hashFunc = kvp.Value;
                result = new HashBenchmarkResult(label);

                Console.WriteLine($"\nБенчмарк функции: '{label}'");

                for (int elementsN = startElements; elementsN <= endElements; elementsN += stepElements)
                {
                    var table = new ChainingHashTable<string, int>(tableSize, hashFunc);

                    for (int i = 0; i < elementsN; i++)
                    {
                        for (int j = 0; j < repetitions; j++)
                        {
                            stopwatch.Restart();
                            table.Add(allKeys[i], i);
                            stopwatch.Stop();
                            result.Adds.Add(stopwatch.Elapsed.TotalMilliseconds);
                        }
                    }

                    result.Elems.Add(elementsN);
                    var index = result.Adds.Count - 1;

                    Console.WriteLine($"N = {elementsN}, M = {tableSize}, Медианное время: {result.Adds[index]:F6} мс");
                }

                var filePath = $"{label}_Adding.png";
                var plt = new Plot();
                plt.Title($"{label}_Adding");
                plt.XLabel("Количество элементов");
                plt.YLabel("Время добавления в таблицу (мс)");

                int[] xData = result.Elems.ToArray();
                double[] yData = result.Adds.ToArray();

                if (xData.Length > 0)
                {
                    var scatter = plt.Add.Scatter(xData, yData);
                    scatter.LegendText = result.Label;
                    scatter.MarkerShape = MarkerShape.None;
                    scatter.LineStyle.Color = Colors.Blue;
                    scatter.LineStyle.Width = 1;
                }

                plt.ShowLegend(Alignment.UpperLeft, Orientation.Vertical);
                plt.SavePng(filePath, 800, 600);
                Console.WriteLine($"\nГрафик зависимости времени от добавления элемента в хеш-функцию сохранен: {System.IO.Path.GetFullPath(filePath)}");

                allResults.Add(result);
            }
            PlotHashComparison(allResults, "Сравнение производительности хеш-функций по Add");
        }

        public static void SearchingPlot(
            Dictionary<string, ChainingHashTable<string, int>.HashFunction> hashFunctions,
            uint tableSize,
            int startElements,
            int endElements,
            int stepElements,
            int repetitions = 1)
        {
            var stopwatch = new Stopwatch();
            var random = new Random();
            var label = "";
            var allResults = new List<HashBenchmarkResult>();
            HashBenchmarkResult result;

            Console.WriteLine("--- Запуск бенчмарка для поиска элемента в хеш-функции ---");
            Console.WriteLine($"Фиксированный размер таблицы (M): {tableSize}");

            int maxKeys = endElements;
            var allKeys = GenerateUniqueKeys(maxKeys);

            foreach (var kvp in hashFunctions)
            {
                label = kvp.Key;
                var hashFunc = kvp.Value;
                result = new HashBenchmarkResult(label);

                Console.WriteLine($"\nБенчмарк функции: '{label}'");

                for (int elementsN = startElements; elementsN <= endElements; elementsN += stepElements)
                {
                    var table = new ChainingHashTable<string, int>(tableSize, hashFunc);
                    string keyToSearch = allKeys[random.Next(0, elementsN)];

                    for (int i = 0; i < elementsN; i++)
                    {
                        table.Add(allKeys[i], i);
                    }

                    for (int i = 0; i < repetitions; i++)
                    {
                        stopwatch.Restart();
                        table.Search(keyToSearch);
                        stopwatch.Stop();
                        result.Searches.Add(stopwatch.Elapsed.TotalMilliseconds);
                    }

                    result.Elems.Add(elementsN);
                    var index = result.Searches.Count - 1;

                    Console.WriteLine($"N = {elementsN}, M = {tableSize}, Медианное время: {result.Searches[index]:F6} мс");
                }

                var filePath = $"{label}_Searching.png";
                var plt = new Plot();
                plt.Title($"{label}_Searching");
                plt.XLabel("Количество элементов");
                plt.YLabel("Время поиска в таблице (мс)");

                int[] xData = result.Elems.ToArray();
                double[] yData = result.Searches.ToArray();

                if (xData.Length > 0)
                {
                    var scatter = plt.Add.Scatter(xData, yData);
                    scatter.LegendText = result.Label;
                    scatter.MarkerShape = MarkerShape.None;
                    scatter.LineStyle.Color = Colors.Blue;
                    scatter.LineStyle.Width = 1;
                }

                plt.ShowLegend(Alignment.UpperLeft, Orientation.Vertical);
                plt.SavePng(filePath, 800, 600);
                Console.WriteLine($"\nГрафик зависимости времени от поиска элемента в хеш-функции сохранен: {System.IO.Path.GetFullPath(filePath)}");

                allResults.Add(result);
            }
            PlotHashComparison(allResults, "Сравнение производительности хеш-функций по Search");
        }

        public static void RemovingPlot(
            Dictionary<string, ChainingHashTable<string, int>.HashFunction> hashFunctions,
            uint tableSize,
            int startElements,
            int endElements,
            int stepElements,
            int repetitions = 1)
        {
            var stopwatch = new Stopwatch();
            var random = new Random();
            var label = "";
            var allResults = new List<HashBenchmarkResult>();
            HashBenchmarkResult result;

            Console.WriteLine("--- Запуск бенчмарка для удаления элемента из хеш-функции ---");
            Console.WriteLine($"Фиксированный размер таблицы (M): {tableSize}");

            int maxKeys = endElements;
            var allKeys = GenerateUniqueKeys(maxKeys);

            foreach (var kvp in hashFunctions)
            {
                label = kvp.Key;
                var hashFunc = kvp.Value;
                result = new HashBenchmarkResult(label);

                Console.WriteLine($"\nБенчмарк функции: '{label}'");

                for (int elementsN = startElements; elementsN <= endElements; elementsN += stepElements)
                {
                    var table = new ChainingHashTable<string, int>(tableSize, hashFunc);
                    string keyToSearch = allKeys[random.Next(0, elementsN)];

                    for (int i = 0; i < elementsN; i++)
                    {
                        table.Add(allKeys[i], i);
                    }

                    for (int i = 0; i < repetitions; i++)
                    {
                        stopwatch.Restart();
                        table.Remove(keyToSearch);
                        stopwatch.Stop();
                        result.Removes.Add(stopwatch.Elapsed.TotalMilliseconds);
                    }

                    result.Elems.Add(elementsN);
                    var index = result.Removes.Count - 1;

                    Console.WriteLine($"N = {elementsN}, M = {tableSize}, Медианное время: {result.Removes[index]:F6} мс");
                }

                var filePath = $"{label}_Removing.png";
                var plt = new Plot();
                plt.Title($"{label}_Removing");
                plt.XLabel("Количество элементов");
                plt.YLabel("Время удаления из таблицы (мс)");

                int[] xData = result.Elems.ToArray();
                double[] yData = result.Removes.ToArray();

                if (xData.Length > 0)
                {
                    var scatter = plt.Add.Scatter(xData, yData);
                    scatter.LegendText = result.Label;
                    scatter.MarkerShape = MarkerShape.None;
                    scatter.LineStyle.Color = Colors.Blue;
                    scatter.LineStyle.Width = 1;
                }

                plt.ShowLegend(Alignment.UpperLeft, Orientation.Vertical);
                plt.SavePng(filePath, 800, 600);
                Console.WriteLine($"\nГрафик зависимости времени от удаления элемента из хеш-функции сохранен: {System.IO.Path.GetFullPath(filePath)}");

                allResults.Add(result);
            }
            PlotHashComparison(allResults, "Сравнение производительности хеш-функций по Remove");
        }

        public static void PlotHashComparison(List<HashBenchmarkResult> results, string title)
        {
            var filePath = $"{title.Replace(' ', '_')}.png";
            var arr = title.Split(" ");
            var name = arr[arr.Length-1];
            var plt = new Plot();
            plt.Title(title);
            plt.XLabel("Количество элементов");
            plt.YLabel($"Медианное время операции {name} (мс)");

            var colors = new[] { Colors.Blue, Colors.Red, Colors.Green, Colors.Purple, Colors.Orange };

            for (int i = 0; i < results.Count; i++)
            {
                var result = results[i];
                int[] xData = result.Elems.ToArray();
                double[] yData = new double[] { };
                switch (name)
                {
                    case "Search":
                        yData = result.Searches.ToArray();
                        break;
                    case "Remove":
                        yData = result.Removes.ToArray();
                        break;
                    case "Add":
                        yData = result.Adds.ToArray();
                        break;
                }
                

                if (xData.Length > 0)
                {
                    var scatter = plt.Add.Scatter(xData, yData);
                    scatter.LegendText = result.Label;
                    scatter.MarkerShape = MarkerShape.None;
                    scatter.LineStyle.Color = colors[i % colors.Length];
                    scatter.LineStyle.Width = 1;
                }
            }

            plt.Axes.SetLimitsY(0, 0.04);
            
            plt.ShowLegend(Alignment.UpperLeft, Orientation.Vertical);
            plt.SavePng(filePath, 800, 600);
            Console.WriteLine($"\nСравнительный график хеш-функций сохранен: {System.IO.Path.GetFullPath(filePath)}");
        }
    }
}