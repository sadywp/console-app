using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using N = System.Net;
using System.Threading;
using T = System.Timers;
using System.Text.RegularExpressions;

namespace ConsoleApp
{
    class Program
    {
        //数据刷新，抓取定时器
        private static T.Timer TimerHnaderAutoScraper;
        static BackgroundWorker bgw = new BackgroundWorker();
        static Program Instance = new Program();
        static string sourcetype = string.Empty;
        static void Main(string[] args)
        {

            //MongoHelper.FindNews();
            //Instance.WacheDataDetailzHuizhouHtml("");
            TimerHnaderAutoScraper = new T.Timer(1000 * 60 * 5);//五分钟执行一次
            TimerHnaderAutoScraper.Enabled = true;
            TimerHnaderAutoScraper.AutoReset = true;
            TimerHnaderAutoScraper.Elapsed += new T.ElapsedEventHandler(Instance.TimerHnaderAutoScraper_Tick);

            sourcetype = "boluo";
            bgw.DoWork -= new DoWorkEventHandler(Instance.bgw_DoWork);
            bgw.DoWork += new DoWorkEventHandler(Instance.bgw_DoWork);
            bgw.RunWorkerAsync();
            Console.ReadKey();
        }
        void TimerHnaderAutoScraper_Tick(object sender, EventArgs args)
        {
            if (!bgw.IsBusy)
                bgw.RunWorkerAsync();

        }
        private void bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            CatchData(sourcetype);
            if (sourcetype == "boluo") sourcetype = "huizhou";
            else sourcetype = "boluo";
        }
        private void CatchData(string sourceTypeStr)
        {
            Console.WriteLine(sourceTypeStr);
            List<Information> datas = null;

            datas = GetWebData(sourceTypeStr);
            if (datas != null && datas.Count > 0)
            {
                MongoHelper.AddNews(datas);
            }


        }
        private List<Information> GetWebData(string datatype)
        {
            List<Information> data = null;
            switch (datatype)
            {
                case "boluo":
                    data = WacheDataboluo();
                    break;
                case "huizhou":

                    data = WacheDataHuizhou();
                    break;
                default:
                    data = WacheDataboluo();
                    break;
            }
            return data;
        }
        private List<Information> WacheDataboluo()
        {
            List<Information> datas = new List<Information>();
            N.WebClient client = new N.WebClient();
            client.Encoding = Encoding.GetEncoding("gb2312");
            string strdata = client.DownloadString("http://www.bljy.cn/cms/html/WSBS/TZGG/");

            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(strdata);
            HtmlNode rootnode = doc.DocumentNode;
            string xapthstr = "//ul[@class='e2']/li";
            HtmlNodeCollection nodes = rootnode.SelectNodes(xapthstr);

            if (nodes != null)
            {

                Information data = null;
                foreach (HtmlNode item in nodes)
                {
                    data = new Information();
                    HtmlNodeCollection chideNodes = item.ChildNodes;
                    data.newsid = Guid.NewGuid().ToString();
                    data.title = chideNodes[5].InnerText.Replace('\n', ' ').Replace('\t', ' ').Replace('\r', ' ').Trim();
                    data.href = @"http://www.bljy.cn" + chideNodes[5].Attributes[0].Value;
                    data.date = Convert.ToDateTime(chideNodes[7].ChildNodes[2].InnerText.Replace('\n', ' ').Replace('\t', ' ').Replace('\r', ' ').Trim());
                    data.click_count = chideNodes[7].ChildNodes[4].InnerText.Replace('\n', ' ').Replace('\t', ' ').Replace('\r', ' ').Trim();
                    data.summary = chideNodes[9].InnerText.Replace('\n', ' ').Replace('\t', ' ').Replace('\r', ' ').Trim();
                    data.source_type = 1;
                    data.details = WacheDataDetailsBoluo(data.href);
                    data.create_at = DateTime.Now;
                    data.update_at = DateTime.Now;
                    datas.Add(data);
                }
            }
            return datas;
        }
        private List<Information> WacheDataHuizhou()
        {
            List<Information> datas = new List<Information>();
            N.WebClient client = new N.WebClient();
            client.Encoding = Encoding.GetEncoding("utf-8");
            string strdata = client.DownloadString(@"http://www.hzjy.edu.cn/List.aspx?nid=1");

            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(strdata);
            HtmlNode rootnode = doc.DocumentNode;
            string xapthstr = "//table[@class='liTable']/tr";
            HtmlNodeCollection nodes = rootnode.SelectNodes(xapthstr);

            if (nodes != null)
            {

                Information data = null;
                foreach (HtmlNode item in nodes)
                {
                    data = new Information();
                    HtmlNodeCollection chideNodes = item.ChildNodes;
                    data.newsid = Guid.NewGuid().ToString();
                    data.title = chideNodes[1].ChildNodes[0].InnerText.Replace('\n', ' ').Replace('\t', ' ').Replace('\r', ' ').Replace("&nbsp;", " ").Remove(0, 1).Trim();
                    data.href = @"http://www.hzjy.edu.cn/" + chideNodes[1].ChildNodes[0].Attributes[2].Value;
                    data.date = Convert.ToDateTime(chideNodes[3].InnerText.Replace('\n', ' ').Replace('\t', ' ').Replace('\r', ' ').Replace("&nbsp;", " ").Trim());
                    data.click_count = "noting";
                    data.summary = chideNodes[1].ChildNodes[0].Attributes[1].Value.Replace('\n', ' ').Replace('\t', ' ').Replace('\r', ' ').Replace("&nbsp;", " ").Trim();
                    data.source_type = 2;
                    data.details = WacheDataDetailsHuizhou(data.href);
                    data.create_at = DateTime.Now;
                    data.update_at = DateTime.Now;
                    datas.Add(data);
                }

            }
            return datas;
        }

