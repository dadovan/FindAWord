using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Threading;

namespace BoggleBoardLibrary
{
    /// <summary>
    /// A node used in the trie graph.
    /// NOTE: THIS IMPLEMENTATION IS ENGLISH ONLY AND EXPECTS LOWER CASE ALPHABETIC CHARACTERS ONLY!
    /// </summary>
    /// <remarks>
    /// This ended up heavily optimized for the problem statement as written which calls for English.  I lower case all characters
    /// since the example provided shows lowercase, though the photo of the Boggle board on the provided Wikipedia link shows uppercase.  :)
    ///
    /// I originally defined Children as a Dictionary mapping the char to TrieNode but perf was not what I wanted.  As an experiment, I
    /// switched to using an array instead.  While this is less memory efficient, overall performance of the trie was much improved.
    /// I hide this implementation detail behind the GetChild/SetChild/HasChildren methods in this class so it is easy to swap if
    /// memory is more of a concern than speed.
    /// </remarks>
    public class TrieNode
    {
        /// <summary>
        /// The letter represented by this node
        /// </summary>
        public char Letter;

        /// <summary>
        /// The parent of the node
        /// </summary>
        public TrieNode Parent;

        /// <summary>
        /// Stores the set of our child nodes, if any
        /// </summary>
        private TrieNode[] m_children;

        /// <summary>
        /// True if this node completes a word.
        /// Note there may still be children if other words derive from this one
        /// </summary>
        public bool CompletesWord;

        /// <summary>
        /// True if this node has child nodes
        /// </summary>
        public bool HasChildren => m_children != null;

        /// <summary>
        /// Gets the index in the child array of given the character
        /// Internal only.
        /// </summary>
        /// <param name="c">The character</param>
        /// <returns>The index in the child array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetIndex(char c) => c - 'a';

        /// <summary>
        /// Sets the value of the child node corresponding to the character
        /// </summary>
        /// <param name="c">The character represented by <see cref="node"/></param>
        /// <param name="node">The node</param>
        public void SetChild(char c, TrieNode node)
        {
            // If we don't have any children yet, create the array
            if (m_children == null)
                m_children = new TrieNode[26];
            m_children[GetIndex(c)] = node;
        }

        /// <summary>
        /// Gets the value of the child node corresponding to the character, if any
        /// </summary>
        /// <param name="c">The character to lookup</param>
        /// <returns>The child node, if one is present</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TrieNode GetChild(char c) => m_children?[GetIndex(c)];

        /// <summary>
        /// Gets the word (or partial word) represented by this node
        /// </summary>
        /// <returns>The word (or partial word)</returns>
        public string GetWord()
        {
            // TODO: We should be able to speed this string concatenation up
            string text = "";
            TrieNode node = this;
            // m_root has no Letter value so \0
            while ((node != null) && (node.Letter != '\0'))
            {
                text = node.Letter + text;
                node = node.Parent;
            }
            return text;
        }
    }

    /// <summary>
    /// A general-purpose trie data structure.
    /// </summary>
    /// <remarks>
    /// Time complexity for adds and lookups are O(log n) I believe since this is a tree.  I want to avoid looking up anything that may clarify
    ///     this for a non-binary tree or trie in order to avoid any potential contamination.
    /// Space complexity for adds is O(26(n-1)) worst case where n is the number of characters in the word.  For performance, each node in the trie
    ///     that has children allocates a TrieNode[26] array.  So worst case (a fresh trie, for instance) each letter except the last creates a new
    ///     TrieNode[26] array.  I admit I'm not sure how to calculate average complexity because it entirely depends on the data supplied.  Like
    ///     time complexity above, I don't want to look up any reference information on this to avoid contaminating my solution.
    ///     For more information, see <see cref="TrieNode"/> below.
    /// </remarks>
    public class Trie
    {
        /// <summary>
        /// The root node for this trie
        /// </summary>
        private readonly TrieNode m_root = new TrieNode();

        /// <summary>
        /// A count of the total number of lookups that have occured.
        /// </summary>
        /// <remarks>
        /// I use this for a cheap locking mechanism.  Once lookups start, we no longer allow modification to the trie.
        /// Not 100% foolproof but very close.
        /// </remarks>
        private int m_lookupCount;

        /// <summary>
        /// Constructs a new trie containing the supplied words
        /// </summary>
        /// <remarks>
        /// I made this a factory method instead of a constructor overload since constructors are supposed to execute quickly and this ... may not.
        /// </remarks>
        /// <param name="words">The word list to initialize the trie with</param>
        public static Trie Create(IEnumerable<string> words)
        {
            Trie trie = new Trie();
            foreach (string word in words)
                trie.Add(word);
            return trie;
        }

        /// <summary>
        /// Adds a new word to the trie
        /// </summary>
        /// <param name="word">The word to add</param>
        public void Add(string word)
        {
            if (m_lookupCount > 0)
                throw new ReadOnlyException("Lookups have already begun.  The trie is now in read-only mode.");
            TrieNode current = m_root;
            foreach (char c in word)
            {
                TrieNode nextNode = current.GetChild(c);
                if (nextNode == null)
                {
                    nextNode = new TrieNode() { Letter = c, Parent = current };
                    current.SetChild(c, nextNode);
                }
                current = nextNode;
            }
            current.CompletesWord = true;
        }

        /// <summary>
        /// Tries to find the node for the last character in the string.
        /// </summary>
        /// <param name="text">The string to search</param>
        /// <param name="node">The node for the final character, if present</param>
        /// <returns>True if the node was found, false otherwise</returns>
        private bool TryGetFinalNode(string text, out TrieNode node)
        {
            node = null;
            if (String.IsNullOrWhiteSpace(text))
                return false;

            TrieNode current = m_root;
            foreach (char c in text)
            {
                current = current.GetChild(c);
                if (current == null)
                    return false;
            }

            node = current;
            return true;
        }

        /// <summary>
        /// Tests if the given string is a valid word or the start of a word
        /// </summary>
        /// <param name="text">The string to test</param>
        /// <returns>True if the string is a word or word start</returns>
        public bool IsValidWordStart(string text)
        {
            Interlocked.Increment(ref m_lookupCount);
            return TryGetFinalNode(text, out _);
        }

        /// <summary>
        /// Tests if a string is a valid word
        /// </summary>
        /// <param name="text">The string to test</param>
        /// <returns>True if the string is a word</returns>
        public bool IsWord(string text)
        {
            Interlocked.Increment(ref m_lookupCount);
            if (TryGetFinalNode(text, out TrieNode node))
                return node.CompletesWord;
            return false;
        }

        /// <summary>
        /// Given a <see cref="TrieNode"/> and an upcoming char, determines whether this represents a word or a potential word.
        /// </summary>
        /// <param name="node">The <see cref="TrieNode"/> representing the base</param>
        /// <param name="c">The upcoming char</param>
        /// <param name="isPotential">True if the string is a word start</param>
        /// <param name="completesWord">True if the string is a word</param>
        public TrieNode IsWordOrWordStart(TrieNode node, char c, out bool isPotential, out bool completesWord)
        {
            Interlocked.Increment(ref m_lookupCount);

            if (node == null)
                node = m_root;
            node = node.GetChild(c);
            if (node != null)
            {
                isPotential = true;
                completesWord = node.CompletesWord;
                return node;
            }
            isPotential = completesWord = false;
            return null;
        }
    }
}
