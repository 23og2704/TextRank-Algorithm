using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ImprovedRank
{
    public class Graph
    {
        private Dictionary<string, HashSet<string>> adjList;

        public Graph()
        {
            adjList = new Dictionary<string, HashSet<string>>();
        }

        public void AddEdge(string word1, string word2)
        {
            if (word1 == word2)
                return; // Avoid self-loops

            if (!adjList.ContainsKey(word1))
                adjList[word1] = new HashSet<string>();

            if (!adjList.ContainsKey(word2))
                adjList[word2] = new HashSet<string>();

            adjList[word1].Add(word2);
            adjList[word2].Add(word1);
        }

        public Dictionary<string, HashSet<string>> GetAdjList()
        {
            return adjList;
        }

        public Dictionary<string, double> CalculatePageRank(double dampingFactor, int iterations)
        {
            var nodeScores = adjList.Keys.ToDictionary(node => node, node => 1.0);

            for (int i = 0; i < iterations; i++)
            {
                var updatedScores = new Dictionary<string, double>();

                foreach (var node in adjList.Keys)
                {
                    double score = (1 - dampingFactor);
                    foreach (var neighbor in adjList[node])
                    {
                        score += dampingFactor * (nodeScores[neighbor] / adjList[neighbor].Count);
                    }
                    updatedScores[node] = score;
                }

                nodeScores = updatedScores;
            }

            return nodeScores;
        }
    }

    internal class Program
    {
        // List of common English stopwords
        public static readonly HashSet<string> StopWords = new HashSet<string>
        {
            "a", "about", "above", "after", "again", "against", "all", "also",
            "am", "an", "and", "any", "are", "aren't", "as", "at", "be",
            "because", "been", "before", "being", "below", "between", "both",
            "but", "by", "can", "can't", "cannot", "could", "couldn't", "did",
            "didn't", "do", "does", "doesn't", "doing", "don't", "down",
            "during", "each", "few", "for", "from", "further", "had", "hadn't",
            "has", "hasn't", "have", "haven't", "having", "he", "he'd", "he'll",
            "he's", "her", "here", "here's", "hers", "herself", "him",
            "himself", "his", "how", "how's", "i", "i'd", "i'll", "i'm", "i've",
            "if", "in", "into", "is", "isn't", "it", "it's", "its", "itself",
            "let's", "me", "more", "most", "mustn't", "my", "myself", "no",
            "nor", "not", "of", "off", "on", "once", "only", "or", "other",
            "ought", "our", "ours", "ourselves", "out", "over", "own", "said",
            "same", "shan't", "she", "she'd", "she'll", "she's", "should",
            "shouldn't", "so", "some", "such", "than", "that", "that's", "the",
            "their", "theirs", "them", "themselves", "then", "there", "there's",
            "these", "they", "they'd", "they'll", "they're", "they've", "this",
            "those", "through", "to", "too", "under", "until", "up", "very",
            "was", "wasn't", "we", "we'd", "we'll", "we're", "we've", "were",
            "weren't", "what", "what's", "when", "when's", "where", "where's",
            "which", "while", "who", "who's", "whom", "why", "why's", "will",
            "with", "won't", "would", "wouldn't", "you", "you'd", "you'll",
            "you're", "you've", "your", "yours", "yourself", "yourselves"
        };

        /// <summary>
        /// Determines the number of top keywords to extract based on the length of the text.
        /// </summary>
        /// <param name="wordCount">Total number of words in the text.</param>
        /// <returns>Number of top keywords to extract.</returns>
        static int TopNumberCalculator(int wordCount)
        {
            // For simplicity, extract top 20% of unique words, minimum of 5 and maximum of 20
            int calculatedNumber = Math.Max(5, Math.Min(20, wordCount / 5));
            return calculatedNumber;
        }

        /// <summary>
        /// Extracts keywords from the text using the TextRank algorithm.
        /// </summary>
        /// <param name="text">Input text.</param>
        /// <param name="dampingFactor">Damping factor for PageRank calculation.</param>
        /// <param name="windowSize">Window size for co-occurrence.</param>
        /// <param name="iterations">Number of iterations for PageRank calculation.</param>
        /// <returns>List of extracted keywords.</returns>
        static List<string> KeywordExtraction(string text, double dampingFactor, int windowSize, int iterations)
        {
            string[] words = GetKeywords(text);
            int topN = TopNumberCalculator(words.Length);

            Graph keywordGraph = CreateGraph(words, windowSize);
            Dictionary<string, double> wordScores = keywordGraph.CalculatePageRank(dampingFactor, iterations);

            List<string> sortedKeywords = wordScores.OrderByDescending(kvp => kvp.Value)
                                                    .Take(topN)
                                                    .Select(kvp => kvp.Key)
                                                    .ToList();

            return sortedKeywords;
        }

        /// <summary>
        /// Processes the text to extract words, removing stopwords and splitting using regex.
        /// </summary>
        /// <param name="text">Input text.</param>
        /// <returns>Array of keywords.</returns>
        static string[] GetKeywords(string text)
        {
            // Use regex to split text into words, handling various punctuation marks
            string[] words = Regex.Split(text.ToLower(), @"\W+")
                                  .Where(word => !string.IsNullOrWhiteSpace(word))
                                  .ToArray();

            // Filter out stopwords
            string[] keywords = words.Where(word => !StopWords.Contains(word)).ToArray();

            return keywords;
        }

        /// <summary>
        /// Creates a co-occurrence graph from the list of keywords.
        /// </summary>
        /// <param name="keywords">Array of keywords.</param>
        /// <param name="windowSize">Window size for co-occurrence.</param>
        /// <returns>Graph representing keyword co-occurrences.</returns>
        static Graph CreateGraph(string[] keywords, int windowSize)
        {
            Graph graph = new Graph();

            for (int i = 0; i < keywords.Length; i++)
            {
                for (int j = i + 1; j < i + windowSize && j < keywords.Length; j++)
                {
                    graph.AddEdge(keywords[i], keywords[j]);
                }
            }

            return graph;
        }

        static void Main(string[] args)
        {
            // Parameters for TextRank algorithm
            double dampingFactor = 0.85;
            int windowSize = 4;
            int iterations = 100;

            Console.WriteLine("Enter text for keyword extraction:");
            string text = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(text))
            {
                Console.WriteLine("No text provided. Exiting.");
                return;
            }

            var extractedKeywords = KeywordExtraction(text, dampingFactor, windowSize, iterations);

            Console.WriteLine("\nExtracted Keywords:");
            foreach (var keyword in extractedKeywords)
            {
                Console.WriteLine(keyword);
            }

            // Wait for user input before closing
            Console.WriteLine("\nPress any key to exit.");
            Console.ReadKey();
        }
    }
}


