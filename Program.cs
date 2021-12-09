using CsvHelper;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automata_Updated_Project
{
    class Program
    {
        static void Main(string[] args)
        {
            string mainUrl = "http://books.toscrape.com/catalogue/category/books/";
            Console.WriteLine("Choose Category");
            Console.WriteLine("Travel = 1 \nMystery = 2 \nHistorical Fiction = 3 \nSequential Art = 4 \nClassics = 5 ");
            int category=Convert.ToInt32(Console.ReadLine());
             switch (category)
            {
                case 1:
                    mainUrl = mainUrl + "travel_2/";
                    break;
                case 2:
                    mainUrl = mainUrl + "mystery_3/";
                    break;
                case 3:
                    mainUrl = mainUrl + "historical-fiction_4/";
                    break;
                case 4:
                    mainUrl = mainUrl + "sequential-art_5/";
                    break;
                case 5:
                    mainUrl = mainUrl + "classics_6/";
                    break;
                default:
                    Console.WriteLine("Invalid");
                    break;
            }
            var bookLinks = GetBookLinks(mainUrl);
            Console.WriteLine("Found {0} links", bookLinks.Count);
            //var books = GetBookDetails(bookLinks);
            var books = GetBookDetailsArray(bookLinks);

            //exportToCSV(books);
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
            string url = mainUrl + "index.html";
            var bookLinks = new List<string>();
            HtmlDocument doc = GetDocument(url);
            HtmlNodeCollection linkNodes = doc.DocumentNode.SelectNodes("//h3/a");
            var baseUri = new Uri(url);
            foreach (var link in linkNodes)
            {
                string href = link.Attributes["href"].Value;
                bookLinks.Add(new Uri(baseUri, href).AbsoluteUri);
            }
            bookLinks = GetPageLinks(bookLinks,mainUrl,url);

            return bookLinks;
        }
        static List<string> GetPageLinks(List<string> bookLinks, string mainUrl, string url)
        {
            string val;
            HtmlDocument doc = GetDocument(url);
            HtmlNodeCollection linkNodes = doc.DocumentNode.SelectNodes("//h3/a");
            string x = "//form/strong";
            int page = Convert.ToInt32(doc.DocumentNode.SelectSingleNode(x).InnerText);
            if (page >20 && bookLinks.Count!=page)
            {
                string href1 = doc.DocumentNode.SelectSingleNode("//ul[contains(@class,\"pager\")]/li[@class=\"next\"]/a").Attributes["href"].Value;
                val = mainUrl + href1;
                HtmlDocument doc1 = GetDocument(val);
                HtmlNodeCollection nodes = doc1.DocumentNode.SelectNodes("//h3/a");
                var baseUri1 = new Uri(val);
                foreach (var node in nodes)
                {
                    string href = node.Attributes["href"].Value;
                    bookLinks.Add(new Uri(baseUri1, href).AbsoluteUri);
                }
                GetPageLinks(bookLinks, mainUrl, val);
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
                int[,] intarray = new int[4, 2];
            }
            return books;
        }
        static Array GetBookDetailsArray(List<string> urls)
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
                    booksArray[i, j] = document.DocumentNode.SelectSingleNode(descriptionXPath).InnerText;
                    i++;
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
