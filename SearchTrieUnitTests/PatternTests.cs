using Global.SearchTrie.Patterns;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Global.SearchTrie.Tests
{
    [TestClass]
    public class PatternTests
    {
        PatternDictionary<char, int> PatDict;

        [TestMethod, TestCategory("Patterns"), Description("Test strings without generics.")]
        public void PatternTest0()
        {
            PatDict = new PatternDictionary<char, int>('X', '*')
            {
                { "hey", 0 }
            };
            var finds = PatDict.Collect("hey");
            Assert.AreEqual(1, finds.Count);
        }

        [TestMethod, TestCategory("Patterns"), Description("Test Generic Pieces.")]
        public void PatternTest1()
        {
            PatDict = new PatternDictionary<char, int>('X', '*')
            {
                { "", 0 },
                { "0X 12 23 34", 1 },
                { "01 12 2X 34", 2 },
                { "XX XX XX XX", 3 }
            };

            var finds = PatDict.Collect("01 12 23 34");
            Assert.AreEqual(3, finds.Count);
            Assert.IsTrue(finds.Contains(1));
            Assert.IsTrue(finds.Contains(2));
            Assert.IsTrue(finds.Contains(3));

            finds = PatDict.Collect("FF FF FF FF");
            Assert.AreEqual(1, finds.Count);
            Assert.IsTrue(finds.Contains(3));
        }

        [TestMethod, TestCategory("Patterns"), Description("Test Generic Series")]
        public void PatternTest2()
        {
            PatDict = new PatternDictionary<char, int>('X', '*')
            {
                { "*", 1 },
                { "A*", 2 },
                { "AB*", 3 },
                { "AC*", 4 },
                { "aksjdf;oiajdfkjas*", 5 },
                { "", 6 }
            };

            var finds = PatDict.Collect("A");
            Assert.AreEqual(2, finds.Count);
            Assert.IsTrue(finds.Contains(1));
            Assert.IsTrue(finds.Contains(2));

            finds = PatDict.Collect("AB");
            Assert.AreEqual(3, finds.Count);
            Assert.IsTrue(finds.Contains(1));
            Assert.IsTrue(finds.Contains(2));
            Assert.IsTrue(finds.Contains(3));

            finds = PatDict.Collect("aksjdf;oiajdfkjas");
            Assert.AreEqual(2, finds.Count);
            Assert.IsTrue(finds.Contains(1));
            Assert.IsTrue(finds.Contains(5));

            finds = PatDict.Collect("aksjdf;oiajdfkjas_hey!!!!");
            Assert.AreEqual(2, finds.Count);
            Assert.IsTrue(finds.Contains(1));
            Assert.IsTrue(finds.Contains(5));
        }

        [TestMethod, TestCategory("Patterns"), Description("Test Generic Series for inlaid *'s")]
        public void PatternTest3()
        {
            PatDict = new PatternDictionary<char, int>('X', '*')
            {
                { "*", 1 },
                { "*A", 2 },
                { "A*", 3 }
            };
            var finds = PatDict.Collect("A");
            Assert.AreEqual(3, finds.Count);
            Assert.IsTrue(finds.Contains(1) && finds.Contains(2) && finds.Contains(3));
        }

        [TestMethod, TestCategory("Patterns"), Description("Test Generic Series for inlaid *'s, longer")]
        public void PatternTest4()
        {
            PatDict = new PatternDictionary<char, int>('X', '*')
            {
                { "*X*X*", 1 }
            };

            var finds = PatDict.Collect("A");
            Assert.AreEqual(0, finds.Count);

            var finds2 = PatDict.Collect("AA");
            Assert.AreEqual(1, finds2.Count);
            Assert.IsTrue(finds2.Contains(1));
        }

        [TestMethod, TestCategory("Patterns"), Description("Test Generic Series for inlaid *'s, longerer")]
        public void PatternTest5()
        {
            PatDict = new PatternDictionary<char, int>('X', '*')
            {
                { "A*A*B", 1 }
            };

            var finds = PatDict.Collect("AAB");
            Assert.AreEqual(1, finds.Count);
            Assert.IsTrue(finds.Contains(1));

            finds = PatDict.Collect("AAAAAAACCCCCAAAABBBB");
            Assert.AreEqual(1, finds.Count);
            Assert.IsTrue(finds.Contains(1));
        }

        [TestMethod, TestCategory("Patterns"), Description("Test Generic Series for inlaid *'s, longerer")]
        public void PatternTest6()
        {
            PatDict = new PatternDictionary<char, int>('X', '*')
            {
                { "A*A*B*", 1 }
            };

            var finds = PatDict.Collect("AAB");
            Assert.AreEqual(1, finds.Count);
            Assert.IsTrue(finds.Contains(1));

            finds = PatDict.Collect("AAAAAAACCCCCAAAABBBB");
            Assert.AreEqual(1, finds.Count);
            Assert.IsTrue(finds.Contains(1));
        }

        [TestMethod, TestCategory("Patterns"), Description("Test Generic Series for inlaid *'s, longerer")]
        public void PatternTest7()
        {
            PatDict = new PatternDictionary<char, int>('X', '*')
            {
                { "A*A*X*", 1 }
            };

            var finds = PatDict.Collect("AAB");
            Assert.AreEqual(1, finds.Count);
            Assert.IsTrue(finds.Contains(1));

            finds = PatDict.Collect("AAAAAAACCCCCAAAABBBB");
            Assert.AreEqual(1, finds.Count);
            Assert.IsTrue(finds.Contains(1));
        }
    }
}
