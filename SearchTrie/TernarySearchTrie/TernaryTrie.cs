using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Global.SearchTrie
{
    /// <summary>
    /// This is the structure for a balanced ternary search trie which can be used
    /// on any appropriate data types.
    /// <para>
    /// It uses an efficient tree structure (SortedDictionary eg. Red-Black Tree)
    /// for indexing at a level to maintain l*log(n) lookup speeds, where 
    /// <code>l = the average length of a word</code> and 
    /// <code>n = the number of items in the structure.</code> 
    /// </para>
    /// </summary>
    /// <typeparam name="TKeyPiece">The peices that the Keys will be split into that are comparable (eg. char's).</typeparam>
    /// <typeparam name="TValue">The object to relate with a Key.</typeparam>
    [DebuggerDisplay("Size={size}")]
    public partial class TernarySearchTrie<TKeyPiece, TValue> : IDictionary<IEnumerable<TKeyPiece>, IList<TValue>>
        where TKeyPiece : IComparable
    {
        /// <summary>
        /// The root of the Trie.
        /// </summary>
        protected Node root = new Node();
        /// <summary>
        /// The internal counter on how many items are in the Trie.
        /// </summary>
        protected int size = 0;
        /// <summary>
        /// A flag to track modifications to the datastructure.
        /// </summary>
        protected bool modified = false;

        /// <summary>
        /// Set the visted status of all nodes in the privided value..
        /// </summary>
        protected bool AllVisited
        {
            set
            {
                SetNV(root, value);
            }
        }

        /// <summary>
        /// Internal set operation to recursively set all nodes 
        /// visited status to <paramref name="v"/>.
        /// </summary>
        /// <param name="n"></param>
        /// <param name="v"></param>
        private void SetNV(Node n, bool v)
        {
            n.Visited = v;
            foreach (var nc in n.Next.Values) SetNV(nc, v);
        }

        /// <summary>
        /// Represents a node of the tree.
        /// </summary>
        /// <remarks>Using fields instead of properties drops execution time by about 40%.</remarks>
        [DebuggerDisplay("Key={Key}, Values={Values.Count}, Size={Next.Count}")]
        protected class Node
        {
            /// <summary>
            /// Gets or sets the node's key.
            /// </summary>
            public TKeyPiece Key;

            /// <summary>
            /// Gets or sets the node's value.
            /// </summary>
            public List<TValue> Values = new List<TValue>();

            /// <summary>
            /// Gets or sets the next Node down.
            /// </summary>
            public SortedDictionary<TKeyPiece, Node> Next = new SortedDictionary<TKeyPiece, Node>();

            /// <summary>
            /// Marks this node as containing data or not.
            /// </summary>
            public bool IsContainer = false;

            /// <summary>
            /// Flag to track when nodes are visited.
            /// </summary>
            internal bool Visited = false;

            /// <summary>
            /// Set when this node is a container.
            /// </summary>
            public IEnumerable<TKeyPiece> repValue = default(IEnumerable<TKeyPiece>);
        }

        /// <summary>
        /// Searches for a given pattern in the datastructure.
        /// </summary>
        /// <param name="pattern">The pattern to match to.</param>
        /// <returns></returns>
        public List<TValue> Search(IEnumerable<TKeyPiece> pattern)
        {
            return Collect(root, pattern.ToArray(), 0);
        }

        /// <summary>
        /// Searches through the datastructure for any items that 
        /// match the given pattern.
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public List<TValue> Search(IList<TKeyPiece> pattern)
        {
            return Collect(root, pattern, 0);
        }

        /// <summary>
        /// Collects items matching the given pieces by progressing recursively
        /// through the trie.
        /// </summary>
        /// <param name="n">The current node.</param>
        /// <param name="pieces">The series of pieces.</param>
        /// <param name="idx">The index of the piece currently in focus.</param>
        /// <returns></returns>
        protected List<TValue> Collect(Node n, IList<TKeyPiece> pieces, int idx)
        {
            if (idx == pieces.Count)
            {
                return n.Values;
            }
            else
            {
                List<TValue> items = new List<TValue>();

                while (idx <= pieces.Count)
                {
                    // Let's look at the given node's children:
                    SortedDictionary<TKeyPiece, Node> list = n.Next;

                    // Collect values if we're at the end.
                    if (idx == pieces.Count)
                    {
                        items.AddRange(n.Values);
                    }

                    if (idx < pieces.Count)
                    {
                        // Let's look at the piece at idx:
                        TKeyPiece piece = pieces[idx];

                        // prepare for the next iteration
                        if (list.ContainsKey(piece))
                        {
                            n = list[piece];
                        }
                        else break;
                    }

                    idx++;
                }

                return items;
            }
        }

        /// <summary>
        /// Collects all items in the Trie with the given prefix.
        /// </summary>
        /// <param name="prefix">The prefix to match.</param>
        /// <returns>A list of values with the given prefix.</returns>
        public IList<TValue> CollectAfter(IList<TKeyPiece> prefix)
        {
            int idx = 0;
            IList<TValue> vals = new List<TValue>();
            Node n = root;

            // Iterate to the end of the prefix.
            while (idx < prefix.Count)
            {
                if (n.Next.ContainsKey(prefix[idx]))
                    n = n.Next[prefix[idx]];
                else
                    return vals;
            }

            // Collect the Nodes' Values
            GetValues(n, ref vals);

            return vals;
        }

        /// <summary>
        /// Recursively select the values of Nodes starting at the 
        /// provided Node <paramref name="n"/>.
        /// </summary>
        /// <param name="n">The Node to collect below.</param>
        /// <param name="values">The place to store TValues found.</param>
        private void GetValues(Node n, ref IList<TValue> values)
        {
            if (n.IsContainer)
                foreach (TValue v in n.Values) values.Add(v);

            foreach (Node next in n.Next.Values)
                GetValues(next, ref values);
        }

        /// <summary>
        /// Obtains the number of unique items in the Trie.
        /// </summary>
        public int Count
        {
            get { return size; }
        }

        /// <summary>
        /// Iterates over the Trie and collects the keys.
        /// </summary>
        public ICollection<IEnumerable<TKeyPiece>> Keys
        {
            get
            {
                ICollection<IEnumerable<TKeyPiece>> keys = new List<IEnumerable<TKeyPiece>>();
                foreach (KeyValuePair<IEnumerable<TKeyPiece>, IList<TValue>> item in this)
                {
                    keys.Add(item.Key);
                }
                return keys;
            }
        }

        /// <summary>
        /// Iterates over the Trie and collects the values.
        /// </summary>
        public ICollection<IList<TValue>> Values
        {
            get
            {
                ICollection<IList<TValue>> values = new List<IList<TValue>>();
                foreach (KeyValuePair<IEnumerable<TKeyPiece>, IList<TValue>> item in this)
                {
                    values.Add(item.Value);
                }
                return values;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        /// <param name="node"></param>
        protected void _getVals(KeyValuePair<IEnumerable<TKeyPiece>, IList<TValue>>[] array, int arrayIndex, Node node)
        {
            foreach (KeyValuePair<TKeyPiece, Node> pair in node.Next)
            {
                // Check if this is an instance of a pair
                if (pair.Value.IsContainer)
                {
                    IEnumerable<TKeyPiece> key = pair.Value.repValue;
                    IList<TValue> values = pair.Value.Values;

                    // set the current item
                    array[arrayIndex++] = new KeyValuePair<IEnumerable<TKeyPiece>, IList<TValue>>(key, values);
                }

                // Children come after a parent
                if (pair.Value != null)
                    _getVals(array, arrayIndex, pair.Value);
            }
        }

        /// <summary>
        /// Returns false by default.
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Access the IList of TValues at the given TKey index. If the
        /// assignment operator is used then the existing TValue's are
        /// overwritten.
        /// </summary>
        /// <param name="key">The location of the values to retrieve.</param>
        /// <returns>The same as the Search function.</returns>
        IList<TValue> IDictionary<IEnumerable<TKeyPiece>, IList<TValue>>.this[IEnumerable<TKeyPiece> key]
        {
            get { return Search(key); }
            set { Remove(key); Add(key, value); }
        }

        /// <summary>
        /// Obtains the <typeparamref name="TValue"/> for the given Key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>The <typeparamref name="TValue"/>for the given Key.</returns>
        public IList<TValue> this[IEnumerable<TKeyPiece> key]
        {
            get
            {
                return Search(key);
            }
            set
            {
                Remove(key);
                Add(key, value);
            }
        }

        /// <summary>
        /// Removes everything from the data structure.
        /// </summary>
        public void Clear()
        {
            modified = true;
            root = new Node();
            size = 0;
        }

        /// <summary>
        /// Checks if a given pair are in the trie.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(KeyValuePair<IEnumerable<TKeyPiece>, TValue> item)
        {
            List<TValue> vals = Search(item.Key);
            return vals.Contains(item.Value);
        }

        /// <summary>
        /// Checks if the given Key has been previously been added into the Trie.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(IEnumerable<TKeyPiece> key)
        {
            return ContainsKey(root, key.GetEnumerator());
        }

        /// <summary>
        /// Determines whether the specified node contains key.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="enmrtr">The enmrtr.</param>
        /// <returns>
        ///   <c>true</c> if the specified node contains key; otherwise, <c>false</c>.
        /// </returns>
        protected bool ContainsKey(Node node, IEnumerator<TKeyPiece> enmrtr)
        {
            if (enmrtr.MoveNext())
            {
                TKeyPiece now = enmrtr.Current;

                if (node.Next == null)
                    return false;

                // Recurse
                if (node.Next.ContainsKey(now))
                    return ContainsKey(node.Next[now], enmrtr);
                else
                    return false;
            }
            else
            {
                return node.IsContainer;
            }
        }

        /// <summary>
        /// Copies the elements of the Trie to an array of type 
        /// KeyValuePair(TKey, TValue), starting at the specified array index.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        /// <exception cref="ArgumentNullException"><paramref name="array"/> is <code>null</code>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception>
        /// <exception cref="ArgumentException">The number of elements in the source Trie is greater than the 
        /// available space from index to the end of the destination array.</exception>
        public void CopyTo(KeyValuePair<IEnumerable<TKeyPiece>, IList<TValue>>[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (Count - arrayIndex > array.Length) throw new ArgumentException("Not enough space in the given array.");

            _copyTo(array, ref arrayIndex, root);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        /// <param name="node"></param>
        protected void _copyTo(KeyValuePair<IEnumerable<TKeyPiece>, IList<TValue>>[] array, ref int arrayIndex, Node node)
        {
            foreach (KeyValuePair<TKeyPiece, Node> pair in node.Next)
            {
                // Check if this is an instance of a pair
                if (pair.Value.IsContainer)
                {
                    IEnumerable<TKeyPiece> key = pair.Value.repValue;
                    IList<TValue> values = pair.Value.Values;

                    // set the current item
                    array[arrayIndex++] = new KeyValuePair<IEnumerable<TKeyPiece>, IList<TValue>>(key, values);
                }

                // Children come after a parent
                if (pair.Value != null)
                    _copyTo(array, ref arrayIndex, pair.Value);
            }
        }

        /// <summary>
        /// Efficiently obtains a <typeparamref name="TValue"/> when 
        /// unsure about its existence in the data structure.
        /// </summary>
        /// <param name="key">The key to check for.</param>
        /// <param name="value">The value to assigned to.</param>
        /// <returns>True if found.</returns>
        public bool TryGetValue(IEnumerable<TKeyPiece> key, out IList<TValue> value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            List<TValue> vals = Search(key);
            if (vals.Count > 0)
            {
                value = vals;
                return true;
            }
            else
            {
                value = default(IList<TValue>);
                return false;
            }
        }
    }
}
