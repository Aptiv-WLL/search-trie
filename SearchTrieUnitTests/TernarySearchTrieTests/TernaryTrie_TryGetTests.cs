using System;
using System.Collections.Generic;
using Global.RandomLibraries;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Global.SearchTrie.Tests
{
    [TestClass()]
    public class TernaryTrie_TryGetValueTests
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
        public static TernarySearchTrie<char, ulong> Read(string resource)
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
        [TestCategory("TryGetValue")]
        [TestCategory("TernaryTrie")]
        [Description("Verify successful retrieval of <size> items in the Trie.")]
        public void TryGetValueTest0()
        {
            Trie = new TernarySearchTrie<char, int>();
            int pip = size;

            while (pip-- > 0)
            {
                string key = rs.makeRandString();
                int val = r.Next();

                Trie.Add(key, val);

                Assert.IsTrue(Trie.TryGetValue(key, out IList<int> outs));
                Assert.IsTrue(outs.Count > 0);
                Assert.AreEqual(val, outs[outs.Count - 1]);
            }
        }

        [TestMethod()]
        [TestCategory("TryGetValue")]
        [TestCategory("TernaryTrie")]
        [Description("Verify unsuccessful retrieval of <size> items not in the Trie.")]
        public void TryGetValueTest1()
        {
            Trie = new TernarySearchTrie<char, int>();
            var Dict = new Dictionary<string, int>();
            int pip = size;

            while (pip-- > 0)
            {
                string key = rs.makeRandString();
                int val = r.Next();

                while (Dict.ContainsKey(key)) { key = rs.makeRandString(); }

                Trie.Add(key, val);
                Dict.Add(key, val);
            }
            pip = size;
            while (pip-- > 0)
            {
                string key = rs.makeRandString();
                if (Dict.ContainsKey(key)) continue;

                Assert.IsFalse(Trie.TryGetValue(key, out IList<int> vals));
                Assert.IsNull(vals);
            }
        }

        [TestMethod()]
        [TestCategory("TryGetValue")]
        [TestCategory("TernaryTrie")]
        [Description("Verify ArgumentNullException is thrown properly.")]
        public void TryGetValueTest2()
        {
            Trie = new TernarySearchTrie<char, int>();

            bool thrown = false;
            try
            {
                Trie.TryGetValue(null, out IList<int> vals);
            }
            catch
            {
                thrown = true;
            }
            Assert.IsTrue(thrown);
        }
    }
}
