using System;
using System.Collections.Generic;

namespace Global.SearchTrie
{
    public partial class TernarySearchTrie<TKeyPiece, TValue> : IDictionary<IEnumerable<TKeyPiece>, IList<TValue>>
        where TKeyPiece : IComparable
    {
        #region --- public ---

        /// <summary>
        /// Adds the given Key/<typeparamref name="TValue" />
        /// pair to the Trie.
        /// </summary>
        /// <param name="key">The location to place the <paramref name="item" />.</param>
        /// <param name="item">The object to place in the trie.</param>
        /// <exception cref="ArgumentNullException">key is null.</exception>
        public void Add(IEnumerable<TKeyPiece> key, TValue item)
        {
            Add(key, new List<TValue>{item});
        }
        /// <summary>Adds the given Key/<typeparamref name="TValue"/>
        /// pair to the Trie.</summary>
        /// <param name="item">The object to place in the trie.</param>
        /// <exception cref="ArgumentNullException">key is null.</exception>
        public void Add(KeyValuePair<IEnumerable<TKeyPiece>, TValue> item)
        {
            Add(item.Key, item.Value);
        }
        /// <summary>
        /// Adds the given set of <typeparamref name="TValue"/>s at the given
        /// Key location.
        /// </summary>
        /// <param name="key">The target location.</param>
        /// <param name="value">The package.</param>
        /// <exception cref="ArgumentNullException">key is null.</exception>
        public void Add(IEnumerable<TKeyPiece> key, IList<TValue> value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            modified = true;
            root = Add(value, root, key.GetEnumerator(), key);
        }
        /// <summary>
        /// Adds a given pair into the dictionary.
        /// </summary>
        /// <param name="item">The pair.</param>
        /// <exception cref="ArgumentNullException">item.key is null.</exception>
        public void Add(KeyValuePair<IEnumerable<TKeyPiece>, IList<TValue>> item)
        {
            Add(item.Key, item.Value);
        }

        #endregion

        #region --- protected ---

        /// <summary>Uses an Enumerator(<typeparamref name="TKeyPiece"/>)
        /// to advance through the Key.</summary>
        protected Node Add(IList<TValue> item, Node node, IEnumerator<TKeyPiece> enmrtr, IEnumerable<TKeyPiece> key)
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
                    Node n = new Node
                    {
                        Key = now
                    };

                    // Go down a level
                    node.Next[now] = Add(item, n, enmrtr, key);
                }
            }
            else
            {
                node.Values.AddRange(item); // Assign the value
                node.repValue = key;
                if (!node.IsContainer)
                {
                    node.IsContainer = true;
                    size++;
                }
            }
            return node;
        }

        #endregion
    }
}
