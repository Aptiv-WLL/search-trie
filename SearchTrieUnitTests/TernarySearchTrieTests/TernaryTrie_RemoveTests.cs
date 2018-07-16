using System;
using System.Collections.Generic;
using Global.RandomLibraries;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Global.SearchTrie.Tests
{
    [TestClass]
    public class TrieTests_Remove
    {
        #region --- Parameters ---
        private TernarySearchTrie<char, int> Trie = new TernarySearchTrie<char, int>();
        private RandomString rs = new RandomString();
        private Random r = new Random();
        private int size = Properties.Settings.Default.TestSize;
        public TernarySearchTrie<char, int> Fill(int count = -1)
        {
            if (count == -1) count = size;

            TernarySearchTrie<char, int> new_trie = new TernarySearchTrie<char, int>();
            for (int i = 0; i < count; i++)
            {
                new_trie.Add(rs.makeRandString(), i);
            }
            return new_trie;
        }
        #endregion

        [TestMethod()]
        [TestCategory("Removing")]
        [TestCategory("TernaryTrie")]
        [Description("Verify removal of one item.")]
        public void RemoveTest0()
        {
            Trie = new TernarySearchTrie<char, int>
            {
                { "hello", 0 }
            };
            bool res = Trie.Remove("hello");

            Assert.IsTrue(res);
            Assert.AreEqual(Trie.Count, 0);
        }

        [TestMethod()]
        [TestCategory("Removing")]
        [TestCategory("TernaryTrie")]
        [Description("Add and remove on object.")]
        public void RemoveTest1()
        {
            string key = "Hello, world!";
            var trie = new TernarySearchTrie<char, int>
            {
                { key, 0 }
            };
            trie.Remove(key);

            Assert.AreEqual(trie.Count, 0);
            Assert.AreEqual(trie.Search(key).Count, 0);
        }

        [TestMethod()]
        [TestCategory("Removing")]
        [TestCategory("TernaryTrie")]
        [Description("Verify <size> removals.")]
        public void RemoveTest2()
        {
            Trie = new TernarySearchTrie<char, int>();
            var Dict = new Dictionary<IEnumerable<char>, IList<int>>();
            int pip = size;
            while (pip-- > 0)
            {
                string key = rs.makeRandString();
                IList<int> value = new List<int>{r.Next()};
                if (!Dict.ContainsKey(key))
                {
                    Trie.Add(key, value);
                    Dict.Add(key, value);
                }
            }

            pip = size;
            var enu = Dict.GetEnumerator();
            while (enu.MoveNext())
            {
                var pair = enu.Current;
                int size_bef = Trie.Count;

                Trie.Remove(pair);

                Assert.AreEqual(size_bef - 1, Trie.Count);
            }
        }

        [TestMethod()]
        [TestCategory("Removing")]
        [TestCategory("TernaryTrie")]
        [Description("Check ArgumentNullException.")]
        public void RemoveTest3()
        {
            bool thrown = false;
            try
            {
                Trie.Remove(null);
            }
            catch (ArgumentNullException)
            {
                thrown = true;
            }

            Assert.IsTrue(thrown);
        }

        [TestMethod()]
        [TestCategory("Removing")]
        [TestCategory("TernaryTrie")]
        [Description("Remove 100,000")]
        public void RemoveTest4()
        {
            int count = 10000;
            Dictionary<int, string> strs = new Dictionary<int, string>();
            for (int i = 0; i < count; i++)
            {
                string s = rs.makeRandString();
                while (strs.ContainsValue(s)) s = rs.makeRandString();
                strs.Add(i, s);
            }
            var trie = new TernarySearchTrie<char, int>();
            for (int i = 0; i < count; i++) trie.Add(strs[i], i);

            for (int i = 0; i < count; i++)
            {
                trie.Remove(strs[i]);
                Assert.AreEqual(trie.Count, count - 1 - i);
                Assert.AreEqual(trie.Search(strs[i]).Count, 0);
            }
        }

        [TestMethod()]
        [TestCategory("Removing")]
        [TestCategory("TernaryTrie")]
        [Description("Verify removal of single elements rather than list of elements in value")]
        public void RemoveTest5()
        {
            Trie = new TernarySearchTrie<char, int>
            {
                // Part 1
                { "hello", 0 },
                { "hello", 1 }
            };
            bool res = Trie.Remove("hello", 1);

            Assert.IsTrue(res);
            Assert.AreEqual(Trie.Count, 1);
            KeyValuePair<IEnumerable<char>, int> keyValuePair = new KeyValuePair<IEnumerable<char>, int>("hello", 0);
            Assert.IsTrue(Trie.Contains(keyValuePair));

            res = Trie.Remove("hello", 2);
            Assert.IsFalse(res);
            res = Trie.Remove("hello", 0);
            Assert.IsTrue(res);
            Assert.AreEqual(Trie.Count, 0);
            // Part 2
            Trie.Add("hello", 0);
            Trie.Add("world", 0);
            res = Trie.Remove("world", 0);
            Assert.IsTrue(res);
            Assert.AreEqual(Trie.Count, 1);
            res = Trie.Remove("hello", 0);
            Assert.IsTrue(res);
            Assert.AreEqual(Trie.Count, 0);
        }
    }
}