        private string WacheDataDetailsBoluoHtml(string url)
        {
            url = "http://www.bljy.cn/cms/html/WSBS/TZGG/201609/02-4352.html";
            string baseUrl = "http://www.bljy.cn";
            string strdata = "";
            string xpath = "";
            N.WebClient client = new N.WebClient();
            client.Encoding = Encoding.GetEncoding("gb2312");
            strdata = client.DownloadString(url);

            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            //var domain = AppDomain.CurrentDomain.BaseDirectory + "details.html";
            //doc.Load(domain, System.Text.Encoding.UTF8);
            doc.LoadHtml(strdata);
            HtmlNode rootnode = doc.DocumentNode;

            //----title
            xpath = "//div[@class='title']";
            HtmlNodeCollection nodes = rootnode.SelectNodes(xpath);
            var title = nodes[0].OuterHtml;
            //----info
            xpath = "//div[@class='info']";
             nodes = rootnode.SelectNodes(xpath);
             var info = nodes[0].OuterHtml;
             var count = nodes[0].ChildNodes[9].Attributes[0].Value;
             count = client.DownloadString(baseUrl + count);//document.write('975');\r\n
             count = count.Substring(count.IndexOf("'") + 1, count.LastIndexOf("'") - count.IndexOf("'") - 1);
             info = info.Replace("次", count + "次");
             //----content
             xpath = "//div[@class='content']";
             nodes = rootnode.SelectNodes(xpath);
             var content = nodes[0].OuterHtml;
             var deatils = "<div class='viewbox'>" + title + info + content + "</div>";
            return "";
        }
        private string WacheDataDetailHuizhouHtml(string url) {
            url = "http://www.hzjy.edu.cn/detail.aspx?ID=41028";
            string strdata = "";
            string xpath = "";
            N.WebClient client = new N.WebClient();
            client.Encoding = Encoding.GetEncoding("utf-8");
            strdata = client.DownloadString(url);

            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            //var domain = AppDomain.CurrentDomain.BaseDirectory + "details1.html";
            //doc.Load(domain, System.Text.Encoding.UTF8);
            doc.LoadHtml(strdata);
            HtmlNode rootnode = doc.DocumentNode;

            //----title
            xpath = "//div[@id='dMain']";
            HtmlNodeCollection nodes = rootnode.SelectNodes(xpath);
            var title = nodes[0].OuterHtml;
            return "";
        }

