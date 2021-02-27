using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;

namespace IsCrawler.Crawler
{
    public class Crawler
    {
        HtmlParser.HtmlParser HTMLParser;
        HttpClient HTTPClient;
        private const string FolderPath = @"C:\Users\vadim\1DOTNET\1st_task\db";
        private readonly int PagesCount = 100; //100
        private readonly int WordsCount = 1000;//1000

        public Crawler(int pagesCount = 100, int wordsCouny = 1000)
        {
            PagesCount = pagesCount;
            WordsCount = wordsCouny;
            CreateForder();
            HTMLParser = new HtmlParser.HtmlParser();
            HTTPClient = new HttpClient();
            HTTPClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.150 Safari/537.36");
        }

        public void GetCrawl(string link) 
        {
            var uri = new Uri(link);

            var links = new List<string>() { link };//links step now
            var indexedLinks = new List<IndexLink>();//indexed links
            var newLinks = new List<string>();//new links 
            var indexedBadLinks = new List<string>();//not found or bad links
            var queuedLinks = new List<string>();//links in queue

            while (indexedLinks.Count < PagesCount)
            {
                if (newLinks.Count > 0 || queuedLinks.Count > 0) 
                {
                    links.AddRange(newLinks.Distinct());
                    links.AddRange(queuedLinks);
                    links.RemoveAll(x => indexedLinks.Select(x => x.Link).Contains(x));
                    links.RemoveAll(x => indexedBadLinks.Contains(x));
                    queuedLinks = links.Skip(PagesCount - indexedLinks.Count).Distinct().ToList();
                    links = links.Take(PagesCount - indexedLinks.Count).ToList();
                    newLinks = new List<string>();
                }

                if(links.Count < 1) 
                {
                    Console.WriteLine("not enough links");
                    break;
                }

                foreach (var url in links)
                {
                    try 
                    {
                        var html = HTTPClient.GetStringAsync(url).Result;
                        newLinks.AddRange(HTMLParser.GetLinks(html, uri));
                        var text = HTMLParser.GetText(html);
                        var wordsCount = GetWordsCount(text);
                        if (wordsCount > WordsCount) 
                        {
                            indexedLinks.Add(new IndexLink 
                            {
                                Link = url,
                                WordsCount = wordsCount,
                                Text = text
                            });
                        }
                        else 
                        {
                            indexedBadLinks.Add(url);
                        }
                    }
                    catch (Exception ex)
                    {
                        indexedBadLinks.Add(url);
                        continue;
                    }

                }
                links = new List<string>();
            }

            SaveToFile(indexedLinks);

        }

        private void SaveToFile(List<IndexLink> indexedLinks) 
        {

            var uri = new Uri(indexedLinks[0].Link);

            var pathToDir = $"{FolderPath}/{uri.Host}";

            if (!Directory.Exists(pathToDir)) 
            {//Create forder for link
                Directory.CreateDirectory(pathToDir);
            }

            for (int i = 0; i < indexedLinks.Count; i++)
            {//save text in file
                var fileName = $"{pathToDir}/{i}.txt";
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
                using (FileStream fs = File.Create(fileName))
                {
                    Byte[] title = new UTF8Encoding(true).GetBytes(indexedLinks[i].Text);
                    fs.Write(title, 0, title.Length);
                }
                indexedLinks[i].FilePath = fileName;
                indexedLinks[i].DocNum = i;

            }

            var indexFile = $"{pathToDir}/index.txt";
            if (File.Exists(indexFile))
            {
                File.Delete(indexFile);
            }
            using (FileStream fs = File.Create(indexFile))
            {
                string text = string.Join('\n', indexedLinks.Select(x => $"{x.DocNum} {x.WordsCount} {x.FilePath.ToString()} {x.Link}"));
                Byte[] title = new UTF8Encoding(true).GetBytes(text);
                fs.Write(title, 0, title.Length);
            }

        }

        private void CreateForder() 
        {
            if (!Directory.Exists(FolderPath))
            {//Create forder for link
                Directory.CreateDirectory(FolderPath);
            }
        }

        private int GetWordsCount(string str) 
        {
            MatchCollection collection = Regex.Matches(str, @"[\S]{3,}");
            return collection.Count;
        }

        private class IndexLink
        {
            public string Link { get; set; }

            public string Text { get; set; }

            public int WordsCount { get; set; }

            public string FilePath { get; set; }

            public int DocNum { get; set; }

        }
    }
}
