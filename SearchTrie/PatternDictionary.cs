using System;
using System.Collections.Generic;
using System.Linq;

namespace Global.SearchTrie.Patterns
{
    /// <summary>
    /// A class for the storage and lookup of patterns.
    /// </summary>
    /// <typeparam name="TKeyPiece"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class PatternDictionary<TKeyPiece, TValue>
        : TernarySearchTrie<TKeyPiece, TValue>
        where TKeyPiece : IComparable
    {
        /// <summary>
        /// The piece that matches any TKeyPiece.
        /// </summary>
        public TKeyPiece genericPiece;
        /// <summary>
        /// The piece that matches any ending of a TKey.
        /// </summary>
        public TKeyPiece genericSeriesPiece;

        /// <summary>
        /// Construct a new Pattern Dictionary for storing patterns.
        /// </summary>
        /// <param name="generic">The piece that represents a generic piece.</param>
        /// <param name="series">The piece that represents a series of any pieces.</param>
        public PatternDictionary(TKeyPiece generic, TKeyPiece series)
        {
            genericPiece = generic;
            genericSeriesPiece = series;
        }

        /// <summary>
        /// Return the set of all patterns values that match the given item.
        /// </summary>
        /// <param name="item">The key to match.</param>
        /// <returns>A set of all the Values.</returns>
        public IList<TValue> Collect(IEnumerable<TKeyPiece> item)
        {
            AllVisited = false;
            return Collect(root, item.ToArray(), 0);
        }

        /// <summary>
        /// Returns the set of all patterns values that match the series of
        /// TKeyPieces in the parameter.
        /// </summary>
        /// <param name="pieces">The series of TKeyPieces to match.</param>
        /// <returns></returns>
        public IList<TValue> Collect(IList<TKeyPiece> pieces)
        {
            AllVisited = false;
            return Collect(root, pieces, 0);
        }

        /// <summary>
        /// Collects the set of Values in the trie (<paramref name="n"/>)
        /// matching the <paramref name="pieces"/> starting at index of idx
        /// in the list of <paramref name="pieces"/>.
        /// </summary>
        /// <param name="n">The "root" node of the trie.</param>
        /// <param name="pieces">The list of TKeyPiece's to match.</param>
        /// <param name="idx">The index to start matching to.</param>
        /// <returns>The collection of values matching the pieces.</returns>
        private new IList<TValue> Collect(Node n, IList<TKeyPiece> pieces, int idx)
        {
            if (idx > pieces.Count && !n.Visited) // changed from >=
            {
                n.Visited = true;
                return n.Values;
            }
            else
            {
                List<TValue> items = new List<TValue>();

                while (idx <= pieces.Count)
                {
                    // Let's look at the given node's children:
                    SortedDictionary<TKeyPiece, Node> list = n.Next;

                    // What if there is a "generic pattern" among the next nodes?
                    if (list.ContainsKey(genericPiece))
                    { // Check the branch
                        items.AddRange(Collect(list[genericPiece], pieces, idx + 1));
                    }

                    // What if there is a "generic series pattern" among the next nodes?
                    if (list.ContainsKey(genericSeriesPiece))
                    { /*// Definitely add this guy.
                        items.AddRange(list[genericSeriesPiece].Values);*/

                        // A generic series pattern can match a series of 0 to infinity TKeyPieces
                        for (int k = 0; k+idx <= pieces.Count; k++)
                        {
                            items.AddRange(Collect(list[genericSeriesPiece], pieces, idx + k));

                            /* This collects the current node's values last, when 
                             * `idx + k == pieces.Count`.
                             * 
                             * If we have the pattern * and an TKey of ABCDE then
                             * first we consider that * matches nothing and pass on ABCDE.
                             * Next, we consider that it matches A, and pass on BCDE.
                             * Then, CDE, and so on
                             *      DE
                             *      E
                             * And finally we consider * matches the entire TKey and 
                             * collect the *'s node's values.
                             */
                        }
                    }

                    // Collect values if we're at the end.
                    if (idx == pieces.Count && !n.Visited)
                    {
                        items.AddRange(n.Values);
                        n.Visited = true;
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
    }
}
