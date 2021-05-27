using System;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace FindAWord
{
    class Program
    {
        static void Main(string[] args)
        {
            var letters = "spryuy";
            var goal = "????";
            var words = File.ReadAllLines(@"C:\Users\dadov\git\english-words\words.txt");
            foreach (var word in words)
            {
                if (word.Length != goal.Length)
                    continue;
                var possible = true;
                var lettersLeft = letters;
                for (var index = 0; index < goal.Length && possible; index ++)
                {
                    var c = goal[index];
                    if ((c != '?') && (word[index] != c))
                        possible = false;
                    else
                    {
                        var i = lettersLeft.IndexOf(word[index]);
                        if (i < 0)
                            possible = false;
                        else
                        {
                            var newLetters = new StringBuilder(lettersLeft);
                            newLetters[i] = '~';
                            lettersLeft = newLetters.ToString();
                        }
                    }
                }
                if (possible)
                    Console.WriteLine(word);
            }
        }
    }
}
