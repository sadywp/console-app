﻿using HtmlAgilityPack;
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
        static bool IsExcute=false;
        //每天执行6次
        static string[] TimeArray = new string[6];
        static void Main(string[] args)
        {
            TimeArray[0] = "00:00";
            TimeArray[0] = "04:00";
            TimeArray[0] = "08:00";
            TimeArray[0] = "12:00";
            TimeArray[0] = "16:00";
            TimeArray[0] = "20:00";
            //MongoHelper.ExportData();
            //Instance.WacheDataDetailHuizhouHtml("");
            //
            TimerHnaderAutoScraper = new T.Timer(1000 * 60 * 1);//一分钟执行一次
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
            var nowTime = DateTime.Now.ToString("hh:mm");
            var ishave = TimeArray.Select(p => p.Equals(nowTime)).FirstOrDefault();
            if (!IsExcute) {
                CatchData(sourcetype);
                IsExcute = true;
            }
            if (ishave)
            {
                CatchData(sourcetype);
            }
            //Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd hh:mm"));
            // CatchData(sourcetype);
        }
        private void CatchData(string sourceTypeStr)
        {
            Console.WriteLine(sourceTypeStr);
            List<Information> datas = null;
            string msg = "";
            string msg_mongo = "";
            string typrStr = "";

            if (sourcetype == "boluo") typrStr = "博罗新闻";
            else typrStr = "惠州新闻";

            for (int i = 1; i <= 5; i++)
            {
                Console.WriteLine("获取" + typrStr + " 第" + i + "次尝试！" + DateTime.Now.ToString());
                datas = GetWebData(sourceTypeStr, ref msg);
                if (datas != null && datas.Count > 0)
                {

                    Console.WriteLine("成功解析" + typrStr+" " + datas.Count + "条" + DateTime.Now.ToString());

                    MongoHelper.AddNews(datas, sourcetype, ref msg_mongo);
                    Console.WriteLine(typrStr +" "+ msg_mongo + DateTime.Now.ToString());
                    break;
                }
            }
            if (sourcetype == "boluo") sourcetype = "huizhou";
            else sourcetype = "boluo";
            if (sourcetype == "boluo") typrStr = "博罗新闻";
            else typrStr = "惠州新闻";

            for (int i = 1; i <= 5; i++)
            {
                Console.WriteLine("获取" + typrStr + " 第" + i + "次尝试！" + DateTime.Now.ToString());
                datas = GetWebData(sourceTypeStr, ref msg);
                if (datas != null && datas.Count > 0)
                {

                    Console.WriteLine("成功解析" + typrStr+" " + datas.Count + "条" + DateTime.Now.ToString());
                    MongoHelper.AddNews(datas, sourcetype, ref msg_mongo);
                    Console.WriteLine(typrStr +" "+ msg_mongo + DateTime.Now.ToString());

                    break;
                }
            }

            if (sourcetype == "boluo") sourcetype = "huizhou";
            else sourcetype = "boluo";
        }
        private List<Information> GetWebData(string datatype, ref string msg)
        {
            List<Information> data = null;
            switch (datatype)
            {
                case "boluo":
                    data = WacheDataboluo(ref msg);
                    break;
                case "huizhou":

                    data = WacheDataHuizhou(ref msg);
                    break;
                default:
                    data = WacheDataboluo(ref msg);
                    break;
            }
            return data;
        }
        private List<Information> WacheDataboluo(ref string msg)
        {
            List<Information> datas = new List<Information>();
            N.WebClient client = new N.WebClient();
            client.Encoding = Encoding.GetEncoding("gb2312");
            string strdata = "";
            try
            {
                strdata = client.DownloadString("http://www.bljy.cn/cms/html/WSBS/TZGG/");
                msg = "获取博罗新闻列表成功！";
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return null;
            }


            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(strdata);
            HtmlNode rootnode = doc.DocumentNode;
            if (rootnode == null)
            {
                msg = "解析博罗新闻列表失败";
                return null;
            }
            string xapthstr = "//ul[@class='e2']/li";
            HtmlNodeCollection nodes = rootnode.SelectNodes(xapthstr);

            if (nodes != null)
            {

                Information data = null;
                foreach (HtmlNode item in nodes)
                {
                    var detailshtml = "";
                    data = new Information();
                    HtmlNodeCollection chideNodes = item.ChildNodes;
                    data.newsid = Guid.NewGuid().ToString();
                    data.title = chideNodes[5].InnerText.Replace('\n', ' ').Replace('\t', ' ').Replace('\r', ' ').Trim();
                    data.href = @"http://www.bljy.cn" + chideNodes[5].Attributes[0].Value;
                    data.date = Convert.ToDateTime(chideNodes[7].ChildNodes[2].InnerText.Replace('\n', ' ').Replace('\t', ' ').Replace('\r', ' ').Trim());
                    data.click_count = chideNodes[7].ChildNodes[4].InnerText.Replace('\n', ' ').Replace('\t', ' ').Replace('\r', ' ').Trim();
                    data.summary = chideNodes[9].InnerText.Replace('\n', ' ').Replace('\t', ' ').Replace('\r', ' ').Trim();
                    data.source_type = 1;
                    data.details = WacheDataDetailsBoluo(data.href, ref detailshtml);
                    data.detailshtml = detailshtml;
                    data.create_at = DateTime.Now;
                    data.update_at = DateTime.Now;
                    if (data.details != null)
                    {
                        datas.Add(data);
                    }
                }
            }
            return datas;
        }
        private List<Information> WacheDataHuizhou(ref string msg)
        {
            List<Information> datas = new List<Information>();
            N.WebClient client = new N.WebClient();
            client.Encoding = Encoding.GetEncoding("utf-8");
            string strdata = "";
            try
            {
                strdata = client.DownloadString(@"http://www.hzjy.edu.cn/List.aspx?nid=1");
                msg = "获取惠州新闻列表成功！";
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return null;
            }


            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(strdata);
            HtmlNode rootnode = doc.DocumentNode;
            if (rootnode == null)
            {
                msg = "解析惠州新闻列表失败!";
                return null;
            }
            string xapthstr = "//table[@class='liTable']/tr";
            HtmlNodeCollection nodes = rootnode.SelectNodes(xapthstr);

            if (nodes != null)
            {

                Information data = null;
                foreach (HtmlNode item in nodes)
                {
                    var detailshtml = "";
                    data = new Information();
                    HtmlNodeCollection chideNodes = item.ChildNodes;
                    data.newsid = Guid.NewGuid().ToString();
                    data.title = chideNodes[1].ChildNodes[0].InnerText.Replace('\n', ' ').Replace('\t', ' ').Replace('\r', ' ').Replace("&nbsp;", " ").Remove(0, 1).Trim();
                    data.href = @"http://www.hzjy.edu.cn/" + chideNodes[1].ChildNodes[0].Attributes[2].Value;
                    data.date = Convert.ToDateTime(chideNodes[3].InnerText.Replace('\n', ' ').Replace('\t', ' ').Replace('\r', ' ').Replace("&nbsp;", " ").Trim());
                    data.click_count = "noting";
                    data.summary = chideNodes[1].ChildNodes[0].Attributes[1].Value.Replace('\n', ' ').Replace('\t', ' ').Replace('\r', ' ').Replace("&nbsp;", " ").Trim();
                    data.source_type = 2;
                    data.details = WacheDataDetailsHuizhou(data.href, ref detailshtml);
                    data.detailshtml = detailshtml;
                    data.create_at = DateTime.Now;
                    data.update_at = DateTime.Now;
                    if (data.details != null)
                    {
                        datas.Add(data);
                    }
                }

            }
            return datas;
        }

        private string WacheDataDetailsBoluoHtml(string htmlstr)
        {
            // url = "http://www.bljy.cn/cms/html/WSBS/TZGG/201609/02-4352.html";
            string baseUrl = "http://www.bljy.cn";
            string xpath = "";
            N.WebClient client = new N.WebClient();
            client.Encoding = Encoding.GetEncoding("gb2312");
            //strdata = client.DownloadString(url);

            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            //var domain = AppDomain.CurrentDomain.BaseDirectory + "details.html";
            //doc.Load(domain, System.Text.Encoding.GetEncoding("utf-8"));
            doc.LoadHtml(htmlstr);
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
            content = content.Replace("href=\"", "href=\"" + baseUrl);
            var deatils = "<div class='viewbox'>" + title + info + content + "</div>";
            return deatils;
        }
        private string WacheDataDetailHuizhouHtml(string htmlstr)
        {
            //url = "http://www.hzjy.edu.cn/detail.aspx?ID=41028";
            string xpath = "";
            //N.WebClient client = new N.WebClient();
            //client.Encoding = Encoding.GetEncoding("utf-8");
            // strdata = client.DownloadString(url);

            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            //var domain = AppDomain.CurrentDomain.BaseDirectory + "details1.html";
            //doc.Load(domain, System.Text.Encoding.UTF8);
            doc.LoadHtml(htmlstr);
            HtmlNode rootnode = doc.DocumentNode;

            //----title
            xpath = "//div[@id='dMain']";
            HtmlNodeCollection nodes = rootnode.SelectNodes(xpath);
            var content = nodes[0].OuterHtml;
            return content;
        }

        private Details WacheDataDetailsBoluo(string url, ref string htmlstr)
        {
            //url = "http://www.bljy.cn/cms/html/WSBS/TZGG/201609/02-4352.html";
            string baseUrl = "http://www.bljy.cn";
            string strdata = "";
            string xpath = "";
            N.WebClient client = new N.WebClient();
            client.Encoding = Encoding.GetEncoding("gb2312");
            try
            {
                strdata = client.DownloadString(url);
            }
            catch (Exception ex)
            {

                return null;
            }


            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            //var domain = AppDomain.CurrentDomain.BaseDirectory + "details.html";
            //doc.Load(domain, System.Text.Encoding.UTF8);
            doc.LoadHtml(strdata);
            HtmlNode rootnode = doc.DocumentNode;
            if (rootnode == null) return null;
            //----title
            xpath = "//div[@class='title']";
            HtmlNodeCollection nodes = rootnode.SelectNodes(xpath);
            var title = NoHTML(nodes[0].InnerText);
            //----info
            xpath = "//div[@class='info']";
            nodes = rootnode.SelectNodes(xpath);
            var info = NoHTML(nodes[0].InnerText);
            var count = nodes[0].ChildNodes[9].Attributes[0].Value;
            try
            {
                count = client.DownloadString(baseUrl + count);//document.write('975');\r\n
                count = count.Substring(count.IndexOf("'") + 1, count.LastIndexOf("'") - count.IndexOf("'") - 1);
            }
            catch (Exception ex)
            {

                count = "0";
            }

            info = info.Replace("次", count + "次");
            //----content
            xpath = "//div[@class='content']";
            nodes = rootnode.SelectNodes(xpath);
            var content = NoHTML(nodes[0].InnerText);
            htmlstr = WacheDataDetailsBoluoHtml(strdata);

            return new Details { title = title, info = info, content = content };
        }
        private Details WacheDataDetailsHuizhou(string url, ref string htmlstr)
        {

            //url = "http://www.hzjy.edu.cn/detail.aspx?ID=40674";
            //string baseUrl = "http://www.bljy.cn";
            string strdata = "";
            string xpath = "";
            N.WebClient client = new N.WebClient();
            client.Encoding = Encoding.GetEncoding("utf-8");
            try
            {
                strdata = client.DownloadString(url);
            }
            catch (Exception ex)
            {

                return null;
            }


            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            //var domain = AppDomain.CurrentDomain.BaseDirectory + "details1.html";
            //doc.Load(domain, System.Text.Encoding.UTF8);
            doc.LoadHtml(strdata);
            HtmlNode rootnode = doc.DocumentNode;
            if (rootnode == null) return null;
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
            htmlstr = WacheDataDetailHuizhouHtml(strdata);
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
