using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

namespace TextRank_Algorithm
{
    public class Graph
    {
        private Dictionary<string, List<string>> adjList;
        public Graph()
        {
            adjList = new Dictionary<string, List<string>>();
        }
        public void addEdge(string kw1, string kw2)
        {
            if (!adjList.ContainsKey(kw1)) adjList[kw1] = new List<string>();
            if (!adjList.ContainsKey(kw2)) adjList[kw2] = new List<string>();
            if (!adjList[kw1].Contains(kw2)) adjList[kw1].Add(kw2);
            if (!adjList[kw2].Contains(kw1)) adjList[kw2].Add(kw1);
        }
        public Dictionary<string, List<string>> getAdjList()
        {
            return adjList;
        }
        public Dictionary<string, double> calcPageRank(double dampingFactor, int iterations)
        {
            Dictionary<string, List<string>> adjList = getAdjList();
            Dictionary<string, double> nodeScores = new Dictionary<string, double>();
            foreach (string node  in adjList.Keys)
            {
                nodeScores[node] = 1;
            }

            for (int loops = 0; loops < iterations; loops++)
            {
                Dictionary<string, double> updatedScores = new Dictionary<string, double>();
                foreach (string node in adjList.Keys)
                {
                    double score = (1 - dampingFactor);
                    foreach (string n in adjList[node])
                    {
                        score += (dampingFactor * nodeScores[n] / adjList[n].Count); // update each nodes score based on the PageRank formula of each neighbours score
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
        static int topNumberCalculator(int length)
        {
            throw new NotImplementedException();
        }
        
        public static string[] stopWords = { "a", "about", "above", "after", "again", "against", "all", "also", "am", "an", "and", "any", "are", "arent", "as", "at", "be", "because", "been", "before", "being", "below", "between", "both", "but", "by", "can", "cant", "cannot", "could", "couldnt", "did", "didnt", "do", "does", "doesnt", "doing", "dont", "down", "during", "each", "few", "for", "from", "further", "had", "hadnt", "has", "hasnt", "have", "havent", "having", "he", "hed", "hell", "hes", "her", "here", "heres", "hers", "herself", "him", "himself", "his", "how", "hows", "i", "id", "ill", "im", "ive", "if", "in", "into", "is", "isnt", "it", "its", "its", "itself", "lets", "me", "more", "most", "mustnt", "my", "myself", "no", "nor", "not", "of", "off", "on", "once", "only", "or", "other", "ought", "our", "ours", "ourselves", "out", "over", "own", "said", "same", "shant", "she", "shed", "shell", "shes", "should", "shouldnt", "so", "some", "such", "than", "that", "thats", "the", "their", "theirs", "them", "themselves", "then", "there", "theres", "these", "they", "theyd", "theyll", "theyre", "theyve", "this", "those", "through", "to", "too", "under", "until", "up", "very", "was", "wasnt", "we", "wed", "well", "were", "weve", "were", "werent", "what", "whats", "when", "whens", "where", "wheres", "which", "while", "who", "whos", "whom", "why", "whys", "will", "with", "wont", "would", "wouldnt", "you", "youd", "youll", "youre", "youve", "your", "yours", "yourself", "yourselves" };
        static List<string> keywordExtraction(string text)
        {
            double dampingFactor = 0.85;
            int windowSize = 4;
            int iterations = 100;
            int topN = topNumberCalculator(text.Length); 

            string[] keywords = stopwordFilter(text); // split into single words and remove stop words
            Graph keywordGraph = createGraph(keywords, windowSize); // create a graph containing each word connected to neighbour words
            Dictionary<string, double> wordValues = keywordGraph.calcPageRank(dampingFactor, iterations); // calculate the score for each word based on its connection to neighbours

            List<KeyValuePair<string, double>> sortedWords = wordValues.OrderByDescending(score => score.Value).ToList(); //Order words in dict by wordValues in descending order (LINQ)

            List<string> finalKeywords = new List<string>();

            for (int i = 0; i < Math.Min(topN, sortedWords.Count()); i++) // extracts the topN words
            {
                finalKeywords.Add(sortedWords[i].Key);
            }
            return finalKeywords; 
        }

        static string[] stopwordFilter(string text)
        {
            string[] splitWords = text.ToLower().Split(new char[] {' ', ',', '.', '?', '!', ':', ';', '/'}, StringSplitOptions.RemoveEmptyEntries);
            List<string> keywords = new List<string>();
            foreach (string i in splitWords)
            {
                if (!stopWords.Contains(i))
                {
                    keywords.Add(i);
                }
                else
                {
                    continue;
                }
            }

            Console.WriteLine("With stopwords removed: "); //testing
            foreach (string keyword in keywords)
            {
                Console.WriteLine(keyword);
            }
            Console.WriteLine();

            return keywords.ToArray();
        }
        static Graph createGraph(string[] keywords, int windowSize)
        {
            Graph graph = new Graph();
            for (int i = 0; i < keywords.Length; i++)
            {
                for (int j = i + 1; j < i + windowSize && j < keywords.Length; j++)
                {
                    graph.addEdge(keywords[i], keywords[j]);
                }
            }
            return graph;
        }
        static void Main(string[] args)
        {
            string text = "Encryption is the process of using an algorithm to convert plaintext into ciphertext, a form that cannot be easily understood by a human or machine without knowing how to decrypt it.";
            //string text = "This is a sample document, and this document is a sample text for keyword extraction using TextRank.";
            var results = keywordExtraction(text);
            Console.WriteLine("After full RAKE process:");
            foreach (var result in results)
            {
                Console.WriteLine(result);
            }
            Console.ReadKey();
        }
    }
}
