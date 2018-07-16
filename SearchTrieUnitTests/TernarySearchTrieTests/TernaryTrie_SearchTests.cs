using Global.RandomLibraries;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Global.SearchTrie.Tests
{
    /// <summary>
    /// Tests the Searching functionality of the Ternary Search Trie.
    /// </summary>
    [TestClass]
    public class TernaryTrie_SearchTests
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
                new_trie.Add(rs.makeRandString(), i);
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

        [TestMethod(),TestCategory("Searching"),TestCategory("TernaryTrie")]
        [Description("Random strings input.")]
        public void SearchTest0()
        {
            Trie = new TernarySearchTrie<char, int>();

            List<string> bunch = new List<string>();
            for (int i = 0; i < size; i++)
            {
                string v = rs.makeRandString();
                bunch.Add(v);
                Trie.Add(v, i);

                List<int> finds = Trie.Search(v);
                Assert.IsTrue(finds.Contains(i));
            }

            foreach (string it in bunch)
            {
                Assert.IsTrue(Trie.Search(it).Count > 0);
            }

            Trie.Clear();
            Assert.IsTrue(Trie.Count == 0);
        }

        [TestMethod(),TestCategory("Searching"),TestCategory("TernaryTrie")]
        [Description("Test the default char matching.")]
        public void SearchTest1()
        {
            Trie = new TernarySearchTrie<char, int>();

            Trie.Clear();
            Trie.Add("a", 1);
            Trie.Add("b", 2);
            Trie.Add("ab", 3);
            Trie.Add("abc", 4);
            Trie.Add("hello!", 5);
            Trie.Add("hell", 6);
            Trie.Add("help", 7);

            Assert.AreEqual(Trie.Search("hel").Count, 0);
            Assert.AreEqual(Trie.Search("").Count, 0);
        }

        [TestMethod(),TestCategory("Searching"),TestCategory("TernaryTrie")]
        [Description("Test with large volume.")]
        public void SearchTest2()
        {
            TernarySearchTrie<char, ulong> TomSawyer = read(Properties.Resources.tom_sawyer);
            foreach (string key in TomSawyer.Keys)
            {
                Assert.AreNotEqual(null, key, "enumeration produced null key: " + key);

                List<ulong> results = TomSawyer.Search(key);
                Assert.IsTrue(results.Count > 0);
            }
        }
    }
}
