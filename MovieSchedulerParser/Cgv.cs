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
using System.Xml;

namespace MovieSchedulerParser
{
    class Cgv
    {
        private WebBrowser ClientBrowser;
        private const string PageURL = @"https://www.cgv.vn/vn/nowshowing/";
        private const string TheaterURL = @"https://www.cgv.vn/megastarXMLData.aspx?RequestType=GetCinemaList&&visLang=1";
        private Logger log = Logger.GetInstance();
        private Dictionary<string, string> theaterList = new Dictionary<string, string>();


        public Dictionary<string, Movie> Parsing(Dictionary<string, Movie> movieList)
        {
            // Connect to cgv website to parse theatre
            ParseTheater();

            // Connect to CGV showing page
            
            HtmlWeb website = new HtmlWeb();

            HtmlAgilityPack.HtmlDocument doc = website.Load(PageURL);
            log.WriteLog("Connect to " + PageURL);

            // Container div
            HtmlAgilityPack.HtmlNodeCollection listNode = doc.DocumentNode.SelectNodes("//div[@class='landingbody_item']");

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

            string xpName = "div[@class='m_center']/a/span";
            string xpDetail = "div[@class='m_center']/a";
            string xpProducer = "//div[@class='info']/p[1]";
            string xpLength = "div[@class='m_center']/text()[3]";
            string xpType = "div[@class='m_center']/text()[4]";
            string xpImage = "//div[@class='movie-info']/div[@class='photo']/img";
            string dbName = "";
            Movie mv = new Movie();
            try
            {
                log.WriteLog("Begin Parse A Movie");
                mv.MvSource = this.GetType().Name;
                mv.Name = movieNode.SelectSingleNode(xpName).InnerText.Trim();
                mv.DetailLink = @"http://www.cgv.vn" + movieNode.SelectSingleNode(xpDetail).Attributes["href"].Value;
                mv.Length = movieNode.SelectSingleNode(xpLength).InnerText.Split(':')[1].Trim();
                mv.Length = mv.Length.Replace("&#7901;", "ờ").Replace("&#224;", "à").Replace("&#250;", "ú");

                mv.Type = movieNode.SelectSingleNode(xpType).InnerText.Split(':')[1].Trim();
                dbName = movieNode.SelectSingleNode("div[@class='m_right']//div[@class='q_session_time']/a[2]").Attributes["onclick"].Value.ToString().Replace("/vn/", " ").Trim();
                Regex cleanDbName = new Regex(".*\"(.*?)\"");
                Match extractDbName = cleanDbName.Match(dbName);
                dbName = extractDbName.Groups[1].Value;

                // Parse detail of a movie
                log.WriteLog("Parse movie detail at " + mv.DetailLink);
                HtmlWeb website = new HtmlWeb();
                HtmlAgilityPack.HtmlDocument doc = website.Load(mv.DetailLink);

                mv.Producer = doc.DocumentNode.SelectSingleNode(xpProducer).InnerText.Split(':')[1].Trim();
                mv.Image = @"http://www.cgv.vn" + doc.DocumentNode.SelectSingleNode(xpImage).Attributes["src"].Value;
                mv.TrailerLink = doc.DocumentNode.SelectSingleNode("//div[@id='tab1']").InnerHtml.ToString();

                Regex pattern = new Regex("ytlink=(.*?)&");
                Match matcher = pattern.Match(mv.TrailerLink);
                mv.TrailerLink =@"http://www.youtube.com/embed/" + matcher.Groups[1].Value;

                mv.Info = doc.DocumentNode.SelectSingleNode("//div[@class='desc']").InnerText.Trim();

                // Parse ticket info
                log.WriteLog("Parse ticket info");
                //HtmlAgilityPack.HtmlNodeCollection listNode = doc.DocumentNode.SelectNodes("//div[@class='dn_contentmvdt dn_tabsmvdt2']/div");
                List<Ticket> tickets = new List<Ticket>();

                XmlDocument xmlDoc = new XmlDocument();
                foreach (var item in theaterList)
                {
                    string ticketInfo = String.Format(@"https://www.cgv.vn/megastarXMLData.aspx?RequestType=GetSessionTimes&&CinemaID={0}&&MovieName={1}&&Time=TodayAndTomorrow&&visLang=1", item.Key, dbName);
                    xmlDoc.Load(ticketInfo);
                    log.WriteLog("Ticket info of " + dbName + " at " + ticketInfo);

                    // read detail
                    XmlElement rootElement = xmlDoc.DocumentElement;
                    XmlNodeList dateList = rootElement.GetElementsByTagName("date");
                    if (dateList.Count != 0)
                    {
                        for (int i = 0; i < dateList.Count; i++)
                        {
                            if (dateList[i].Attributes.Count != 0)
                            {
                                string showDate = dateList[i].Attributes["name"].Value.ToString();
                                string showTime = "";
                                for (int j = 0; j < dateList[i].ChildNodes.Count; j++)
                                {
                                    XmlNodeList timeList = dateList[i].ChildNodes[j].ChildNodes;
                                    for (int k = 0; k < timeList.Count; k++)
                                    {
                                        if (timeList[k].Name.Equals("value"))
                                        {
                                            string time = timeList[k].InnerText;
                                            Regex clean = new Regex("<a .*>(.*?)</a>");
                                            Match extract = clean.Match(time);
                                            time = extract.Groups[1].Value;
                                            if (time.Contains("AM"))
                                            {
                                                time = time.Replace("AM", " ").Trim();
                                            }
                                            else if (time.Contains("PM"))
                                            {
                                                time = time.Replace("PM", " ").Trim();
                                                int hour = int.Parse(time.Split(':')[0]) + 12;
                                                string min = time.Split(':')[1];
                                                time = hour + ":" + min;
                                            }
                                            showTime += time + " ";
                                            break;
                                        }
                                    }
                                }
                                Ticket mvTicket = new Ticket(this.GetType().Name, item.Value, showDate, showTime);
                                tickets.Add(mvTicket);
                            }
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


        private void ParseTheater()
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(TheaterURL);
            log.WriteLog("Connect to " + TheaterURL);
            XmlElement rootElement = xmlDoc.DocumentElement;
            XmlNodeList cinemaList = rootElement.GetElementsByTagName("Cinema");

            for (int i = 0; i < cinemaList.Count; i++)
            {
                string id = "";
                string name = "";
                for (int j = 0; j < cinemaList[i].ChildNodes.Count; j++)
                {
                    if (cinemaList[i].ChildNodes[j].Name.Equals("Cinema_strID"))
                    {
                        id = cinemaList[i].ChildNodes[j].InnerText.ToString();
                    }
                    if (cinemaList[i].ChildNodes[j].Name.Equals("Cinema_strName"))
                    {
                        name = cinemaList[i].ChildNodes[1].InnerText.ToString().Split('-')[1].Trim();
                    }
                }

                theaterList.Add(id, name);
            }
            log.WriteLog("Parsed " + theaterList.Count + " theaters");
        }

    }
}
