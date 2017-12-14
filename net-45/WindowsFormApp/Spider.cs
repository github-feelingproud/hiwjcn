using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlAgilityPack;
using Lib.net;
using Lib.core;
using Lib.helper;
using Abot.Crawler;
using Abot.Poco;

namespace WindowsFormApp
{
    public partial class Spider : Form
    {
        public Spider()
        {
            InitializeComponent();
        }

        private void Spider_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //
        }
    }

    class SpiderHelper
    {
        static void Main___(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            //Uncomment only one of the following to see that instance in action
            var crawler = GetManuallyConfiguredWebCrawler();

            crawler.PageCrawlCompleted += (sender, e) =>
            {
                //
            };

            //Start the crawl
            //This is a synchronous call
            var result = crawler.Crawl(new Uri("http://www.autohome.com.cn/"));

            //Now go view the log.txt file that is in the same directory as this executable. It has
            //all the statements that you were trying to read in the console window :).
            //Not enough data being logged? Change the app.config file's log4net log level from "INFO" TO "DEBUG"
            Console.ReadLine();
        }

        private static IWebCrawler GetManuallyConfiguredWebCrawler()
        {
            IWebCrawler crawler = new PoliteWebCrawler();

            var dofilter = false;
            if (dofilter)
            {
                //Register a lambda expression that will make Abot not crawl any url that has the word "ghost" in it.
                //For example http://a.com/ghost, would not get crawled if the link were found during the crawl.
                //If you set the log4net log level to "DEBUG" you will see a log message when any page is not allowed to be crawled.
                //NOTE: This is lambda is run after the regular ICrawlDecsionMaker.ShouldCrawlPage method is run.
                crawler.ShouldCrawlPage((pageToCrawl, crawlContext) =>
                {
                    if (pageToCrawl.Uri.AbsoluteUri.Contains("ghost"))
                        return new CrawlDecision { Allow = false, Reason = "Scared of ghosts" };

                    return new CrawlDecision { Allow = true };
                });

                //Register a lambda expression that will tell Abot to not download the page content for any page after 5th.
                //Abot will still make the http request but will not read the raw content from the stream
                //NOTE: This lambda is run after the regular ICrawlDecsionMaker.ShouldDownloadPageContent method is run
                crawler.ShouldDownloadPageContent((crawledPage, crawlContext) =>
                {
                    if (crawlContext.CrawledCount >= 5)
                        return new CrawlDecision { Allow = false, Reason = "We already downloaded the raw page content for 5 pages" };

                    return new CrawlDecision { Allow = true };
                });

                //Register a lambda expression that will tell Abot to not crawl links on any page that is not internal to the root uri.
                //NOTE: This lambda is run after the regular ICrawlDecsionMaker.ShouldCrawlPageLinks method is run
                crawler.ShouldCrawlPageLinks((crawledPage, crawlContext) =>
                {
                    if (!crawledPage.IsInternal)
                        return new CrawlDecision { Allow = false, Reason = "We dont crawl links of external pages" };

                    return new CrawlDecision { Allow = true };
                });
            }

            return crawler;
        }
    }

}
