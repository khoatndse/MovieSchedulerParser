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
    class Lotte
    {
        private const string PageURL = @"http://lottecinemavn.com/vi-vn/phim-%C4%91ang-chieu.aspx";
        private Logger log = Logger.GetInstance();
       

        public Dictionary<string, Movie> Parsing(Dictionary<string, Movie> movieList)
        {
            // Connect to lotte website
            HtmlWeb website = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc = website.Load(PageURL);
            log.WriteLog("Connect to " + PageURL);

            // Container div
            HtmlAgilityPack.HtmlNodeCollection listNode = doc.DocumentNode.SelectNodes("//div[@class='dn_contentmv dn_mvtabs1']/ul/li");
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

            string xpName = "div[2]/ul/li[1]";
            string xpProducer = "div[2]/ul/li[5]";
            string xpLength = "div[2]/ul/li[3]";
            string xpType = "div[2]/ul/li[2]";
            string xpImage = ".//img[@class='dn_imgmovies']";
            string xpDetail = "div[2]//a";
            Movie mv = new Movie();
            try
            {
                log.WriteLog("Begin Parse A Movie");
                mv.MvSource = this.GetType().Name;
                mv.Name = movieNode.SelectSingleNode(xpName).InnerText.Trim();
                

                mv.Producer = movieNode.SelectSingleNode(xpProducer).InnerText;
                mv.Producer = CleanLotteData(mv.Producer);

                mv.Length = movieNode.SelectSingleNode(xpLength).InnerText;
                mv.Length = CleanLotteData(mv.Length);

                mv.Type = movieNode.SelectSingleNode(xpType).InnerText.Trim();
                mv.Type = CleanLotteData(mv.Type);

                mv.Image = @"http://lottecinemavn.com/vi-vn" + movieNode.SelectSingleNode(xpImage).Attributes["src"].Value;
                mv.DetailLink = @"http://lottecinemavn.com/vi-vn" + movieNode.SelectSingleNode(xpDetail).Attributes["href"].Value;
                
                // Parse detail of a movie
                log.WriteLog("Parse movie detail at " + mv.DetailLink);
                HtmlWeb website = new HtmlWeb();
                HtmlAgilityPack.HtmlDocument doc = website.Load(mv.DetailLink);

                mv.TrailerLink = "http:" + doc.DocumentNode.SelectSingleNode("//iframe[contains(@src,'youtube')]").Attributes["src"].Value;
                mv.Info = doc.DocumentNode.SelectSingleNode("//div[@class='dn_contentmvdt dn_tabsmvdt1']").InnerText.Trim();
                
                // Parse ticket info
                log.WriteLog("Parse ticket info");
                HtmlAgilityPack.HtmlNodeCollection listNode = doc.DocumentNode.SelectNodes("//div[@class='dn_contentmvdt dn_tabsmvdt2']/div");
                List<Ticket> tickets = new List<Ticket>();
                if (listNode == null)
                {
                }
                else
                {
                    for (int i = 0; i < listNode.Count; i = i + 3)
                    {
                        string strTheater = listNode[i].InnerText.Trim();
                        HtmlAgilityPack.HtmlNodeCollection scheduleNode = listNode[i + 2].SelectNodes(".//table");
                        for (int j = 0; j < scheduleNode.Count; j++)
                        {
                            Regex clean = new Regex("\n|\r");
                            string showDate = scheduleNode[j].SelectNodes(".//div")[0].InnerText.Trim().Replace("-","/");
                            string showTime = clean.Replace(scheduleNode[j].SelectNodes(".//div")[1].InnerText.Trim(), " ");
                            Ticket mvTicket = new Ticket(this.GetType().Name, strTheater, showDate, showTime);
                            tickets.Add(mvTicket);
                        }
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

        private string CleanLotteData(string str)
        {
            if(str.Contains(':'))
            {
                str = str.Split(':')[1];
            }
            Regex trim = new Regex("\n|\r");
            return trim.Replace(str, " ").Trim();
        }
    }
}
