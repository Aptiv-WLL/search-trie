using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Global.SearchTrie
{
    public partial class TernarySearchTrie<TKeyPiece, TValue> : IDictionary<IEnumerable<TKeyPiece>, IList<TValue>>
        where TKeyPiece : IComparable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<IEnumerable<TKeyPiece>, IList<TValue>>> GetEnumerator()
        {
            //return new TernaryTrieEnumerator(this);
            return GetEnumerable(root).GetEnumerator();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            //return new TernaryTrieEnumerator(this);
            return GetEnumerable(root).GetEnumerator();
        }

        /// <summary>
        /// Uses yielding to iterate over the tree.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private IEnumerable<KeyValuePair<IEnumerable<TKeyPiece>, IList<TValue>>> GetEnumerable(Node node)
        {
            // Check this node
            if (node.IsContainer)
            {
                var pair = new KeyValuePair<IEnumerable<TKeyPiece>, IList<TValue>>(node.repValue, node.Values);
                yield return pair;
            }

            // Go through the children
            foreach (KeyValuePair<TKeyPiece, Node> pair in node.Next)
            {
                if (pair.Value != null)
                    foreach (var p in GetEnumerable(pair.Value))
                        yield return p;
            }

            // Check for modifications
            if (modified) yield break;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(KeyValuePair<IEnumerable<TKeyPiece>, IList<TValue>> item)
        {
            return ContainsKey(item.Key) && this[item.Key] == item.Value;
        }

        /// <summary>
        /// 
        /// </summary>
        public class TernaryTrieEnumerator : IEnumerator<KeyValuePair<IEnumerable<TKeyPiece>, IList<TValue>>>
        {
            // A reference to the trie
            private TernarySearchTrie<TKeyPiece, TValue> trie;

            /// <summary>
            /// The Current item pointed to by the enumerator.
            /// </summary>
            private KeyValuePair<IEnumerable<TKeyPiece>, IList<TValue>> current;
            /// <summary>
            /// Current protector.
            /// </summary>
            private static object o = new object();

            /// <summary>
            /// Recursive stack for iteration over trie.
            /// </summary>
            private Thread iteratorThread;

            /// <summary>
            ///  Disposing flag.
            /// </summary>
            private bool disp = false;
            /// <summary>
            /// Identifies whether the recursive iterator thread has started or
            /// not.
            /// </summary>
            private bool started = false;
            /// <summary>
            /// Flag to identify when the end of the Trie is reached or to 
            /// short circuit the iteration.
            /// </summary>
            private bool Done = false;

            private bool assigned;

            /// <summary>
            /// Obtains the current item being referenced.
            /// </summary>
            public KeyValuePair<IEnumerable<TKeyPiece>, IList<TValue>> Current
            {
                get
                {
                    KeyValuePair<IEnumerable<TKeyPiece>, IList<TValue>> c;
                    Monitor.Enter(o);
                    c = current;
                    Monitor.Exit(o);
                    return c;
                }
            }
            private object Current1
            {
                get
                {
                    return Current;
                }
            }
            object IEnumerator.Current
            {
                get
                {
                    return Current1;
                }
            }

            /// <summary>
            /// Construct a new TernaryTrieEnumerator.
            /// </summary>
            /// <param name="trie"></param>
            public TernaryTrieEnumerator(TernarySearchTrie<TKeyPiece, TValue> trie)
            {
                // Assign the Trie
                this.trie = trie;
                this.trie.modified = false;

                // Create a new Thread and initialize its first stack. 
                //(DO NOT START HERE)
                iteratorThread = new Thread(ThreadInitFunction);

                // Assign the default to current
                Monitor.Enter(o);
                current = default(KeyValuePair<IEnumerable<TKeyPiece>, IList<TValue>>);
                Monitor.Exit(o);
            }


            /// <summary>
            /// Starts the recursion thread.
            /// </summary>
            private void StartThread()
            {
                iteratorThread.Start();
                //Thread.Sleep(50);
                started = true;
            }

            /// <summary>
            /// This function does some simple things.
            /// 
            /// <para>Mainly it traverses the Trie left to right, recursively.
            /// Thus the left-most will be visited first and the
            /// right-most nodes last.</para>
            /// 
            /// <para>Whenever it encounters an <typeparamref name="TValue"/>
            /// it will assign it to the current value.</para>
            /// 
            /// <para> A special feature of this function is that it calls 
            /// WaitOne() on a waitHandle object so that the function
            /// stops after it arrives at the next <typeparamref name="TValue"/>
            /// and thus can be used by the function MoveNext().</para>
            /// 
            /// <para>BEWARE:
            /// Any exiting of this function will not leave the lock and thus
            /// must be exited after the return, externally.</para>
            /// </summary>
            /// <param name="node"></param>
            private void ThreadFunction(Node node)
            {
                // See if we are done
                if (disp || Done) return;

                // Check this node
                if (node.IsContainer)
                {
                    // Release the lock on the current object protector
                    Monitor.Exit(o);

                    // NOTE: ONLY GO TO SLEEP WHEN NOT IN THE LOCKED STATE
                    try { Thread.Sleep(Timeout.Infinite); }
                    catch (ThreadInterruptedException) {; }

                    // Obtain the lock on the current object protector
                    Monitor.Enter(o);
                    if (disp) return;

                    current = new KeyValuePair<IEnumerable<TKeyPiece>, IList<TValue>>(node.repValue, node.Values);
                    assigned = true;
                }

                // Go through the children
                foreach (KeyValuePair<TKeyPiece, Node> pair in node.Next)
                {
                    if (disp || Done) return;
                    if (pair.Value != null)
                        ThreadFunction(pair.Value);
                }
            }

            /// <summary>
            /// This starts the thread.
            /// </summary>
            private void ThreadInitFunction()
            {
                // Obtain the initial lock on the current object protector.
                Monitor.Enter(o);
                try
                {
                    // Enter the tree traversal.
                    ThreadFunction(trie.root);
                }
                finally
                {
                    Done = true;

                    // Release the lock on the current object protector.
                    Monitor.Exit(o);
                }
            }

            /// <summary>
            /// Prompts the next item to be referenced by Current.
            /// </summary>
            /// <returns>True if the Current object is not past the end of 
            /// the collection.</returns>
            public bool MoveNext()
            {
                // Check for modifications
                if (trie.modified)
                    throw new InvalidOperationException("The collection was modified after the enumerator was created.");

                // Check for disposal
                if (disp)
                    throw new ObjectDisposedException("TernaryTrieEnumerator");

                // Run the internal move
                return MoveInternal();
            }

            /// <summary>
            /// Prompts the IteratorThread to advance to the next object in the Trie.
            /// </summary>
            /// <returns></returns>
            private bool MoveInternal()
            {
                // Check if we're already done
                if (Done)
                    return false;

                // Start if unstarted
                if (!started)
                {
                    StartThread();
                }

                assigned = false;

                // Obtain the lock and wake the thread
                Monitor.Enter(o);
                iteratorThread.Interrupt();
                Monitor.Exit(o);

                while (!Done && !assigned)
                {
                    ;
                    //Thread.Sleep(10);
                }
                //Thread.Sleep(1);
                
                // Figure out what to return based on the thread state.
                return !Done;
            }

            /// <summary>
            /// Resets the enumerator to start from the beginning of the Trie.
            /// </summary>
            public void Reset()
            {
                if (disp) throw new ObjectDisposedException("TernaryTrieEnumerator");

                // Get the ThreadFunction to return
                Done = true;

                // Obtain the lock and wake the thread
                Monitor.Enter(o);
                iteratorThread.Interrupt();
                Monitor.Exit(o);

                //Thread.Sleep(50);

                // Clean up old values
                Monitor.Enter(o);
                current = default(KeyValuePair<IEnumerable<TKeyPiece>, IList<TValue>>);
                Monitor.Exit(o);

                // Create a new Thread and initialize its stack. (Do not start)
                iteratorThread = new Thread(ThreadInitFunction);
                trie.modified = false;
                started = false;
                Done = false;
            }

            #region IDisposable Support
            private bool disposedValue = false; // To detect redundant calls

            /// <summary>
            /// 
            /// </summary>
            /// <param name="disposing"></param>
            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        // TODO: dispose managed state (managed objects).

                        // Get the ThreadFunction to return
                        disp = true;
                        // Obtain the lock and wake the thread
                        Monitor.Enter(o);
                        iteratorThread.Interrupt();
                        Monitor.Exit(o);

                        //Thread.Sleep(50);
                    }

                    // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                    // TODO: set large fields to null.

                    disposedValue = true;
                }
            }

            // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
            /*~TernaryTrieEnumerator()
            {
                // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                Dispose(false);
            }*/

            // This code added to correctly implement the disposable pattern.
            /// <summary>
            /// 
            /// </summary>
            public void Dispose()
            {
                // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                Dispose(true);
                // TODO: uncomment the following line if the finalizer is overridden above.
                // GC.SuppressFinalize(this);
            }
            #endregion
        }
    }
}
