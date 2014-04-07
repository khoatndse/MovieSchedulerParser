using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using mshtml;
using System.Threading;
using HtmlAgilityPack;
using System.Collections;
using System.Text.RegularExpressions;

namespace MovieSchedulerParser
{
    class Galaxy
    {
        private WebBrowser ClientBrowser;
        private const string PageURL = @"https://www.galaxycine.vn/vi/phim/nowshowing";
        private Logger log = Logger.GetInstance();
        private bool finishedLoading;

        

        public Dictionary<string, Movie> Parsing(Dictionary<string, Movie> movieList)
        {
            // Connect to lotte website
            HtmlWeb website = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc = website.Load(PageURL);
            log.WriteLog("Connect to " + PageURL);

            // Container div
            HtmlAgilityPack.HtmlNodeCollection listNode = doc.DocumentNode.SelectNodes("//div[@class='movie-item']");
            for (int i = 0; i < listNode.Count; i++)
            {
                // parsing single movie
                Movie mv = ParseMovie(listNode[i]);
                // testing only
                log.WriteLog(mv.ToString());
                movieList.Add(mv.Name, mv);
            }

            // Complete parsing
            log.WriteLog("Finish parsing " + movieList.Count + " movies.");
            return movieList;
        }

        private Movie ParseMovie(HtmlNode movieNode)
        {

            string xpName = "input[2]";
            string xpFixName = "//div[@class='fr movie-detail']/div[1]";
            string xpType = "//div[@class='fr movie-detail']/div[8]/span[2]";
            string xpLength = "//div[@class='fr movie-detail']/div[11]/span[2]";
            string xpProducer = "//div[@class='fr movie-detail']/div[10]/span[2]";
            string xpImage = "div[@class='img-movie-item']//div[@class='rel']/a[1]/img";
            string xpDetail = "div[@class='img-movie-item']//div[@class='rel']/a[1]";
            string xpInfo = "//div[@class='fr movie-detail']/div[18]";

            Movie mv = new Movie();
            try
            {
                log.WriteLog("Begin Parse A Movie");
                mv.MvSource = this.GetType().Name;
                mv.Name = movieNode.SelectSingleNode(xpName).Attributes["value"].Value.Trim();
                mv.TrailerLink = movieNode.SelectSingleNode("input[3]").Attributes["value"].Value.Trim().Replace("/v/","/embed/");
                mv.Image = @"https://www.galaxycine.vn" + movieNode.SelectSingleNode(xpImage).Attributes["src"].Value;
                mv.DetailLink = @"https://www.galaxycine.vn" + movieNode.SelectSingleNode(xpDetail).Attributes["href"].Value;

                // Parse detail of a movie
                log.WriteLog("Parse movie detail at " + mv.DetailLink);

                HtmlWeb website = new HtmlWeb();
                HtmlAgilityPack.HtmlDocument doc = website.Load(mv.DetailLink);
                mv.Name = doc.DocumentNode.SelectSingleNode(xpFixName).InnerText.Trim();
                mv.Type = doc.DocumentNode.SelectSingleNode(xpType).InnerText.Trim();
                mv.Producer = doc.DocumentNode.SelectSingleNode(xpProducer).InnerText.Trim();
                mv.Length = doc.DocumentNode.SelectSingleNode(xpLength).InnerText.Trim();
                mv.Info = doc.DocumentNode.SelectSingleNode(xpInfo).InnerText.Trim();
                
                // Parse ticket info
                log.WriteLog("Parse ticket info");
                HtmlAgilityPack.HtmlNodeCollection listNode = doc.DocumentNode.SelectNodes("//div[@class='mov_sc']");
                List<Ticket> tickets = new List<Ticket>();

                for (int i = 0; i < listNode.Count; i++)
                {
                    string strTheater = listNode[i].SelectSingleNode("span[1]").InnerText.Replace("GLX"," ").Trim();
                    HtmlAgilityPack.HtmlNodeCollection scheduleNode = listNode[i].SelectNodes("./div[2]/div");
                    for (int j = 0; j < scheduleNode.Count; j++)
                    {
                        string showDate = scheduleNode[j].SelectSingleNode("span").InnerText.Trim().Split(',')[1].Replace(".", "/").Trim() ;
                        string showTime = "";
                        for (int k = 0; k < scheduleNode[j].SelectSingleNode("div[2]").ChildNodes.Count; k++)
                        {
                            showTime += scheduleNode[j].SelectSingleNode("div[2]").ChildNodes[k].InnerText.Trim() + " ";
                        }
                        
                        Ticket mvTicket = new Ticket(this.GetType().Name, strTheater, showDate, showTime);
                        tickets.Add(mvTicket);
                    }
                }
                mv.Tickets = tickets;
            }
            catch (Exception e)
            {
                log.WriteLog("##############################################");
                log.WriteLog("Exception " + e.Source + " : " + e.Message);
            }
            log.WriteLog("Parsed : " + mv.DetailLink);
            return mv;
        }

    }
}
