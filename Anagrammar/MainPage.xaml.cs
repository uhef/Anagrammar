using System;
using System.Linq;
using System.Windows.Navigation;
using System.Xml.Linq;
using Microsoft.Phone.Controls;

namespace Anagrammar
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            sourceWord.GotFocus += (sender, args) => { sourceWord.Text = ""; };

            searchButton.Click += (sender, args) => CountWords();
        }

        private void CountWords()
        {
            var xml = XDocument.Load("kotus-sanalista_v1.xml");

            anagrammarResults.Text += String.Format(" Word Count: {0}", WordCount(xml));
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