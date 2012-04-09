﻿using System;
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
                anagramList = new List<List<string>> { new List<String> {args.Argument as String} };
            };

            _worker.RunWorkerCompleted += (sender, args) =>
            {
                var anagrams = anagramList.Aggregate(String.Format("Word Count: {0}", wordCount), (s, list) => 
                    s + Environment.NewLine + list.Aggregate((s1, s2) => s1 + " " + s2));
                anagrammarResults.Text = anagrams;
            };
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
                var wordQuery = from elements in parent.Elements("st") select elements;
                return wordQuery.Count();
            }
            return 0;
        }
    }
}