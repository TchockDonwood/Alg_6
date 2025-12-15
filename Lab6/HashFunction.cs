using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab6
{
    public static class HashFunction<TKey>
    {
        private static int GetStrHashCode(string text)
        { 
            var hash = 0;
            
            foreach (var c in text)
            {
                hash += c;
            }

            return hash;
        }

        public static int DivisionHash(TKey key, int tableSize)
        {
            if (key is int number)
                return number % tableSize;
            else if (key is string text)
                return GetStrHashCode(text) % tableSize;
            else
                throw new ArgumentException();
        }

        public static int MultiplicationHash(int key, int tableSize)
        {
            const double A = 0.6180339887;
            double fractionalPart = (key * A) - Math.Floor(key * A);
            return (int)Math.Floor(tableSize * fractionalPart);
        }
    }
}
