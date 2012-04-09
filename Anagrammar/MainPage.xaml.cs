using System;
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
            _worker.DoWork += (sender, args) =>
            {
                var xml = XDocument.Load("kotus-sanalista_v1.xml");
                wordCount = WordCount(xml);
            };

            _worker.RunWorkerCompleted += (sender, args) =>
            {
                anagrammarResults.Text = String.Format("Word Count: {0}", wordCount);
            };
        }

        private void CountWords()
        {
            if (!_worker.IsBusy)
            {
                anagrammarResults.Text = String.Format("Counting words...");
                _worker.RunWorkerAsync();                
            }
        }

        private static int WordCount(XContainer xml)
        {
            var parent = xml.Element("kotus-sanalista");
            if (parent != null)
            {
                var wordQuery = from elements in parent.Elements("st") select elements;
                return wordQuery.Count();
            }
            return 0;
        }
    }
}