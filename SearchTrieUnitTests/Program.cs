using System;

namespace Global.SearchTrie.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            TernarySearchTrie<char, ulong> HuckFinn = read(Properties.Resources.huck_finn);
            TernarySearchTrie<char, ulong> Bible = read(Properties.Resources.bible_asv_utf8);
            TernarySearchTrie<char, ulong> TomSawyer = read(Properties.Resources.tom_sawyer);
        }

        /// <summary>
        /// Read in a given source file into a trie of strings.
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        private static TernarySearchTrie<char, ulong> read(string resource)
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
    }
}
