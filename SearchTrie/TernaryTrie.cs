using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Global.SearchTrie
{
    /// <summary>
    /// This is the structure for a balanced ternary search trie which can be used
    /// on any appropriate data types.
    /// 
    /// It uses an efficient tree structure (<paramref name="SortedDictionary"/> eg. Red-Black Tree)
    /// for indexing at a level to maintain l*log(n) lookup speeds, where 
    /// <code>l = the average length of a word</code> and 
    /// <code>n = the number of items in the structure.</code> 
    /// </summary>
    /// <typeparam name="TKey">The item to be searched for (eg. string's).</typeparam>
    /// <typeparam name="TKeyPiece">The peices that the <paramref name="TKey"/> will be split into that are comparable (eg. char's).</typeparam>
    /// <typeparam name="TValue">The object to relate with <paramref name="TKey"/>.</typeparam>
    [DebuggerDisplay("Size={size}")]
    public class TernarySearchTrie<TKey, TKeyPiece, TValue> : IDictionary<TKey, IList<TValue>>
        where TKey : IEnumerable<TKeyPiece> where TKeyPiece : IComparable
    {
        protected Node root = new Node();
        protected int size = 0;
        protected bool modified = false;

        /// <summary>Represents a node of the tree.</summary>
        /// <remarks>Using fields instead of properties drops execution time by about 40%.</remarks>
        [DebuggerDisplay("Key={Key}, Value={Value}, Size={Next.Count}")]
        protected class Node
        {
            /// <summary>Gets or sets the node's key.</summary>
            public TKeyPiece Key;

            /// <summary>Gets or sets the node's value.</summary>
            public List<TValue> Values = new List<TValue>();

            /// <summary>Gets or sets the next Node down.</summary>
            public SortedDictionary<TKeyPiece, Node> Next = new SortedDictionary<TKeyPiece, Node>();

            /// <summary>Marks this node as containing data or not.</summary>
            public bool IsContainer = false;

            /// <summary>Set when this node is a container.</summary>
            public TKey repValue = default(TKey);
        }

        /// <summary>
        /// Searches for a given pattern in the datastructure.
        /// default(<typeparamref name="TKeyPiece"/>) will match anything.
        /// </summary>
        /// <param name="pattern">The pattern to match to.</param>
        /// <param name="ignoreCase">Set to true to ignore case when searching.</param>
        /// <returns></returns>
        public List<TValue> Search(TKey pattern)
        {
            return collect(root, pattern.ToArray(), 0);
        }
        protected List<TValue> collect(Node n, IList<TKeyPiece> pieces, int idx)
        {
            List<TValue> items = new List<TValue>();
            if (n == null) return items;

            if (idx < pieces.Count)
            {
                TKeyPiece tis = pieces[idx];

                if (tis.Equals(default(TKeyPiece)))
                {
                    foreach (Node next in n.Next.Values)
                    {
                        items.AddRange(collect(next, pieces, idx + 1));
                    }
                }
                else
                {
                    Node next;
                    if (n.Next.TryGetValue(tis, out next))
                    {
                        items.AddRange(collect(next, pieces, idx + 1));
                    }
                }
            }
            else if (n.IsContainer) items.AddRange(n.Values);

            return items;
        }

        /// <summary>
        /// Searches through the datastructure for any items that 
        /// match the given pattern. If any <typeparamref name="TKeyPiece"/> 
        /// is equivalent to 
        /// <code>default(</code><typeparamref name="TKeyPiece"/><code>)</code>
        /// then it will be matched to any.
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public List<TValue> Search(IList<TKeyPiece> pattern)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Collects all items in the Trie with the given prefix.
        /// </summary>
        /// <param name="prefix">The prefix to match.</param>
        /// <returns>A list of values with the given prefix.</returns>
        public List<TValue> CollectAfter(IList<TKeyPiece> prefix)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds the given <typeparamref name="TKey" />/<typeparamref name="TValue" />
        /// pair to the Trie.
        /// </summary>
        /// <param name="key">The location to place to <paramref name="item" />.</param>
        /// <param name="item">The object to place in the trie.</param>
        /// <exception cref="ArgumentNullException">key</exception>
        public void Add(TKey key, TValue item)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            modified = true;
            root = Add(item, root, key.GetEnumerator(), key);
            size++;
        }
        /// <summary>Adds the given <typeparamref name="TKey"/>/<typeparamref name="TValue"/>
        /// pair to the Trie.</summary>
        /// <param name="key">The location to place to <paramref name="item"/>.</param>
        /// <param name="item">The object to place in the trie.</param>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }
        /// <summary>Uses an Enumerator(<typeparamref name="TKeyPiece"/>)
        /// to advance through the <typeparamref name="TKey"/></summary>
        protected Node Add(TValue item, Node node, IEnumerator<TKeyPiece> enmrtr, TKey key)
        {
            if (enmrtr.MoveNext())
            {
                TKeyPiece now = enmrtr.Current;

                if (node.Next == null)
                    node.Next = new SortedDictionary<TKeyPiece, Node>();

                // Recurse
                if (node.Next.ContainsKey(now))
                    node.Next[now] = Add(item, node.Next[now], enmrtr, key);
                else
                {
                    // Make a new node
                    Node n = new Node();
                    n.Key = now;

                    // Go down a level
                    node.Next[now] = Add(item, n, enmrtr, key);
                }
            }
            else
            {
                node.Values.Add(item); // Assign the value
                node.repValue = key;
                node.IsContainer = true;
            }
            return node;
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
        public ICollection<TKey> Keys
        {
            get
            {
                ICollection<TKey> keys = new List<TKey>();
                foreach (KeyValuePair<TKey, IList<TValue>> item in this)
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
                foreach (KeyValuePair<TKey, IList<TValue>> item in this)
                {
                    values.Add(item.Value);
                }
                return values;
            }
        }
        protected void _getVals(KeyValuePair<TKey, IList<TValue>>[] array, int arrayIndex, Node node)
        {
            foreach (KeyValuePair<TKeyPiece, Node> pair in node.Next)
            {
                // Check if this is an instance of a pair
                if (pair.Value.IsContainer)
                {
                    TKey key = pair.Value.repValue;
                    IList<TValue> values = pair.Value.Values;

                    // set the current item
                    array[arrayIndex++] = new KeyValuePair<TKey, IList<TValue>>(key, values);
                }

                // Children come after a parent
                if (pair.Value != null)
                    _getVals(array, arrayIndex, pair.Value);
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        IList<TValue> IDictionary<TKey, IList<TValue>>.this[TKey key]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        /// <summary>
        /// Obtains the <typeparamref name="TValue"/> for the given <typeparamref name="TKey"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>The <typeparamref name="TValue"/>for the given <typeparamref name="TKey"/>.</returns>
        public IList<TValue> this[TKey key]
        {
            get
            {
                return Search(key);
            }
            set
            {
                Add(key, value);
            }
        }

        /// <summary>
        /// Removes an item from the Trie by Key value.
        /// </summary>
        /// <param name="key"></param>
        public bool Remove(TKey key)
        {
            if (key == null) throw new ArgumentException(nameof(key));

            bool mod;
            root = Remove(root, key.GetEnumerator(), out mod);
            size--;
            if (mod) modified = true;

            return mod;
        }
        /// <summary>Uses an Enumerator(<typeparamref name="TKeyPiece"/>)
        /// to advance through the <typeparamref name="TKey"/></summary>
        protected Node Remove(Node node, IEnumerator<TKeyPiece> enmrtr, out bool mod)
        {
            if (enmrtr.MoveNext())
            {
                TKeyPiece now = enmrtr.Current;

                if (node.Next == null)
                {
                    mod = false;
                    return node;
                }

                // Recurse
                if (node.Next.ContainsKey(now))
                    node.Next[now] = Remove(node.Next[now], enmrtr, out mod);
                else
                    mod = false;
            }
            else
            {
                node.Values.Clear();
                node.IsContainer = false;
                mod = true;
            }
            return node;
        }
        /// <summary>
        /// Ignores the <typeparamref name="TValue"/> or value part of the
        /// <paramref name="item"/>, and removes based only on the 
        /// <typeparamref name="TKey"/> or key part of the <paramref name="item"/>.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
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
        /// Checks if the given <typeparamref name="TKey"/> has been previously been added into the Trie.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(TKey key)
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
        /// Efficiently obtains a <typeparamref name="TValue"/> when 
        /// unsure about its existence in the data structure.
        /// </summary>
        /// <param name="key">The key to check for.</param>
        /// <param name="value">The value to assigned to.</param>
        /// <returns>True if found.</returns>
        public bool TryGetValue(TKey key, out IList<TValue> value)
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

        /// <summary>
        /// Checks if a given pair are in the trie.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            List<TValue> vals = Search(item.Key);
            return vals.Contains(item.Value);
        }

        /// <summary>
        /// Copies the elements of the Trie to an array of type 
        /// KeyValuePair(TKey, TValue), starting at the specified array index.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        /// <exception cref="ArgumentNullException"><paramref name="nameof(array)"/> is <code>null</code>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="nameof(arrayIndex)"/> is less than 0.</exception>
        /// <exception cref="ArgumentException">The number of elements in the source Trie is greater than the 
        /// available space from index to the end of the destination array.</exception>
        public void CopyTo(KeyValuePair<TKey, IList<TValue>>[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (Count - arrayIndex > array.Length) throw new ArgumentException("Not enough space in the given array.");

            _copyTo(array, ref arrayIndex, root);
        }
        protected void _copyTo(KeyValuePair<TKey, IList<TValue>>[] array, ref int arrayIndex, Node node)
        {
            foreach (KeyValuePair<TKeyPiece, Node> pair in node.Next)
            {
                // Check if this is an instance of a pair
                if (pair.Value.IsContainer)
                {
                    TKey key = pair.Value.repValue;
                    IList<TValue> values = pair.Value.Values;

                    // set the current item
                    array[arrayIndex++] = new KeyValuePair<TKey, IList<TValue>>(key, values);
                }

                // Children come after a parent
                if (pair.Value != null)
                    _copyTo(array, ref arrayIndex, pair.Value);
            }
        }


        #region --- IEnumeration Interface Functions ---

        public IEnumerator<KeyValuePair<TKey, IList<TValue>>> GetEnumerator()
        {
            return new TernaryTrieEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new TernaryTrieEnumerator(this);
        }

        public void Add(TKey key, IList<TValue> value)
        {
            throw new NotImplementedException();
        }

        public void Add(KeyValuePair<TKey, IList<TValue>> item)
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<TKey, IList<TValue>> item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<TKey, IList<TValue>> item)
        {
            throw new NotImplementedException();
        }

        public class TernaryTrieEnumerator : IEnumerator<KeyValuePair<TKey, IList<TValue>>>
        {
            // A reference to the trie
            private TernarySearchTrie<TKey, TKeyPiece, TValue> trie;

            // Access controlled current value
            private KeyValuePair<TKey, IList<TValue>> current;
            object o = new object();

            // Recursive stack for iteration over trie
            private Thread iteratorThread;
            private AutoResetEvent waitHandle = new AutoResetEvent(false);

            // Disposing flag
            private bool disp = false;
            private bool started = false;
            private bool Done = false;

            /// <summary>
            /// Obtains the current item being referenced.
            /// </summary>
            public KeyValuePair<TKey, IList<TValue>> Current
            {
                get { return current; }
            }
            private object Current1
            {
                get { return Current; }
            }
            object IEnumerator.Current
            {
                get
                {
                    return Current1;
                }
            }

            /// <summary>
            /// This function does some simple things.
            /// 
            /// Mainly it traverses the Trie left to right, recursively.
            /// Thus the left-most will be visited first and the
            /// right-most nodes last.
            /// 
            /// Whenever it encounters an <typeparamref name="TValue"/>
            /// it will assign it to the current value.
            /// 
            /// A special feature of this function is that it calls 
            /// WaitOne() on a waitHandle object so that the function
            /// stops after it arrives at the next <typeparamref name="TValue"/>
            /// and thus can be used by the function MoveNext().
            /// </summary>
            /// <param name="node"></param>
            private void threadFunction(Node node)
            {
                foreach (KeyValuePair<TKeyPiece, Node> pair in node.Next)
                {
                    if (disp || Done) return;

                    // Check if this is an instance of a pair
                    if (pair.Value.IsContainer)
                    {
                        TKey key = pair.Value.repValue;
                        IList<TValue> values = pair.Value.Values;

                        // set the current item
                        current = new KeyValuePair<TKey, IList<TValue>>(key, values);
                        Monitor.Pulse(o);
                        Monitor.Wait(o);

                    }

                    // Children come after a parent
                    if (pair.Value != null)
                        threadFunction(pair.Value);
                }
            }

            /// <summary>
            /// This starts the thread.
            /// </summary>
            private void ThreadInit()
            {
                Monitor.Enter(o);
                try
                {
                    threadFunction(trie.root);
                }
                finally
                {
                    Done = true;
                    Monitor.Pulse(o);
                    Monitor.Exit(o);
                }
            }

            // Constructor
            public TernaryTrieEnumerator(TernarySearchTrie<TKey, TKeyPiece, TValue> trie)
            {
                this.trie = trie;
                this.trie.modified = false;
                current = default(KeyValuePair<TKey, IList<TValue>>);

                // Create a new Thread and initialize its stack. (Do not start)
                iteratorThread = new Thread(ThreadInit);
            }

            /// <summary>
            /// Prompts the next item to be referenced by #Current.
            /// </summary>
            /// <returns></returns>
            public bool MoveNext()
            {
                // Check for modifications
                if (trie.modified)
                    throw new InvalidOperationException("The collection was modified after the enumerator was created.");

                // Start if unstarted
                if (!started) { iteratorThread.Start(); started = true; }

                // Otherwise we will signal the thread to continue its event stack operations.
                lock (o)
                {
                    Monitor.Pulse(o);
                    Monitor.Wait(o);
                }

                // Figure out what to return based on the thread state.
                return !Done;
            }

            /// <summary>
            /// Resets the enumerator to start from the beginning of the Trie.s
            /// </summary>
            public void Reset()
            {
                trie.modified = false;
                current = default(KeyValuePair<TKey, IList<TValue>>);

                // Create a new Thread and initialize its stack. (Do not start)
                iteratorThread = new Thread(ThreadInit);
            }

            #region IDisposable Support
            private bool disposedValue = false; // To detect redundant calls

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        // TODO: dispose managed state (managed objects).
                    }

                    // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                    // TODO: set large fields to null.
                    disp = true;
                    waitHandle.Set();
                    iteratorThread = null;
                    trie = null;
                    waitHandle.Dispose();

                    disposedValue = true;
                }
            }

            // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
            ~TernaryTrieEnumerator()
            {
                // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                Dispose(false);
            }

            // This code added to correctly implement the disposable pattern.
            public void Dispose()
            {
                // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                Dispose(true);
                // TODO: uncomment the following line if the finalizer is overridden above.
                // GC.SuppressFinalize(this);
            }
            #endregion
        }

        #endregion
    }
}
