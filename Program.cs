// Program.cs
using ScottPlot;
using ScottPlot.Colormaps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab6
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var hashFunctions = new Dictionary<string, ChainingHashTable<string, int>.HashFunction>
            {
                { "Division", (k, s) => HashFunction<string>.DivisionHash(k, (int)s) },
                { "Multiplication", (k, s) => HashFunction<string>.MultiplicationHash(k, (int)s) },
                { "DJB2", HashFunction<string>.DJB2Hash },
                { "FNV-1a", HashFunction<string>.Fnv1aHash },
                { "MurmurHash3", HashFunction<string>.MurmurHash3 }
            };
            
            const uint tableSizeFixed = 1_000;
            const int startElements = 1; 
            const int endElements = 100_000;
            const int stepElements = 100;

            //Benchmark.GenerateHeatmaps(hashFunctions, tableSizeFixed, endElements);
            /*
            Benchmark.AddingPlot(
                hashFunctions,
                tableSizeFixed,
                startElements,
                endElements,
                stepElements
            );

            Benchmark.SearchingPlot(
                hashFunctions,
                tableSizeFixed,
                startElements,
                endElements,
                stepElements
            );

            Benchmark.RemovingPlot(
                hashFunctions,
                tableSizeFixed,
                startElements,
                endElements,
                stepElements
            );
            */

            Console.WriteLine("\n\n--- Анализ статистических характеристик (N=100000) ---");

            var allKeysForAnalysis = Benchmark.GenerateUniqueKeys(endElements);
            var results = new List<double>();

            foreach (var hf in hashFunctions)
            {
                var table = new ChainingHashTable<string, int>(tableSizeFixed, hf.Value);

                var stats = table.GetChainStats();
                var samples = new List<double>();

                for (int i = 0; i < endElements; i += 15)
                {
                    table.Add(allKeysForAnalysis[i], i);
                    stats = table.GetChainStats();
                    samples.Add(stats.avg);
                    Console.WriteLine($"i: {i} avg: {stats.avg} hash: {hf.Key}");
                }

                double avg = 0.0;
                foreach (var item in samples)
                {
                    avg += item;
                }
                avg = Math.Round(avg / samples.Count(), 3);
                results.Add(avg);
            }

            Plot myPlot = new();
            var barPlot = myPlot.Add.Bars(results.ToArray());

            foreach (var bar in barPlot.Bars)
            {
                bar.Label = bar.Value.ToString();
            }
            barPlot.ValueLabelStyle.FontSize = 18;

            Tick[] ticks =
            {
                new(0, "Division"),
                new(1, "Multiplication"),
                new(2, "DJB2"),
                new(3, "FNV-1a"),
                new(4, "MurmurHash3"),
            };

            myPlot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(ticks);
            myPlot.Axes.Bottom.MajorTickStyle.Length = 0;
            myPlot.HideGrid();

            myPlot.Axes.Margins(bottom: 0);

            myPlot.SavePng("Avg.png", 800, 600);

            Console.WriteLine("\n--- Задание выполнено. Результаты в консоли и в файле 'Сравнение_производительности_хеш-функций_(время_Search_vs._Lambda).png' ---");


            /*
            Console.WriteLine("\n\n--- Анализ статистических характеристик (N=100000) ---");

            var allKeysForAnalysis = Benchmark.GenerateUniqueKeys(endElements);
            var results = new List<int[]>();

            foreach (var hf in hashFunctions)
            {
                var table = new ChainingHashTable<string, int>(tableSizeFixed, hf.Value);

                for (int i = 0; i < endElements; i++)
                {
                    table.Add(allKeysForAnalysis[i], i);
                }

                var stats = table.GetChainStats();

                var result = new int[2] { stats.min, stats.max };
                results.Add(result);
            }

            Plot myPlot = new();
            Bar[] bars =
            {
                // 1 group
                new() { Position = 1, Value = results[0][0], FillColor = Colors.Green },
                new() { Position = 2, Value = results[0][1], FillColor = Colors.Orange },
                
                // 2 group
                new() { Position = 3, Value = results[1][0], FillColor = Colors.Green },
                new() { Position = 4, Value = results[1][1], FillColor = Colors.Orange },

                // 3 group
                new() { Position = 5, Value = results[2][0], FillColor = Colors.Green },
                new() { Position = 6, Value = results[2][1], FillColor = Colors.Orange },
                
                // 4 group
                new() { Position = 7, Value = results[3][0], FillColor = Colors.Green },
                new() { Position = 8, Value = results[3][1], FillColor = Colors.Orange },

                // 5 group
                new() { Position = 9, Value = results[4][0], FillColor = Colors.Green },
                new() { Position = 10, Value = results[4][1], FillColor = Colors.Orange },
            };

            myPlot.Add.Bars(bars);

            myPlot.Legend.IsVisible = true;
            myPlot.Legend.Alignment = Alignment.UpperLeft;
            myPlot.Legend.ManualItems.Add(new() { LabelText = "Мин. цепочка", FillColor = Colors.Blue });
            myPlot.Legend.ManualItems.Add(new() { LabelText = "Макс. цепочка", FillColor = Colors.Orange });

            Tick[] ticks =
            {
                new(1.5, "Division"),
                new(3.5, "Multiplication"),
                new(5.5, "DJB2"),
                new(7.5, "FNV-1a"),
                new(9.5, "MurmurHash3"),
            };

            myPlot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(ticks);
            myPlot.Axes.Bottom.MajorTickStyle.Length = 0;
            myPlot.HideGrid();

            myPlot.Axes.Margins(bottom: 0);

            myPlot.SavePng("Chains.png", 800, 600);            

            Console.WriteLine("\n--- Задание выполнено. Результаты в консоли и в файле 'Сравнение_производительности_хеш-функций_(время_Search_vs._Lambda).png' ---");
            */
        }
    }
}