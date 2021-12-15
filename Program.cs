using CsvHelper;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Automata_Updated_Project
{
    class Program
    {
        static void Main(string[] args)
        {
            dynamic d1 = new System.Dynamic.ExpandoObject();
            var categoryLinks = new Dictionary<int, dynamic>();
            string mainUrl = "https://books.toscrape.com/index.html";
            HtmlDocument doc = GetDocument(mainUrl);
            HtmlNodeCollection linkNodes = doc.DocumentNode.SelectNodes("//li/ul/li/a");
            var baseUri = new Uri(mainUrl);
             int key=0;
            foreach (var link in linkNodes)
            {
                categoryLinks[key] = d1;
                string href = link.Attributes["href"].Value;
                string val = link.InnerText.Trim();
                var a_link = new Uri(new Uri(baseUri, href).AbsoluteUri);
                string text = a_link.Segments.GetValue(4).ToString();
                string[] textSplit = text.Split('_');
                categoryLinks[key] = new {s1=textSplit[0], s2 = val, s3 = a_link.ToString() };
                key++;
            }
            {
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
                        break;
                    }

                    
                }

                var bookLinks = GetBookLinks(categoryUrl);
                Console.WriteLine("Found {0} links", bookLinks.Count);
                ////var books = GetBookDetails(bookLinks);
                var books = GetBookDetailsArray(bookLinks);
                for (int i = 0; i < books.GetLength(0); i++)
                {
                    for (int j = 0; j < books.GetLength(1); j++)
                    {
                        Console.WriteLine("Books[{0},{1}] : " + books[i, j], i, j);
                    }
                }
                //exportToCSV(books);
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

            string href =string.Empty;
            foreach (var link in linkNodes)
            {
                 href = link.Attributes["href"].Value;
                bookLinks.Add(new Uri(baseUri, href).AbsoluteUri);
            }
            bookLinks = GetPageLinks(bookLinks,mainUrl,noLastSegment);

            return bookLinks;
        }
        static List<string> GetPageLinks(List<string> bookLinks, string mainUrl,string url)
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
                GetPageLinks(bookLinks, val,null);
            }
            return bookLinks;
        }

        static List<Book> GetBookDetails(List<string> urls)
        {
            var books = new List<Book>();
            foreach (var url in urls)
            {
                HtmlDocument document = GetDocument(url);
                var titleXPath = "//h1";
                var descriptionXPath = "//article[contains(@class,\"product_page\")]/p";
                var book = new Book();
                book.Title = document.DocumentNode.SelectSingleNode(titleXPath).InnerText;
                book.Description = document.DocumentNode.SelectSingleNode(descriptionXPath).InnerText;
                books.Add(book);
            }
            return books;
        }
         static string[,] GetBookDetailsArray(List<string> urls)
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
                        booksArray[i, j] = document.DocumentNode.SelectSingleNode(titleXPath).InnerText;
                    }
                if (descriptionXPath == null)
                {
                    booksArray[i, j] = "No description";

                }
                else
                {
                    booksArray[i, j] = document.DocumentNode.SelectSingleNode(descriptionXPath).InnerText;
                    i++;
                }
            }
            
            return booksArray;
        }
        static void exportToCSV(List<Book> books)
        {
            using (var writer = new StreamWriter("./books.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(books);
            }
        }

        
    }

}
