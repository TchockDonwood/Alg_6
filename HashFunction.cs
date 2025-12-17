// HashFunction.cs
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using static System.Numerics.BitOperations;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Lab6
{
    public static class HashFunction<TKey>
    {
        // Вспомогательный метод для получения положительного хеш-кода из любого типа
        private static int GetIntHash(TKey key)
        {
            // Используем GetHashCode() для преобразования любого типа в int, 
            // затем обнуляем знаковый бит для получения положительного числа
            if (key == null) return 0;
            return key.GetHashCode() & 0x7FFFFFFF;
        }

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

        public static int MultiplicationHash(TKey key, int tableSize)
        {
            int k = GetIntHash(key);
            const double A = 0.6180339887;
            double fractionalPart = (k * A) - Math.Floor(k * A);
            return (int)Math.Floor(tableSize * fractionalPart);
        }

        public static int DJB2Hash(TKey key, uint tableSize)
        {
            string str;

            if (key is int number)
            {
                str = number.ToString();
            }    
            else if (key is string text)
            {
                str = text;
            }
            else
                throw new ArgumentException();

            ulong hash = 5381;

            foreach (char c in str)
            {
                hash = (hash << 5) + hash + c;
            }

            return (int)(hash % tableSize);
        }

        public static int Fnv1aHash(TKey key, uint tableSize)
        {
            byte[] bytes;

            if (key is string s)
            {
                bytes = Encoding.UTF8.GetBytes(s);
            }
            else if (key is int i)
            {
                bytes = BitConverter.GetBytes(i);
            }
            else
            {
                bytes = BitConverter.GetBytes(key.GetHashCode());
            }

            const uint FNV_PRIME = 16777619;
            uint hash = 2166136261;

            foreach (byte b in bytes)
            {
                hash ^= b;
                hash *= FNV_PRIME;
            }

            return (int)(hash % tableSize);
        }

        public static int MurmurHash3(TKey key, uint tableSize)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            byte[] bytes;

            switch (key)
            {
                case int i:
                    bytes = BitConverter.GetBytes(i);
                    break;

                case string s:
                    bytes = Encoding.UTF8.GetBytes(s);
                    break;

                default:
                    throw new ArgumentException("Unsupported key type");
            }

            ReadOnlySpan<byte> span = bytes;
            uint hash = Hash32(ref span, 420);

            return (int)(hash % tableSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint Hash32(ref ReadOnlySpan<byte> bytes, uint seed)
        {
            ref byte bp = ref MemoryMarshal.GetReference(bytes);
            ref uint endPoint = ref Unsafe.Add(ref Unsafe.As<byte, uint>(ref bp), bytes.Length >> 2);
            if (bytes.Length >= 4)
            {
                do
                {
                    seed = RotateLeft(seed ^ RotateLeft(Unsafe.ReadUnaligned<uint>(ref bp) * 3432918353U, 15) * 461845907U, 13) * 5 - 430675100;
                    bp = ref Unsafe.Add(ref bp, 4);
                } while (Unsafe.IsAddressLessThan(ref Unsafe.As<byte, uint>(ref bp), ref endPoint));
            }

            var remainder = bytes.Length & 3;
            if (remainder > 0)
            {
                uint num = 0;
                if (remainder > 2) num ^= Unsafe.Add(ref endPoint, 2) << 16;
                if (remainder > 1) num ^= Unsafe.Add(ref endPoint, 1) << 8;
                num ^= endPoint;

                seed ^= RotateLeft(num * 3432918353U, 15) * 461845907U;
            }

            seed ^= (uint)bytes.Length;
            seed = (uint)((seed ^ (seed >> 16)) * -2048144789);
            seed = (uint)((seed ^ (seed >> 13)) * -1028477387);
            return seed ^ seed >> 16;
        }
    }
}