using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GraphTheory
{
    class Node
    {
        private string wordOne, wordTwo;
        private int frequency = 0;

        public Node()
        {
            wordOne = "";
            wordTwo = "";
            frequency = 0;
        }
        public Node(string n)
        {
            wordOne = n;
            wordTwo = n;
            frequency = 1;
        }
        public Node(string n, int i)
        {
            wordOne = n;
            wordTwo = n;
            frequency = i;
        }
        public Node(string nOne, string nTwo, int freq = 1)
        {
            wordOne = nOne;
            wordTwo = nTwo;
            frequency = freq;
        } 

        public Node(Node previous)
        {
            wordOne = previous.wordOne;
            wordTwo = previous.wordTwo;
            frequency = previous.frequency;
        }

        ~Node()
        {
            Console.WriteLine("Removing node: {0} -> {1}", wordOne, wordTwo);
        }

        public bool incFrequency()
        {
            try
            {
                frequency++;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string getWord()
        {
            try
            {
                return wordOne;
            }
            catch
            {
                return "";
            }
        }

        public string GetSecond()
        {
            return wordTwo;
        }

        public int getFrequency()
        {
            try
            {
                return frequency;
            }
            catch
            {
                return 0;
            }
        }

        public override String ToString()
        {
            return wordTwo;
        }
    }

    class Graph
    {
        private int max;
        private Dictionary<string, int> indices = new Dictionary<string, int>();
        private string longestWord = "";

        private Node[,] adjMat;

        public Graph()
        {
            adjMat = new Node[0, 0];
            max = 0;
        }

        public bool AddWord(string s)
        {
            try
            {
                if (indices.ContainsKey(s) || s.Trim().Length == 0)
                {
                    // Do Nothing
                }
                else
                {
                    indices.Add(s, max);
                    DoubleMat();
                    max++;
                    if(s.Length > longestWord.Length)
                    {
                        longestWord = s;
                    }
                }
                return true;
            }
            catch
            {
                Console.WriteLine("Error adding Node {0}", s);
                return false;
            }
        }

        public bool AddNode(string one, string two)
        {
            if (one.Trim().Length == 0 || two.Trim().Length == 0)
                return false;

            int indO = indices[one];
            int indT = indices[two];

            try
            {
                if (adjMat[indO, indT] == null)
                {
                    adjMat[indO, indT] = new Node(one, two);
                }
                else
                {
                    adjMat[indO, indT].incFrequency();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Error adding node: \n{0}", e);
            }
            

            return true;
        }

        //The size of the matrix will ALWAYS be a power of 2
        private bool DoubleMat()
        {
            double logResult = 1;
            if (max != 0) 
                logResult = Math.Log2((double)max);
            
            if ((int)logResult == logResult)
            {
                
                if (adjMat.GetLength(0) == 0)
                {
                    adjMat = new Node[1, 1];
                }
                else
                {
                    Node[,] newMat = new Node[max * 2, max * 2];
                    for(int i = 0; i < adjMat.GetLength(0); i++)
                        Array.Copy(adjMat, i* adjMat.GetLength(1), newMat, i* newMat.GetLength(1), adjMat.GetLength(1));

                    this.adjMat = newMat;
                }
            }

            return true;
        }

        public bool ReadCSV(string folder)
        {
            DirectoryInfo dir = new DirectoryInfo(folder);
            FileInfo[] files = dir.GetFiles("*.txt");
            foreach( FileInfo fi in files)
            {
                string prev = "";
                Console.WriteLine(fi);

                string[] lines = System.IO.File.ReadAllLines(fi.ToString());
                foreach(string l in lines)
                {
                    string[] words = l.Split(new Char[] { ' ', ',', '!', '?', ';', ':', '.', ':', '\t', '\n', '{', '}', '/', '"', '(', ')' });
                    foreach(string w in words)
                    {
                        string word = w.ToLower();
                        AddWord(word);
                        if (prev != "")
                            AddNode(prev, word);
                        prev = word;
                    }
                }
            }
            
            return true;
        }

        public string ProduceWord(int l, string beginning = "the")
        {
            string current = beginning.ToLower();
            string str = beginning.ToLower();

            Random emergency = new Random();

            int iters = 0;
            while(iters < l)
            {
                iters++;

                if (!indices.ContainsKey(beginning))
                {
                    return string.Format("\"{0}\" is not in the passage.", current);
                }

                Console.WriteLine(current);
                int row = indices[current];
                var probable = (p: 0, s: "");
                for(int i = 0; i < max; i++)
                {
                    int emRow = emergency.Next(max-1);
                    if (adjMat[emRow, i] != null && probable.p == 0)
                    {
                        probable.s = adjMat[emRow, i].GetSecond();
                        Console.WriteLine("The probable.s is {0}, length: {1}", probable.s, probable.s.Length);
                    }
                    if(adjMat[row, i] != null && adjMat[row, i].getFrequency() > probable.p && !str.Contains(adjMat[row, i].GetSecond()))
                    {
                        probable = (p: adjMat[row, i].getFrequency(), s: adjMat[row, i].GetSecond());
                        Console.WriteLine("This one SHOULD be setting. p: {0}, s: {1}", probable.p, probable.s);
                    }
                }

                str += " " + probable.s;
                current = probable.s;
                Console.WriteLine("Current is set to {0}", current);
            }

            return str;
        }

        public override String ToString()
        {
            string str = "The Words and indices are:\n";
            string[,] prettyMat = new string[indices.Count + 1, indices.Count + 1];

            foreach(KeyValuePair<string, int> entry in indices)
            {
                str += entry.Key + ": " + entry.Value + " | ";
                prettyMat[0, entry.Value] = entry.Key;
                prettyMat[entry.Value, 0] = entry.Key;
            }
            str += "\n\n\nMATRIX\n";


            List<string> ks = indices.Keys.ToList<string>();

            for (int k = 0; k < longestWord.Length; k++)
                str += " ";
            foreach (string s in ks)
            {
                int k = 0;
                for (k = 0; k < (int)((longestWord.Length - s.Length) / 2); k++)
                    str += " ";
                str += (s);
                for (k = k; k < longestWord.Length - s.Length; k++)
                    str += " ";
            }
            str += "\n";

            for (int i = 0; i < max; i++)
            {
                int k;
                for (k = 0; k < (int)((longestWord.Length - ks[i].Length) / 2); k++)
                    str += " ";
                str += (ks[i]);
                for (k = k; k < longestWord.Length - ks[i].Length; k++)
                    str += " ";

                for (int j = 0; j < max; j++)
                {
                    if (adjMat[i, j] == null)
                        for (k = 0; k < longestWord.Length; k++)
                            str += " ";
                    else
                    {
                        string fr = (adjMat[i, j].getFrequency()).ToString();
                        for (k = 0; k < (int)((longestWord.Length - (fr.ToString()).Length)/2); k++)
                            str += " ";
                        str += (fr);
                        for (k = k; k < longestWord.Length - (fr.ToString()).Length; k++)
                            str += " ";
                    }
                        
                }
                str += (Environment.NewLine);
            }

            return str;
        }

        static void Main(string[] args)
        {
            Graph g = new Graph();

            Console.WriteLine("Enter the path to the folder where your text files are stored");
            string pathWay = Console.ReadLine();

            Console.WriteLine("Enter the path to the folder where you want the output to go");
            string outputFolder = Console.ReadLine();

            Console.WriteLine("What word do you want to start your sentence with?");
            string startWord = Console.ReadLine();

            Console.WriteLine("What length do you want the sentence to be?");
            int num = Int32.Parse(Console.ReadLine());

            g.ReadCSV(pathWay);

            string filename = string.Format("{1}/{0:yyyy-MM-dd_hh-mm-ss-tt}.txt", DateTime.Now, outputFolder);
            System.IO.File.WriteAllText(@"" + filename, g.ProduceWord(num, startWord));
            //System.IO.File.WriteAllText(@"C:\Users\Conno\OneDrive\Professional\SampleTxts\OutputFolder\" + filename, g.ProduceWord(53, "old"));
        }
    }
}
