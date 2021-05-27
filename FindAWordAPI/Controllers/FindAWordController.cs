using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

// https://sourceforge.net/projects/scrabbledict/
namespace FindAWordAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FindAWordController : ControllerBase
    {
        private readonly ILogger<FindAWordController> _logger;

        public FindAWordController(ILogger<FindAWordController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<string> Get(string letters, string goal)
        {
            letters = letters.ToLower();
            goal = goal.ToLower();

            var words = new List<string>();
            //string[] resourceNames = this.GetType().Assembly.GetManifestResourceNames();
            //foreach (string resourceName in resourceNames)
            //{
            //    possibilities.Add(resourceName);
            //}
            using (var stream = GetType().Assembly.GetManifestResourceStream("FindAWordAPI.Resources.dictionary.txt"))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                var contents = reader.ReadToEndAsync().Result;
                words.AddRange(contents.Split('\n'));
            }

            var possibilities = new List<string>();

            foreach (var word in words)
            {
                if (word.Length != goal.Length)
                    continue;
                var possible = true;
                var lettersLeft = letters;
                for (var index = 0; index < goal.Length && possible; index++)
                {
                    var c = goal[index];
                    if (c != '?' && word[index] != c)
                        possible = false;
                    else
                    {
                        var i = lettersLeft.IndexOf(word[index]);
                        if (i < 0)
                            possible = false;
                        else
                        {
                            var newLetters = new StringBuilder(lettersLeft) { [i] = '~' };
                            lettersLeft = newLetters.ToString();
                        }
                    }
                }

                if (possible)
                    possibilities.Add(word);
            }

            return possibilities;
        }
    }
}