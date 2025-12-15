using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Lab6
{
    public class ChainingHashTable<TKey, TValue> where TKey : IEquatable<TKey>
    {
        public delegate int HashFunction(TKey key, int tableSize);

        public int Count { get; private set; }
        private HashFunction hashFunction;
        private int size;
        private HashNode<TKey, TValue>[] table;

        public ChainingHashTable(int size, HashFunction hashFunc) 
        {
            if (size <= 0)
                throw new ArgumentException();
            hashFunction = hashFunc;
            this.size = size;
            table = new HashNode<TKey, TValue>[size];
        }

        public TValue this[TKey key]
        {
            set
            {
                Add(key, value);
            }
            get 
            {
                return Search(key);
            }
        }

        public TValue Search(TKey key)
        {
            var hashedKey = hashFunction(key, size);
            var bucket = table[hashedKey];

            var current = bucket;
            HashNode<TKey, TValue> previous = null;

            while (current != null)
            {
                if (current.Key.Equals(key))
                {
                    return current.Value;
                }
            }

            throw new KeyNotFoundException();
        }

        public void Add(TKey key, TValue value)
        {
            var hashedKey = hashFunction(key, size);
            var bucket = table[hashedKey];
            var hashNode = new HashNode<TKey, TValue>(key, value);
            HashNode<TKey, TValue> head = bucket;
            

            if (bucket == null)
            {
                head = hashNode;
                table[hashedKey] = head;
            }
            else
            {
                var current = head;
                HashNode<TKey, TValue> previous = null;

                while (current != null)
                {
                    if (current.Key.Equals(key))
                    {
                        current.Value = value;
                        return;
                    }
                    
                    previous = current;
                    current = current.Next;
                }

                current = hashNode;
                previous.Next = current;
                hashNode.Prev = previous;
            }

            Count++;
        }

        public bool Remove(TKey key)
        {
            var hashedKey = hashFunction(key, size);
            var bucket = table[hashedKey];
            
            var current = bucket;
            HashNode<TKey, TValue> previous = null;

            while (current != null)
            {
                if (current.Key.Equals(key))
                {
                    if (previous == null)
                    {
                        table[hashedKey] = current.Next;
                    }
                    else
                    {
                        previous.Next = current.Next;
                    }
                    Count--;
                    return true;
                }

                previous = current;
                current = current.Next;
            }

            return false;
        }

        public override string ToString()
        {
            var text = "";

            for (var i = 0; i < table.Length; i++)
            {   
                var bucket = table[i];
                var current = bucket;
                text += $"{i}: ";
                while (current != null)
                {
                    text += $"{current.Key}:\"{current.Value}\" –> ";
                    current = current.Next;
                }
                text += "null\n";
            }

            return text;
        }
    }
}
