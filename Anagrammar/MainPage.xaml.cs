using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Phone.Controls;

namespace Anagrammar
{
    public partial class MainPage : PhoneApplicationPage
    {
        private readonly BackgroundWorker _worker = new BackgroundWorker();

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            sourceWord.GotFocus += (sender, args) => { sourceWord.Text = ""; };

            searchButton.Click += (sender, args) => CountWords();

            var wordCount = 0;
            var anagramList = new List<List<String>>();
            _worker.DoWork += (sender, args) =>
            {
                var xml = XDocument.Load("kotus-sanalista_v1.xml");
                wordCount = WordCount(xml);
                anagramList = AnagramList(xml, args.Argument as String);
            };

            _worker.RunWorkerCompleted += (sender, args) =>
            {
                var anagrams = anagramList.Aggregate(String.Format("Word Count: {0}", wordCount), (s, list) => 
                    s + Environment.NewLine + list.Aggregate((s1, s2) => s1 + " " + s2));
                anagrammarResults.Text = anagrams;
            };
        }

        private static List<List<string>> AnagramList(XContainer xml, String sourceWord)
        {
            var analyzedWord = CharacterCount.AnalyzeWord(sourceWord);
            var sourceWordAnalysis = analyzedWord.Aggregate("", (c, grouping) => c + " " + grouping.Key + ": " + grouping.Count);

            var parent = xml.Element("kotus-sanalista");
            if (parent != null)
            {
                var words = from elements in parent.Elements("st").Elements("s")
                            select elements;
                var possibleCandidates = words
                    .Where(element => analyzedWord.CanConstruct(CharacterCount.AnalyzeWord(element.Value)))
                    .Take(10)
                    .Select(element => new List<string> {element.Value});
                return new List<List<string>> { new List<string> {sourceWordAnalysis} }.Concat(possibleCandidates).ToList();
            }
            return new List<List<string>>();
        }

        private void CountWords()
        {
            if (!_worker.IsBusy)
            {
                anagrammarResults.Text = String.Format("Counting words...");
                _worker.RunWorkerAsync(sourceWord.Text);
            }
        }

        private static int WordCount(XContainer xml)
        {
            var parent = xml.Element("kotus-sanalista");
            if (parent != null)
            {
                var wordQuery = from elements in parent.Elements("st").Elements("s") select elements;
                return wordQuery.Count();
            }
            return 0;
        }
    }

    public class CharacterCount
    {
        public char Key { get; private set; }
        public int Count { get; private set; }

        public static IEnumerable<CharacterCount> AnalyzeWord(String word)
        {
            return word.Where(c => !Char.IsWhiteSpace(c))
                .OrderBy(Char.ToLower)
                .GroupBy(c => c)
                .Select(grouping => new CharacterCount { Key = grouping.Key, Count = grouping.Count() });
        }

        public bool CanConstruct(CharacterCount characterCount)
        {
            return Key == characterCount.Key && Count >= characterCount.Count;
        }
    }

    static class EnumerableExtensions
    {
        public static bool CanConstruct(this IEnumerable<CharacterCount> source, IEnumerable<CharacterCount> target)
        {
            return target.All(characterCount => source.Any(c => c.CanConstruct(characterCount)));
        }
    }
}