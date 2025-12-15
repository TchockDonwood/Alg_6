using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab6
{
    public class HashNode<TKey, TValue>
    {
        public TKey Key { get; private set; }
        public TValue Value { get; set; }

        public HashNode<TKey, TValue> Next { get; set; }

        public HashNode<TKey, TValue> Prev { get; set; }

        public HashNode(TKey Key, TValue Value)
        {
            if (Key == null)
                throw new ArgumentNullException();

            this.Key = Key;
            this.Value = Value;
            Next = null;
            Prev = null;
        }

        public override string ToString()
        {
            return Key.ToString();
        }
    }
}