        private Details WacheDataDetailsBoluo(string url)
        {
            //url = "http://www.bljy.cn/cms/html/WSBS/TZGG/201609/02-4352.html";
            string baseUrl = "http://www.bljy.cn";
            string strdata = "";
            string xpath = "";
            N.WebClient client = new N.WebClient();
            client.Encoding = Encoding.GetEncoding("gb2312");
            strdata = client.DownloadString(url);

            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            //var domain = AppDomain.CurrentDomain.BaseDirectory + "details.html";
            //doc.Load(domain, System.Text.Encoding.UTF8);
            doc.LoadHtml(strdata);
            HtmlNode rootnode = doc.DocumentNode;

            //----title
            xpath = "//div[@class='title']";
            HtmlNodeCollection nodes = rootnode.SelectNodes(xpath);
            var title = NoHTML(nodes[0].InnerText);
            //----info
            xpath = "//div[@class='info']";
            nodes = rootnode.SelectNodes(xpath);
            var info = NoHTML(nodes[0].InnerText);
            var count = nodes[0].ChildNodes[9].Attributes[0].Value;
            count = client.DownloadString(baseUrl + count);//document.write('975');\r\n
            count = count.Substring(count.IndexOf("'") + 1, count.LastIndexOf("'") - count.IndexOf("'") - 1);
            info = info.Replace("次", count + "次");
            //----content
            xpath = "//div[@class='content']";
            nodes = rootnode.SelectNodes(xpath);
            var content = NoHTML(nodes[0].InnerText);
            return new Details { title = title, info = info, content = content };
        }
        private Details WacheDataDetailsHuizhou(string url)
        {

            //url = "http://www.hzjy.edu.cn/detail.aspx?ID=40674";
            //string baseUrl = "http://www.bljy.cn";
            string strdata = "";
            string xpath = "";
            N.WebClient client = new N.WebClient();
            client.Encoding = Encoding.GetEncoding("utf-8");
            strdata = client.DownloadString(url);

            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            //var domain = AppDomain.CurrentDomain.BaseDirectory + "details1.html";
            //doc.Load(domain, System.Text.Encoding.UTF8);
            doc.LoadHtml(strdata);
            HtmlNode rootnode = doc.DocumentNode;

            //----title
            xpath = "//span[@id='Main_labTitle']";
            HtmlNodeCollection nodes = rootnode.SelectNodes(xpath);
            var title = NoHTML(nodes[0].InnerText);

            //----info
            xpath = "//span[@id='Main_LabDate']";
            nodes = rootnode.SelectNodes(xpath);
            var info = NoHTML(nodes[0].InnerText);

            //----content
            xpath = "//div[@id='DivContent']";
            nodes = rootnode.SelectNodes(xpath);
            var content = nodes[0].InnerText.Replace("&nbsp;", "").Replace("&mdash;", "—").Trim();
            return new Details { title = title, info = info, content = content };
        }


        public static string NoHTML(string Htmlstring)
        {
            //删除脚本   
            Htmlstring = Regex.Replace(Htmlstring, @"<script[^>]*?>.*?</script>", "", RegexOptions.IgnoreCase);
            //删除HTML   
            Htmlstring = Regex.Replace(Htmlstring, @"<(.[^>]*)>", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"([\r\n])[\s]+", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"-->", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"<!--.*", "", RegexOptions.IgnoreCase);

            Htmlstring = Regex.Replace(Htmlstring, @"&(quot|#34);", "\"", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(amp|#38);", "&", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(lt|#60);", "<", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(gt|#62);", ">", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(nbsp|#160);", "   ", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(iexcl|#161);", "\xa1", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(cent|#162);", "\xa2", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(pound|#163);", "\xa3", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(copy|#169);", "\xa9", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&#(\d+);", "", RegexOptions.IgnoreCase);

            Htmlstring.Replace("<", "");
            Htmlstring.Replace(">", "");
            Htmlstring.Replace("\r\n", "");
            //Htmlstring = HttpContext.Current.Server.HtmlEncode(Htmlstring).Trim();

            return Htmlstring;
        }
    }
}
