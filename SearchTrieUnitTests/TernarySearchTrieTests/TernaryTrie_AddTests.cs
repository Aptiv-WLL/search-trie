using Global.RandomLibraries;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Global.SearchTrie.Tests
{
    [TestClass()]
    public class TrieTests_Add
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
                string s = rs.makeRandString();
                while (new_trie.ContainsKey(s)) s = rs.makeRandString();

                new_trie.Add(s, i);
            }
            return new_trie;
        }
        #endregion

        [TestMethod(), TestCategory("Adding"), TestCategory("TernaryTrie")]
        [Description("Verify insertion of individuals.")]
        public void AddTest0()
        {
            for (int i = 0; i < size; i++)
            {
                string s = rs.makeRandString();
                while (Trie.ContainsKey(s)) s = rs.makeRandString();

                Trie.Add(s, i);

                // When adding an item the size should update on return.
                Assert.AreEqual(Trie.Count, i + 1);

                // The item should be immediately available for searching.
                Assert.IsTrue(Trie.Search(s).Count > 0);

                // The item should have the correct value.
                Assert.IsTrue(Trie.Search(s).Contains(i));
            }
        }

        [TestMethod(), TestCategory("Adding"), TestCategory("TernaryTrie")]
        [Description("Verify 10,000 insertions.")]
        public void AddTest1()
        {
            TernarySearchTrie<char, int> Trie = new TernarySearchTrie<char, int>();

            int count = 10000;
            Trie = Fill(count);
            Assert.AreEqual(count, Trie.Count);
        }

        [TestMethod(), TestCategory("Adding"), TestCategory("TernaryTrie")]
        [Description("Characterize insertion speeds.")]
        public void AddTest2()
        {
            /*

            // Graph objects
            GraphView graphForm = new GraphView();
            Gaussian gauss = new Gaussian();

            int init = 10;
            int factor = 2;
            var trie = new TernarySearchTrie<char, int>();

            // Timing objects
            Stopwatch watch = new Stopwatch();
            Process p = Process.GetCurrentProcess();

            while (init < 10000)
            {
                // Generate a set of strings

                string[] strs = new string[init];
                for (int i = 0; i < init; i++)
                    strs[i] = rs.makeRandString();

                watch.Start();
                double startUserProcessorTm = p.UserProcessorTime.TotalMilliseconds;

                foreach (string word in strs) trie.Add(word, 0);

                double endUserProcessorTm = p.UserProcessorTime.TotalMilliseconds;
                watch.Stop();

                // Get the timespans
                double CPU_Time = endUserProcessorTm - startUserProcessorTm;
                double Clock_Time = watch.Elapsed.TotalMilliseconds;

                // Add the coordinate
                int x = init;
                double y = CPU_Time;
                graphForm.Chart.Series[0].Points.AddXY(x, y);
                Debug.WriteLine("{0}\t{1}", x, y);

                // Reset
                watch.Reset();
                init *= factor;
            }

            graphForm.ShowInTaskbar = false;
            //graphForm.ShowDialog();*/
        }

        [TestMethod(), TestCategory("Adding"), TestCategory("TernaryTrie")]
        [Description("Place sets of objects in a name.")]
        public void AddTest3()
        {
            int count = 0;

            while (count < size)
            {
                Trie = new TernarySearchTrie<char, int>();
                string word = rs.makeRandString();
                List<int> vals = new List<int>();

                int a = r.Next(0, 10);
                for (int i = 0; i < a; i++)
                    vals.Add(r.Next());

                Trie.Add(word, vals);

                Assert.AreEqual(Trie[word].Count, a);

                count += a;
            }
        }

        [TestMethod(), TestCategory("Adding"), TestCategory("TernaryTrie")]
        [Description("Try a different set of types.")]
        public void AddTest4()
        {
            var tst = new TernarySearchTrie<byte, string>();
            int size_ = size;
            while (size_-- > 0)
            {
                byte[] ba = new byte[r.Next(1, 20)];
                for (int i = 0; i < ba.Length; i++)
                    ba[i] = (byte)r.Next(0, 0xFF);

                List<string> list = new List<string>
                {
                    rs.makeRandString()
                };
                tst.Add(ba, list);
                tst.Add(ba, rs.makeRandString());

                IList<string> res = tst[ba];
                Assert.AreEqual(res.Count%2, 0);
            }
        }

        [TestMethod(), TestCategory("Adding"), TestCategory("TernaryTrie")]
        [Description("Check ArgumentNullException")]
        public void AddTest5()
        {
            bool thrown = false;
            try
            {
                Trie.Add(null, 0);
            }
            catch (ArgumentNullException)
            {
                thrown = true;
            }
            Assert.IsTrue(thrown);
        }

        [TestMethod(), TestCategory("Adding"), TestCategory("TernaryTrie")]
        [Description("Test adding an empty string")]
        public void AddTest6()
        {
            Trie = new TernarySearchTrie<char, int>();
            int count = size;
            while (count-- > 0)
            {
                Trie.Add("", count);
            }
            Assert.AreEqual(1, Trie.Count);
            Assert.AreEqual(size, Trie.Search("").Count);
        }
    }
}
