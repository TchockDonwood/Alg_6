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
            var table = new ChainingHashTable<int, string>(5, HashFunction<int>.MultiplicationHash);
            table[15] = "0";
            table.Add(2655, "1");
            table.Add(17, "2");
            table.Add(15, "3");
            table.Add(20, "4");

            Console.WriteLine(table.ToString());
            Console.WriteLine(table[15]);
        }
    }
}