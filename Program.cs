using CsvHelper;
using HtmlAgilityPack;
using StopWord;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

namespace Automata_Updated_Project
{
    class Program
    {
        static void Main()
        {            
            //var s = StopWords.GetStopWords("en");
            string mainUrl = "https://books.toscrape.com/index.html";
            string categoryUrl=GetCategory(mainUrl);
            if (categoryUrl=="")
                Console.WriteLine("Invalid category");
            else
            {
                var bookLinks = GetBookLinks(categoryUrl);
                if (bookLinks==null)
                {
                    Console.WriteLine("Found {0} Books", bookLinks.Count);
                }
                else
                {
                    Console.WriteLine("Found {0} Books", bookLinks.Count);
                    GetBookDetailsArray(bookLinks);
                }

            }
        }
        //To get category
        static string GetCategory(string url)
        {
            try
            {
                dynamic d1 = new System.Dynamic.ExpandoObject();
                var categoryLinks = new Dictionary<int, dynamic>();
                HtmlDocument doc = GetDocument(url);
                HtmlNodeCollection linkNodes = doc.DocumentNode.SelectNodes("//li/ul/li/a");
                var baseUri = new Uri(url);
                int key = 0;
                foreach (var link in linkNodes)
                {
                    categoryLinks[key] = d1;
                    string href = link.Attributes["href"].Value;
                    string val = link.InnerText.Trim();
                    var a_link = new Uri(new Uri(baseUri, href).AbsoluteUri);
                    string text = a_link.Segments.GetValue(4).ToString();
                    string[] textSplit = text.Split('_');
                    categoryLinks[key] = new { s1 = textSplit[0], s2 = val, s3 = a_link.ToString() };
                    key++;
                }
                Console.WriteLine("Here are the categories");
                foreach (KeyValuePair<int, dynamic> category in categoryLinks)
                    Console.WriteLine("{0}", category.Value.s2);
                Console.WriteLine("Enter Category Name");
                string selectedCategory = Console.ReadLine().ToLower().Replace(" ", "-");
                string categoryUrl = string.Empty;
                foreach (KeyValuePair<int, dynamic> category in categoryLinks)
                {
                    if (selectedCategory == category.Value.s1)
                    {
                        categoryUrl = category.Value.s3;
                        return categoryUrl;
                    }
                }
                return categoryUrl;
            }
            catch (Exception)
            {

                throw;
            }
        }

