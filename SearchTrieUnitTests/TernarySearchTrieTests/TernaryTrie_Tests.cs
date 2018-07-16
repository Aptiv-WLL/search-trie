using Global.RandomLibraries;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Global.SearchTrie.Tests
{
    [TestClass()]
    public class TernarySearchTrieTests
    {
        #region --- Parameters ---
        private TernarySearchTrie<char, int> Trie = new TernarySearchTrie<char, int>();
        private RandomString rs = new RandomString();
        private Random r = new Random();
        private int size = Properties.Settings.Default.TestSize;
        public TernarySearchTrie<char, int> fill(int count = -1)
        {
            if (count == -1) count = size;

            TernarySearchTrie<char, int> new_trie = new TernarySearchTrie<char, int>();
            for (int i = 0; i < count; i++)
            {
                string s = rs.makeRandString();
                while (new_trie.ContainsKey(s)) s = rs.makeRandString();

                new_trie.Add(s, i);
            }
            return new_trie;
        }
        public static TernarySearchTrie<char, ulong> read(string resource)
        {
            var trie = new TernarySearchTrie<char, ulong>();
            ulong counter = 0;
            try
            {
                foreach (string wordy in resource.Split())
                {
                    string word = wordy.Trim("\".;:',/?()*![]".ToCharArray());
                    trie.Add(word, counter++);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
            return trie;
        }
        #endregion

        [TestMethod()]
        [TestCategory("TernaryTrie")]
        [Description("Verify the constructor.")]
        public void ConstructorTest0()
        {
            Trie = new TernarySearchTrie<char, int>();
            Assert.AreEqual(0, Trie.Count);
            Assert.AreEqual(false, Trie.IsReadOnly);
            Assert.AreEqual(0, Trie.Keys.Count);
            Assert.AreEqual(0, Trie.Values.Count);
            Assert.AreEqual(0, Trie.Search("").Count);
        }

        [TestMethod()]
        [TestCategory("TernaryTrie")]
        [Description("Verify Clear() works.")]
        public void ClearTest0()
        {
            TernarySearchTrie<char, int> Trie = new TernarySearchTrie<char, int>();
            Assert.AreEqual(Trie.Count, 0);

            for (int i = 0; i < size; ++i)
            {
                for (int j = 0; j < i; ++j)
                {
                    string s = rs.makeRandString();
                    Trie.Add(s, j);

                    Trie.Clear();
                    Assert.AreEqual(Trie.Count, 0);
                    Assert.AreEqual(Trie.Search(s).Count, 0);
                }
            }
        }

        [TestMethod()]
        [TestCategory("TernaryTrie")]
        [Description("Verify <size> insertions can be found after insertion.")]
        public void ContainsKeyTest0()
        {
            int count = size;
            var trie = new TernarySearchTrie<char, int>();
            List<string> strs = new List<string>();
            
            for (int i = 0; i < count; i++)
            {
                strs.Add(rs.makeRandString());
                trie.Add(strs[i], i);
            }

            foreach (string key in strs)
            {
                Assert.IsTrue(trie.ContainsKey(key));
            }
        }

        [TestMethod()]
        [TestCategory("TernaryTrie")]
        [Description("Compare functionality to that of a Dictionary.")]
        public void ContainsTest0()
        {
            int count = size;
            var trie = new TernarySearchTrie<char, int>();
            var pairs = new KeyValuePair<IEnumerable<char>, int>[count];
            var dict = new Dictionary<IEnumerable<char>, int>();

            for (int i = 0; i < count; i++)
            {
                do
                    pairs[i] = new KeyValuePair<IEnumerable<char>, int>(rs.makeRandString(), i);
                while (trie.ContainsKey(pairs[i].Key));

                trie[pairs[i].Key] = new List<int> { pairs[i].Value };
                dict[pairs[i].Key] = pairs[i].Value;
            }

            for (int i = 0; i < count; i++)
            {
                Assert.IsTrue(dict.Contains(pairs[i]));
                Assert.IsTrue(trie.Contains(pairs[i]));
            }
        }

        [TestMethod()]
        [TestCategory("TernaryTrie")]
        [Description("Verify the copyTo function.")]
        public void CopyToTest0()
        {
            int count = size;
            var trie = new TernarySearchTrie<char, double>();
            var dict = new Dictionary<IEnumerable<char>, double>();
            var pairs = new KeyValuePair<IEnumerable<char>, double>[count];

            while (count-- > 0)
            {
                do
                {
                    pairs[count] = new KeyValuePair<IEnumerable<char>, double>(rs.makeRandString(), count);
                } while (trie.ContainsKey(pairs[count].Key));

                trie.Add(pairs[count]);
                dict.Add(pairs[count].Key, pairs[count].Value);
            }
            count = size;

            var pairsArray = new KeyValuePair<IEnumerable<char>, IList<double>>[count];
            trie.CopyTo(pairsArray, 0);

            while (count-- > 0)
            {
                IEnumerable<char> key = pairs[count].Key;
                double value = pairs[count].Value;

                Assert.IsNotNull(key);

                Assert.IsTrue(dict.ContainsKey(key));
                Assert.AreEqual(dict[key], value);

                Assert.IsTrue(trie.ContainsKey(key));
                Assert.IsTrue(trie[key].Contains(value));
            }
        }

        [TestMethod(),TestCategory("TernaryTrie"),TestCategory("Enumerator")]
        [Description("Test the threaded enumerator with <size>.")]
        public void GetEnumeratorTest0()
        {
            int count = size;
            var trie = new TernarySearchTrie<char, double>();

            var pairs = new Dictionary<IEnumerable<char>, double>();

            for (int i = 0; i < count; i++)
            {
                string str;
                do { str = rs.makeRandString(); }
                while (trie.ContainsKey(str));

                pairs.Add(str, i);
                trie.Add(str, i);
            }
            foreach (KeyValuePair<IEnumerable<char>, IList<double>> pair in trie)
            {
                Assert.IsNotNull(pair);
                Assert.IsNotNull(pair.Key);
                Assert.IsNotNull(pair.Value);

                Assert.IsTrue(pairs.ContainsKey(pair.Key));
                Assert.AreEqual(pairs[pair.Key], pair.Value[0]);
            }
        }

        [TestMethod(),TestCategory("TernaryTrie"),TestCategory("Enumerator")]
        [Description("Test the threaded enumerator with 100000.")]
        public void GetEnumeratorTest1()
        {
            int count = 10000;
            var trie = new TernarySearchTrie<char, double>();

            var pairs = new Dictionary<IEnumerable<char>, double>();

            for (int i = 0; i < count; i++)
            {
                string str;
                do
                {
                    str = rs.makeRandString();
                } while (trie.ContainsKey(str));

                pairs.Add(str, i);
                trie.Add(str, i);
            }
            foreach (KeyValuePair<IEnumerable<char>, IList<double>> pair in trie)
            {
                Assert.IsNotNull(pair);
                Assert.IsNotNull(pair.Key);
                Assert.IsNotNull(pair.Value);

                Assert.IsTrue(pairs.ContainsKey(pair.Key));
                Assert.AreEqual(pairs[pair.Key], pair.Value[0]);
            }
        }

        [TestMethod(),TestCategory("TernaryTrie"),TestCategory("Enumerator")]
        [Description("Check that the enumerator throws exception when the datastructure is modified.")]
        public void GetEnumeratorTest2()
        {
            int count = size;
            Trie = fill(count);
            bool thrown = false;

            try
            {
                foreach (KeyValuePair<IEnumerable<char>, IList<int>> pair in Trie)
                {
                    Trie.Add(pair);
                }
            }
            catch (InvalidOperationException)
            {
                thrown = true;
            }
            Assert.IsTrue(thrown);
        }

        [TestMethod(),TestCategory("TernaryTrie"),TestCategory("Enumerator")]
        [Description("Check that the enumerator is accurate with zero entries.")]
        public void GetEnumeratorTest3()
        {
            Trie = new TernarySearchTrie<char, int>();
            bool isEmpty = true;
            foreach (KeyValuePair<IEnumerable<char>, IList<int>> pair in Trie)
            {
                isEmpty = false;
            }
            Assert.IsTrue(isEmpty);
        }

        [TestMethod(), TestCategory("TernaryTrie"), TestCategory("Enumerator")]
        [Description("Check that the enumerator is accurate with <size> entries.")]
        public void GetEnumeratorTest4()
        {
            Trie = fill(size+1);
            int count = 0;
            foreach (var item in Trie) count++;
            Assert.AreEqual(Trie.Count, count);
        }

        [TestMethod()]
        [TestCategory("TernaryTrie")]
        [Description("Make sure that the Keys parameter has the right number of items.")]
        public void KeysTest0()
        {
            Trie = fill(size);
            Assert.AreEqual(Trie.Count, Trie.Keys.Count);
        }

        [TestMethod()]
        [TestCategory("TernaryTrie")]
        [Description("Make sure that the Keys parameter has an accurate record of the keys in the Trie.")]
        public void KeysTest1()
        {
            Trie = new TernarySearchTrie<char, int>();
            Dictionary<IEnumerable<char>, int> Dict = new Dictionary<IEnumerable<char>, int>();
            int count = size;

            while (count-- > 0)
            {
                string s = rs.makeRandString();
                int i = r.Next();
                if (!Dict.ContainsKey(s))
                {
                    Dict.Add(s, i);
                    Trie.Add(s, i);
                }
            }

            ICollection<IEnumerable<char>> keys = Trie.Keys;
            foreach (string dk in Dict.Keys)
            {
                Assert.IsTrue(keys.Contains(dk));
            }
        }

        [TestMethod()]
        [TestCategory("TernaryTrie")]
        [Description("Make sure that the Values parameter has a number of items that makes sense.")]
        public void ValuesTest0()
        {
            Trie = fill(size);
            Assert.IsTrue(Trie.Values.Count >= Trie.Count);
        }
    }
}