        // Parses the URL and returns HtmlDocument object
        static HtmlDocument GetDocument(string url)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);
            return doc;
        }
        //public HtmlNodeCollection SelectNodes(string xpath) { }
        //public HtmlNode SelectSingleNode(string xpath) { }
        static List<string> GetBookLinks(string mainUrl)
        {
            var uri = new Uri(mainUrl);
            var noLastSegment = string.Format("{0}://{1}", uri.Scheme, uri.Authority);
            for (int j = 0; j < uri.Segments.Length - 1; j++)
            {
                noLastSegment += uri.Segments[j];
            }
            var bookLinks = new List<string>();
            HtmlDocument doc = GetDocument(mainUrl);
            HtmlNodeCollection linkNodes = doc.DocumentNode.SelectNodes("//h3/a");
            var baseUri = new Uri(mainUrl);

            foreach (var link in linkNodes)
            {
                string href = link.Attributes["href"].Value;
                bookLinks.Add(new Uri(baseUri, href).AbsoluteUri);
            }
            bookLinks = GetPageLinks(bookLinks, mainUrl, noLastSegment);

            return bookLinks;
        }
        //To get next page link
        static List<string> GetPageLinks(List<string> bookLinks, string mainUrl, string url)
        {
            string val;
            HtmlDocument doc = GetDocument(mainUrl);
            string x = "//form/strong";
            int page = Convert.ToInt32(doc.DocumentNode.SelectSingleNode(x).InnerText);

            if (page > 20 && bookLinks.Count != page)
            {
                string href1 = doc.DocumentNode.SelectSingleNode("//ul[contains(@class,\"pager\")]/li[@class=\"next\"]/a").Attributes["href"].Value;
                val = url + href1;
                HtmlDocument doc1 = GetDocument(val);
                HtmlNodeCollection nodes = doc1.DocumentNode.SelectNodes("//h3/a");
                var baseUri1 = new Uri(val);
                foreach (var node in nodes)
                {
                    string href = node.Attributes["href"].Value;
                    bookLinks.Add(new Uri(baseUri1, href).AbsoluteUri);
                }
                GetPageLinks(bookLinks, val, url);
            }
            return bookLinks;
        }
       //To get book details
        static List<Book> GetBookDetails(List<string> urls)
        {
            var books = new List<Book>();
            foreach (var url in urls)
            {
                HtmlDocument document = GetDocument(url);
                var titleXPath = "//h1";
                var descriptionXPath = "//article[contains(@class,\"product_page\")]/p";
                var book = new Book
                {
                    Word = document.DocumentNode.SelectSingleNode(titleXPath).InnerText,
                    Sentence = document.DocumentNode.SelectSingleNode(descriptionXPath).InnerText
                };
                books.Add(book);
            }
            return books;
        }
        //2D array of books
        static void GetBookDetailsArray(List<string> urls)
        {
            string[,] booksArray = new string[urls.Count, 2];

            int i = 0;
            foreach (var url in urls)
            {
                HtmlDocument document = GetDocument(url);
                var titleXPath = "//h1";
                var descriptionXPath = "//article[contains(@class,\"product_page\")]/p";

                int j;
                for (j = 0; j < 1; j++)
                {
                    booksArray[i, j] = document.DocumentNode.SelectSingleNode(titleXPath).InnerText.ToLower().Replace("&39;", "'").Replace("&#39;", "'");
                }
                if (document.DocumentNode.SelectSingleNode(descriptionXPath) == null)
                {
                    booksArray[i, j] = "No description";
                }
                else
                {
                    booksArray[i, j] = document.DocumentNode.SelectSingleNode(descriptionXPath).InnerText.ToLower();
                    i++;
                }
            }
            PrintArray(booksArray);
            ApplyRegex(booksArray);
        }
        //Applying regex on selected book
        static void ApplyRegex(string[,] books)
        {
            int temp = 0;
            var booksRegex = new List<Book>();
            MatchCollection matchedDesc;
            Console.WriteLine("Enter Book Title");
            string bookName = Console.ReadLine().ToLower().Replace("()", " ");
            for (int i = 0; i<books.GetLength(0); i++)
            {
               if (books[i, 0] == bookName)
                  {
                    temp = 1;
                      var newString = bookName.RemoveStopWords("en");
                      string rr = Regex.Replace(newString, @"[():,#0-9]", "").Trim();
                      string duplicatesRemoved = string.Join(" ", rr.Split(' ').Distinct());
                      string[] titleSplit = duplicatesRemoved.Split(' ');
                      string pattern;
                      string desc = books[i, 1];
                        for (int j = 0; j<titleSplit.Length; j++)
                        {
                            pattern = @"[^.]*" + titleSplit[j] + "[^.]*\\.";
                            matchedDesc = Regex.Matches(desc, pattern, RegexOptions.IgnoreCase);
                            for (int count = 0; count<matchedDesc.Count; count++)
                            {
                                var book = new Book
                                {
                                    Word = titleSplit[j],
                                    Sentence = matchedDesc[count].Value
                                };
                            booksRegex.Add(book);
                            }

                        }
               }

            }
            if (temp==0)
                Console.WriteLine("Invalid Book Name");
            else
            {
                if (booksRegex.Count==0)
                    Console.WriteLine("Title not found in description");
                else
                ExportToCSV(booksRegex);
            }

        }
        static void ExportToCSV(List<Book> books)
        {
            using (var writer = new StreamWriter("./regx.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(books);
                Console.WriteLine("Values saved in CSV file named: regx.csv ");
            }
        }
        //Print array
        static void PrintArray(string[,] books)
        {
            for (int i = 0; i<books.GetLength(0); i++)
            {
                int j = 0;
                for ( j = 0; j<1; j++)
                {
                    Console.WriteLine("Title : " + books[i, j]);
                }
                Console.WriteLine("Description : " + books[i, j]);

            }
        }

    }
}