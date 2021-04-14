using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        #region khai nao
        static String webPath1 = "https://api.naturalreaders.com/v2/tts/macspeak?t=";
        static String webPath2 = "";
        static String videoTitle;
        static String textTile;
        static int numOfLineHeader = 35;
        static int numOfLine = 40;
        static int fontSize = 64;
        static String[] numline;
        static int numlinecount;
        static String pathTbin = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
        static String pathRun = pathTbin + "\\ffmpeg\\" + "ffmpeg.exe";
        static String finalVideo = pathTbin + "\\finalVideo.mp4";
        static String outVideo = pathTbin + "\\outVideo.mp4";
        static String finalVideoTemp = pathTbin + "\\finalVideoTemp.mp4";
        static String pathLogo = pathTbin + "\\ffmpeg\\" + "logo.png";
        static String pathIntro = pathTbin + "\\ffmpeg\\" + "intro.mp4";
        static String pathImage = pathTbin + "\\ffmpeg\\" + "IMG.Jpeg";
        static String pathImageLogo = pathTbin + "\\IMGLG.Jpeg";
        static String pathTieuDeMp3 = pathTbin + "\\ffmpeg\\" + "tieudechuatext.mp3";
        static String pathMp3 = pathTbin + "\\ffmpeg\\" + "MP3.mp3";
        static String pathTieuDeChuaText = pathTbin + "\\tieudechuatext.mp4";
        static String pathTieuDe = pathTbin + "\\tieude.mp4";
        static String font = pathTbin.Replace("\\", "/") + "/ffmpeg/duy.ttf";
        static String pathVideo = pathTbin + "\\outVideo.mp4";
        static String pathThumb = pathTbin + "\\thumb.Jpeg";
        static String mergeIntro2video = pathTbin + "\\ffmpeg\\" + "mergeIntro2video.txt";
        static Process process = null;
        String txInputText;
        static int lengtSplit = 4200;
        static string tempLinkImg = "https://static1.squarespace.com/static/5728e34fd51cd4809e7aefe0/t/57b14838440243d6c7f366bf/1471236160861/3051795-poster-p-1-fast-company-is-hiring-a-senior-editor-for-news.jpg?format=1500w";

        // xu ly load bai nao
       

        string[] url = { "http://www.politico.com/rss/congress.xml" , //1
                "http://www.politico.com/rss/healthcare.xml",//2
                "http://www.politico.com/rss/defense.xml",//3
                "http://www.politico.com/rss/economy.xml",//4
                "http://www.politico.com/rss/energy.xml",//5
                "https://rss.politico.com/politics-news.xml",//6
                "http://www.washingtontimes.com/rss/headlines/news/politics/"//7
            };
        static int indexNewsNow = 0;
        static int indexSize = 0;
        static int numOfNews = 7;
        string[] linkArrayLink = new string[80];
        string[] linkArrayLinkBuff3day = new string[300];

        string pathTxt = @"MyText.txt";

        //link bai vao hien tai
        string linkUpload;
        #endregion ////
        public Form1()
        {
            InitializeComponent();
            readTextLink();
            readRss();

            //Test 
            //doNews();

            //dinh thoi reset 24h -- lay bai bao moi moi 24h
            timer1.Interval = 86400000;
            timer1.Enabled = true;
            timer1.Tick += new EventHandler(resetNews);
            timer1.Start();
            //dinh thoi goi moi 27p -- upload bao bao moi moi 20p
            timer2.Interval = 1620000;
            timer2.Enabled = true;
            timer2.Tick += new EventHandler(doNews);
            timer2.Start();
            //dinh thoi hien thoi 3ngay -- lam moi du lieu check trung moi 3 ngay
            timer3.Interval = 259200000;
            timer3.Enabled = true;
            timer3.Tick += new EventHandler(resetAfter3Day);
            timer3.Start();
            

            //check neu 24h thi reset
        }
        void readTextLink()
        {
            if (File.Exists(pathTxt))
            {
                string[] temArr = File.ReadAllLines(pathTxt);
                foreach(string text in temArr)
                {
                    linkArrayLinkBuff3day[indexSize++] = text;
                }
            }

        }
        void writeTextLink(string link)
        {
            if (!File.Exists(pathTxt))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(pathTxt))
                {
                    sw.WriteLine(link);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(pathTxt))
                {
                    sw.WriteLine(link);
                }
            }
        }
        private void resetAfter3Day(object sender, EventArgs e)
        {
            linkArrayLinkBuff3day = new string[300];
            indexSize = 0;
            if (File.Exists(pathTxt))
            {
                File.Delete(pathTxt);
            }
            /*
            if (lbstatus.InvokeRequired)
            {
                lbstatus.Invoke(new MethodInvoker(delegate {; lbstatus.Text = "Se bat dau trong " + timer2.ToString(); }));
            }
            */
            }
       private void doNews(object sender, EventArgs e)
       //private void doNews()
        {
            //xoa clip
            xoaMain();
            xoaClipTam();
            if (indexNewsNow >= 300)
                return;
            linkUpload = linkArrayLink[indexNewsNow++];
            if (linkUpload != null && linkUpload.Length> 10)
            {
                getParseNews(linkUpload);
                //goi han parse link
                if (txTieude.Text.Length > 0 && txLink.Text.Length > 0 && rtInput.Text.Length > 300 && tag.Text.Length > 0)
                {
                    webPath1 = "https://api.naturalreaders.com/v" + ver.Value + "/tts/macspeak?" + "&apikey=" + txtApi.Text + "&t=";
                    lbTimeEnd.Text = "--";
                    //reset param
                    lbstatus.Text = "Bắt đầu";
                    webPath2 = "&r=" + numGiong.Value;
                    String tileGetVoid = txTieude.Text.Replace(" — ", " - ");
                    videoTitle = tileGetVoid;
                    String txLinkIput = txLink.Text;
                    txInputText = rtInput.Text.Replace("&", " and ").Replace(".)", ").").Replace(" — ", " - ");
                    getDataFromWeb(tileGetVoid, txLinkIput, txInputText);
                }
            }
            else
            {
                return;
            }
        }

        //cu 24h reset doi lai ham nay upload 1 clip
        private void resetNews(object sender, EventArgs e)
        {
            indexNewsNow = 0;
            linkArrayLink = new string[80];
            readRss();
            //asdfad
        }
        //su dung lay thong tin politico
        void readRssPolitico(String url)
        {
            //StreamReader inStream;
            //WebRequest webRequest;
            //WebResponse webresponse;
            //webRequest = WebRequest.Create("https://www.politico.com/news/2020/06/02/rod-rosenstein-senate-russia-295155");
            //webresponse = webRequest.GetResponse();
            //inStream = new StreamReader(webresponse.GetResponseStream());
            //String textBox = inStream.ReadToEnd();
                
            //XmlReader reader = XmlReader.Create(url);
            //SyndicationFeed feed = SyndicationFeed.Load(reader);
            //reader.Close();
            //if (feed != null && feed.Items != null && feed.Items.Count() > 0)
            //{
            //    SyndicationItem item = feed.Items.ElementAt(0);
            //    if (item.Links != null && item.Links.Count > 0 && item.Links[0].Uri != null && item.Links[0].Uri.AbsoluteUri != null)
            //    {

            //    }
            //}

            string url1 = "https://rss.politico.com/politics-news.xml";
            XmlReader reader = XmlReader.Create(url1);
            SyndicationFeed feed = SyndicationFeed.Load(reader);
            reader.Close();
            foreach (SyndicationItem item in feed.Items)
            {
                String subject = item.Title.Text;
                String summary = item.Summary.Text;
                StringBuilder sb = new StringBuilder();
                foreach (SyndicationElementExtension extension in item.ElementExtensions)
                {
                    XElement ele = extension.GetObject<XElement>();
                    if (ele.Name.LocalName == "encoded" && ele.Name.Namespace.ToString().Contains("content"))
                    {
                        sb.Append(ele.Value + "<br/>");
                    }
                }
                String subject11 = item.Title.Text;
            }   
        }

        //can 1 mang lon de kiem tra bai da dang hay chua
        void readRss()
        {
            //readRssPolitico("https://www.politico.com/news/2020/06/02/rod-rosenstein-senate-russia-295155");
            string[] linkArrayCongress = new string[15];
            string[] linkArrayHealcare = new string[15];
            string[] linkArrayDefense = new string[15];
            string[] linkArrayEconomy = new string[15];
            string[] linkArrayEnergy = new string[15];
            string[] linkArrayPolotics = new string[15];
            string[] linkArraywashington = new string[15];
            for (int i = 0; i < numOfNews; i++)
            {
                int type = (i + 1) % numOfNews;
                int index = 0;
                XmlReader reader = XmlReader.Create(url[i]);
                SyndicationFeed feed = SyndicationFeed.Load(reader);
                reader.Close();
                foreach (SyndicationItem item in feed.Items)
                {
                    if(item.Links !=null && item.Links.Count>0 && item.Links[0].Uri !=null && item.Links[0].Uri.AbsoluteUri != null)
                    {
                        String linkAdd = item.Links[0].Uri.AbsoluteUri;
                        switch (type)
                        {
                            case 1:
                                linkArrayCongress[index++] = linkAdd;
                                break;
                            case 2:
                                linkArrayHealcare[index++] = linkAdd;
                                break;
                            case 3:
                                linkArrayDefense[index++] = linkAdd;
                                break;
                            case 4:
                                linkArrayEconomy[index++] = linkAdd;
                                break;
                            case 5:
                                linkArrayEnergy[index++] = linkAdd;
                                break;
                            case 6:
                                linkArrayPolotics[index++] = linkAdd;
                                break;
                            case 0:
                                linkArraywashington[index++] = linkAdd;
                                break;
                        }
                        if (index > 14)
                        {
                            break;
                        }
                    }
                    //kiem tra da co chua, neu chua thi add vao mang
                    
                }
            }

            //tao mang bai bao hom nay
            //linkArrayLink
            int length = 15;
            var chooseArr = 0;
            for (int i = 0; i < length; i++)
            {
                if (i < 10)
                {
                    addNews(linkArrayCongress[i], chooseArr++);
                    addNews(linkArrayHealcare[i], chooseArr++);
                    addNews(linkArrayDefense[i], chooseArr++);
                    addNews(linkArrayEconomy[i], chooseArr++);
                    addNews(linkArrayEnergy[i], chooseArr++);
                    addNews(linkArrayPolotics[i], chooseArr++);
                    addNews(linkArraywashington[i], chooseArr++);
                }
                else
                {
                    addNews(linkArraywashington[i], chooseArr++);
                }
                /*
                linkArrayCongress
                linkArrayHealcare
                linkArrayDefense
                linkArrayEconomy
                linkArrayEnergy
                linkArrayPolotics
                linkArraywashington
                */
                //check da co trong mang cua 7 tuan chua
                


            }
            Console.Write("get news done");
        }

        void addNews(string arr, int index)
        {
            Boolean check = true;
            for (int k = 0; k < 300&&k< linkArrayLinkBuff3day.Length; k++)
            {
                if (linkArrayLinkBuff3day[k] == arr)
                {
                    check = false;
                    break;
                }
            }
            if (check)
            {
                linkArrayLink[index] = arr;
            }
        }

        private void GetInformation_Click(object sender, EventArgs e)
        {
            getParseNews(link.Text);
        }

        private void btExport_Click(object sender, EventArgs e)
        {
            webPath1 = "https://api.naturalreaders.com/v" + ver.Value + "/tts/macspeak?" + "&apikey=" + txtApi.Text + "&t=";
            lbTimeEnd.Text = "--";
            //reset param
            lbstatus.Text = "Bắt đầu";
            webPath2 = "&r=" + numGiong.Value;
            String tileGetVoid = txTieude.Text.Replace(" — ", " - ");
            videoTitle = tileGetVoid;
            String txLinkIput = txLink.Text;
            txInputText = rtInput.Text.Replace("&", " and ").Replace(".)", ").").Replace(" — ", " - ");
            getDataFromWeb(tileGetVoid, txLinkIput, txInputText);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            txLink.Text = "";
            txTieude.Text = "";
            rtInput.Text = "";
            lbstatus.Text = "wating....";
            link.Text = "";
        }

        private void cptitle_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(txTieude.Text);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            String textTemp = "";
            String[] textGet = rtInput.Text.Split(' ');
            for (int i = 0; (i < textGet.Length && i < 150); i++)
                textTemp += " " + textGet[i];
            Clipboard.SetText(textTemp + "...");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(tag.Text);
        }
        private void getParseNews(string textLink)
        {
            //String textLink = link.Text;
            String linkGet = "http://newspaper-demo.herokuapp.com/articles/show?url_to_clean=" + textLink + "&ie=UTF-8";
            using (System.Net.WebClient client = new WebClient())
            {
                var htmlData = client.DownloadData(linkGet);
                MemoryStream ms = new MemoryStream(htmlData);
                var htmlCode = Encoding.UTF8.GetString(htmlData);
                String path = System.AppDomain.CurrentDomain.BaseDirectory + "/temp.html";
                File.WriteAllText(path, htmlCode);

                HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
                htmlDoc.Load(ms, Encoding.UTF8);
                //htmlDoc.Load(htmlCode);
                // There are various options, set as needed
                htmlDoc.OptionFixNestedTags = true;

                // filePath is a path to a file containing the html
                //htmlDoc.Load(path);

                if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Count() > 0)
                {
                    // Handle any parse errors as required

                }
                else
                {

                    if (htmlDoc.DocumentNode != null)
                    {
                        News news = new News();
                        int indexpro = 1;
                        foreach (HtmlNode node in htmlDoc.DocumentNode.SelectNodes("//tr"))
                        {
                            String tempText = WebUtility.HtmlDecode(node.InnerText.Trim());
                            switch (indexpro)
                            {
                                case 1:
                                    news.Title = tempText.Substring(6).Trim();
                                    break;
                                case 2:
                                    news.Authors = tempText.Length > 8 ? tempText.Substring(8).Trim() : tempText;
                                    break;
                                case 3:
                                    news.Text = tempText.Length > 5 ? tempText.Substring(5).Trim() : tempText;
                                    break;
                                case 4:
                                    news.UrlImage = tempText.Length > 15 ? tempText.Substring(15).Trim() : tempText;
                                    break;
                                case 6:
                                    news.Tag = tempText.Length > 15 ? tempText.Substring(15).Trim() : tempText;
                                    break;
                            }
                            indexpro++;
                            if (indexpro > 6)
                            {
                                break;
                            }
                        }
                        if (news.Text.Length > 0)
                        {
                            txLink.Text = news.UrlImage;
                            txTieude.Text = news.Title;
                            rtInput.Text = news.Text;
                            tag.Text = news.Tag;
                        }
                    }
                }

            }
        }

        void getDataFromWeb(String tileGetVoid, String txLinkIput, String txInput)
        {
            try
            {
                //xoaClipTam();
                lbstatus.Text = "Lấy tiêu đề";
                //String tileGetVoid = txTieude.Text;
                textTile = txTieude.Text.Replace(":", " - ").Replace("'", "'\\\\\\''").Replace("\"", @"\""").Replace("\n", " ");
                string[] terms = textTile.Trim().Split(' ');
                int indexGet = 0;
                if (textTile.Length <= numOfLineHeader)
                {
                    numline = new String[1];
                    numline[0] = textTile;
                    numlinecount = 1;
                }
                else if (textTile.Length <= numOfLineHeader * 2)
                {
                    numlinecount = 2;
                    numline = new String[2];
                    String text1 = textTile.Substring(0, numOfLineHeader);
                    String text2 = textTile.Substring(numOfLineHeader);
                    int numberspace = text1.Trim().Split(' ').Length;
                    for (int i = 0; i < numberspace; i++)
                    {
                        numline[0] += terms[indexGet] + " ";
                        indexGet++;
                    }
                    numberspace = text2.Trim().Split(' ').Length;
                    for (int i = 0; i < numberspace; i++)
                    {
                        if (indexGet < terms.Length)
                        {
                            numline[1] += terms[indexGet] + " ";
                            indexGet++;
                        }
                    }

                }
                else
                {
                    numlinecount = 3;
                    numline = new String[3];

                    String text1 = textTile.Substring(0, numOfLineHeader);
                    String text2 = textTile.Substring(numOfLineHeader, numOfLineHeader);
                    String text3 = textTile.Substring(numOfLineHeader);

                    int numberspace = text1.Trim().Split(' ').Length;
                    for (int i = 0; i < numberspace; i++)
                    {
                        numline[0] += terms[i] + " ";
                        indexGet++;
                    }
                    numberspace = text2.Trim().Split(' ').Length;
                    for (int i = 0; i < numberspace; i++)
                    {
                        if (indexGet < terms.Length)
                        {
                            numline[1] += terms[indexGet] + " ";
                            indexGet++;
                        }
                    }
                    numberspace = text3.Trim().Split(' ').Length;
                    for (int i = 0; i < numberspace; i++)
                    {
                        if (indexGet < terms.Length)
                        {
                            numline[2] += terms[indexGet] + " ";
                            indexGet++;
                        }
                    }

                    //numline[0] = textTile.Substring(0, 40);
                    //numline[1] = textTile.Substring(40, 40);
                    //numline[2] = textTile.Substring(40);
                }
                //xoa du lieu cu
                if (File.Exists(finalVideo))
                {
                    try
                    {
                        File.Delete(finalVideo);
                    }
                    catch
                    {
                        return;
                    }
                }
                //String txLinkIput = txLink.Text;
                //xu ly lay anh
                //String txLinkIput = "https://i.ytimg.com/vi/E4LkTxs-5LI/maxresdefault.jpg";
                //FileStream fileStreamimage;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls;
                //exception image input
                HttpWebRequest requestIm;
                try
                {
                    requestIm = (HttpWebRequest)WebRequest.Create(txLinkIput);
                }
                catch (Exception ex)
                {
                    requestIm = (HttpWebRequest)WebRequest.Create(tempLinkImg);
                }

                requestIm.UserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.106 Safari/537.36";
                HttpWebResponse responseIm = (HttpWebResponse)requestIm.GetResponse();
                Stream receiveStreamImg = responseIm.GetResponseStream();
                byte[] bufferim = new byte[32768];
                Image tempim = ResizeImage(receiveStreamImg, 1280);
                if (tempim != null)
                {
                    var tempIm = new Bitmap(tempim);
                    tempIm.Save(pathImage, ImageFormat.Jpeg);
                }
                /*
                using (fileStreamimage = File.Create(pathImage))
                {
                    while (true)
                    {
                        int read = receiveStreamImg.Read(bufferim, 0, bufferim.Length);
                        if (read <= 0)
                        {
                            //MessageBox.Show("Lay hinh xong" + txLinkIput, "Thông báo nhẹ");
                            break;
                        }

                        fileStreamimage.Write(bufferim, 0, read);
                    }
                }*/

                //xu ly lay text
                String outTest = (txInput + ". Thank you listen! please subscribe to check news. Please turn on subtitle if you can't hear voice clearly. I'm Adam.").Replace("&", " and ").Replace(".)", ").");
                //String outTest = "NEWSTrey Gowdy Asks The One Question Hillary And Obama Are Terrified To Answer.";


                //xu li chen video tieu de
                FileStream fileStreamTD;
                String urlTieude = webPath1 + tileGetVoid + webPath2;
                HttpWebRequest requestTieuDe = (HttpWebRequest)WebRequest.Create(urlTieude);
                HttpWebResponse responseTieuDe = (HttpWebResponse)requestTieuDe.GetResponse();
                Stream receiveStreamTieuDe = responseTieuDe.GetResponseStream();
                byte[] bufferTieuDe = new byte[32768];

                using (fileStreamTD = File.Create(pathTieuDeMp3))
                {
                    while (true)
                    {
                        int read = receiveStreamTieuDe.Read(bufferTieuDe, 0, bufferTieuDe.Length);
                        if (read <= 0)
                        {
                            break;
                        }
                        fileStreamTD.Write(bufferTieuDe, 0, read);
                    }
                }
            }catch(Exception ext)
            {
                return;
            }
            //ghi File txt Script
            /*
            DateTime now = DateTime.Now;
            String pathDesk = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + indexNewsNow + "Of80" + ".txt";
            String textWrite = tag.Text + ".\n" + tileGetVoid + ".\n" + outTest;
            FileStream fs = null;
            try
            {
                fs = new FileStream(pathDesk, FileMode.CreateNew);
                using (StreamWriter writer = new StreamWriter(fs, Encoding.UTF8, 512))
                {
                    writer.Write(textWrite);
                }
            }
            finally
            {
                if (fs != null)
                    fs.Dispose();
            }
            */

            ///////////////////////////////////////
            // xu li lay mp3 bai bao
            /*
            FileStream fileStream = null;
            if (outTest.Length < 4200)
            {
                String url = webPath1 + outTest + webPath2;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                // Get the stream associated with the response.
                Stream receiveStream = response.GetResponseStream();
                byte[] buffer = new byte[32768];

                using (fileStream = File.Create(pathMp3))
                {
                    while (true)
                    {
                        int read = receiveStream.Read(buffer, 0, buffer.Length);
                        if (read <= 0)
                        {
                            break;
                        }

                        fileStream.Write(buffer, 0, read);
                    }
                }
            }
            else
            {
                String url1 = webPath1 + outTest.Substring(0, 4200) + webPath2;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url1);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                // Get the stream associated with the response.
                Stream receiveStream = response.GetResponseStream();
                byte[] buffer = new byte[32768];

                ////////////2222
                String url2 = webPath1 + outTest.Substring(4200) + webPath2;
                HttpWebRequest request1 = (HttpWebRequest)WebRequest.Create(url2);
                HttpWebResponse response1 = (HttpWebResponse)request1.GetResponse();
                // Get the stream associated with the response.
                Stream receiveStream1 = response1.GetResponseStream();
                byte[] buffer1 = new byte[32768];

                using (fileStream = File.Create(pathMp3))
                {
                    while (true)
                    {
                        int read = receiveStream.Read(buffer, 0, buffer.Length);
                        if (read <= 0)
                        {
                            int read2 = receiveStream1.Read(buffer1, 0, buffer.Length);
                            if (read2 <= 0)
                            {
                                break;
                            }
                            fileStream.Write(buffer1, 0, read2);
                        }

                        fileStream.Write(buffer, 0, read);
                    }
                }
            }
            */

            //if (fileStream != null)
            //{


            //process.StartInfo.FileName = pathRun;



            // process.StartInfo.Arguments = "-loop 1 -i IMG.jpg -i MP3.mp3 -c:v libx264 -c:a copy -shortest outVideo.mp4";

            try
            {
                lbstatus.Text = "tieudechuatext";
                String createVideoTieuDe = "-loop 1 -i " + pathImage + " -i " + pathTieuDeMp3 + " -c:v libx264 -s 1280x720 -c:a copy -shortest tieudechuatext.mp4";
                //String addLogo = "ffmpeg -i outVideo.mp4 -i logo.png -filter_complex overlay=main_w-overlay_w-5:5 -codec:a copy outVideo1.mp4";
                process = new Process();
                ProcessStartInfo info = new ProcessStartInfo(pathRun, createVideoTieuDe);
                info.CreateNoWindow = true;
                info.UseShellExecute = false;
                process.StartInfo = info;
                process.EnableRaisingEvents = true;
                process.Exited += new EventHandler(process_Exited1);
                process.Start();


            }
            catch
            {
                if (process != null) process.Dispose();
                return;
            }
            //}
        }
        public Image ResizeImage(Stream image, int width)
        {
            try
            {
                using (Image fromStream = Image.FromStream(image))
                {
                    // calculate height based on the width parameter
                    int newHeight = (int)(fromStream.Height / ((double)fromStream.Width / width));

                    using (Bitmap resizedImg = new Bitmap(fromStream, width, newHeight))
                    {
                        using (MemoryStream stream = new MemoryStream())
                        {
                            resizedImg.Save(stream, fromStream.RawFormat);
                            return Image.FromStream(stream);
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                // log error
            }

            return null;
        }
        void process_Exited1(object sender, EventArgs e)
        {
            // chen tua de len clip
            if (lbstatus.InvokeRequired)
            {
                lbstatus.Invoke(new MethodInvoker(delegate {; lbstatus.Text = "chen tua de len clips"; }));
            }
            process.Dispose();
            String codeRender = "";
            //string[] lines = System.IO.File.ReadAllLines(pathInserttextToVideo);
            if (numlinecount == 1)
            {
                codeRender = "-i " + pathTieuDeChuaText + @" -filter:v ""drawtext=box=1:fontfile=" + font + ":text ='" + numline[0] + "':fontcolor=white: fontsize=" + fontSize + @": boxcolor=black@0.5:boxborderw=5:x=(w-text_w)/2:y=300"" -codec:a copy tieude.mp4";
            }

            if (numlinecount == 2)
            {
                codeRender = "-i " + pathTieuDeChuaText + @" -filter:v ""[in]drawtext=box=1:fontfile=" + font + ":text ='" + numline[0] + "':fontcolor=white: fontsize=" + fontSize + @": boxcolor=black@0.5:boxborderw=5:x=(w-text_w)/2.5:y=260";
                codeRender += ", drawtext=box=1:fontfile=" + font + ":text ='" + numline[1] + "':fontcolor=white: fontsize=" + fontSize + @": boxcolor=black@0.5:boxborderw=5:x=(w-text_w)/2:y=350[out]"" -codec:a copy tieude.mp4";
            }

            if (numlinecount == 3)
            {
                codeRender = "-i " + pathTieuDeChuaText + @" -filter:v ""[in]drawtext=box=1:fontfile=" + font + ":text ='" + numline[0] + "':fontcolor=white: fontsize=" + fontSize + @": boxcolor=black@0.5:boxborderw=5:x=(w-text_w)/2.5:y=210";
                codeRender += ",drawtext=box=1:fontfile=" + font + ":text ='" + numline[1] + "':fontcolor=white: fontsize=" + fontSize + @": boxcolor=black@0.5:boxborderw=5:x=(w-text_w)/2:y=300";
                codeRender += ",drawtext=box=1:fontfile=" + font + ":text ='" + numline[2] + "':fontcolor=white: fontsize=" + fontSize + @": boxcolor=black@0.5:boxborderw=5:x=(w-text_w)/2:y=390[out]"" -codec:a copy tieude.mp4";
            }

            //String codeRender = String.Format(lines[0],pathTieuDeChuaText, textTile);
            // String createVideo = "-loop 1 -i " + pathImage + " -i " + pathMp3 + " -c:v libx264 -s 1280x720 -c:a copy -shortest outVideo.mp4";
            //String addogo2 = String.Format(" -i "+ pathVideo + " -i logo.png -filter_complex [0:v][1:v]overlay = 10:10 -codec:a copy outVideoKassss.mp4");
            process = new Process();
            ProcessStartInfo info = new ProcessStartInfo(pathRun, codeRender);

            info.CreateNoWindow = true;
            info.UseShellExecute = false;
            info.RedirectStandardOutput = true;

            process.StartInfo = info;

            process.EnableRaisingEvents = true;

            process.Exited += new EventHandler(process_Exited2);
            process.Start();
        }
        void process_Exited2(object sender, EventArgs e)
        {
            // chen tua de len clip
            if (lbstatus.InvokeRequired)
            {
                lbstatus.Invoke(new MethodInvoker(delegate {; lbstatus.Text = "chen tua de len clips"; }));
            }
            process.Dispose();
            FileStream fileStream = null;
            String outTest = (txInputText + ". Thank you listen! please subscribe to check news. Please turn on subtitle if you can't hear voice clearly. I'm Ava.").Replace("&", " and ").Replace(".)", ").");
            int indextCut = getIndextCut(outTest, lengtSplit);
            if (indextCut ==0)
            {
                String url = webPath1 + outTest + webPath2;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.ReadWriteTimeout = 100000;
                //request.Timeout =30000;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                // Get the stream associated with the response.
                Stream receiveStream = response.GetResponseStream();
                byte[] buffer = new byte[32768];

                using (fileStream = File.Create(pathMp3))
                {
                    while (true)
                    {
                        int read = receiveStream.Read(buffer, 0, buffer.Length);
                        if (read <= 0)
                        {
                            break;
                        }

                        fileStream.Write(buffer, 0, read);
                    }
                }
            }
            else if(indextCut < lengtSplit&& outTest.Substring(lengtSplit).Length <= lengtSplit)
            {
                String url1 = webPath1 + outTest.Substring(0, indextCut) + webPath2;
                //xu li cat chu
                
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url1);
                request.ReadWriteTimeout = 100000;
                //request.Timeout = 30000;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                // Get the stream associated with the response.
                Stream receiveStream = response.GetResponseStream();
                byte[] buffer = new byte[32768];

                ////////////2222
                String url2 = webPath1 + outTest.Substring(indextCut) + webPath2;
                HttpWebRequest request1 = (HttpWebRequest)WebRequest.Create(url2);
                request1.ReadWriteTimeout = 100000;
                //request1.Timeout = 30000;
                HttpWebResponse response1 = (HttpWebResponse)request1.GetResponse();
                // Get the stream associated with the response.
                Stream receiveStream1 = response1.GetResponseStream();
                byte[] buffer1 = new byte[32768];

                using (fileStream = File.Create(pathMp3))
                {
                    while (true)
                    {
                        int read = receiveStream.Read(buffer, 0, buffer.Length);
                        if (read <= 0)
                        {
                            int read2 = receiveStream1.Read(buffer1, 0, buffer.Length);
                            if (read2 <= 0)
                            {
                                break;
                            }
                            fileStream.Write(buffer1, 0, read2);
                        }

                        fileStream.Write(buffer, 0, read);
                    }
                }
            }
            else
            {
                String url1 = webPath1 + outTest.Substring(0, indextCut) + webPath2;
                //xu li cat chu

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url1);
                request.ReadWriteTimeout = 100000;
                //request.Timeout = 30000;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                // Get the stream associated with the response.
                Stream receiveStream = response.GetResponseStream();
                byte[] buffer = new byte[32768];

                ////////////2222
                int indextCut1 = getIndextCut(outTest.Substring(indextCut), lengtSplit);
                String url2 = webPath1 + outTest.Substring(indextCut, indextCut1) + webPath2;
                HttpWebRequest request1 = (HttpWebRequest)WebRequest.Create(url2);
                request1.ReadWriteTimeout = 100000;
                //request1.Timeout = 30000;
                HttpWebResponse response1 = (HttpWebResponse)request1.GetResponse();
                // Get the stream associated with the response.
                Stream receiveStream1 = response1.GetResponseStream();
                byte[] buffer1 = new byte[32768];
                /////////3
                String url3 = webPath1 + outTest.Substring(indextCut1) + webPath2;
                HttpWebRequest request2 = (HttpWebRequest)WebRequest.Create(url2);
                request2.ReadWriteTimeout = 100000;
                //request1.Timeout = 30000;
                HttpWebResponse response2 = (HttpWebResponse)request1.GetResponse();
                // Get the stream associated with the response.
                Stream receiveStream2 = response1.GetResponseStream();
                byte[] buffer2 = new byte[32768];

                using (fileStream = File.Create(pathMp3))
                {
                    while (true)
                    {
                        int read = receiveStream.Read(buffer, 0, buffer.Length);
                        if (read <= 0)
                        {
                            int read2 = receiveStream1.Read(buffer1, 0, buffer1.Length);
                            if (read2 <= 0)
                            {
                                int read3 = receiveStream2.Read(buffer2, 0, buffer2.Length);
                                if (read3 <= 0)
                                {
                                    break;
                                }
                                fileStream.Write(buffer2, 0, read2);
                            }
                            fileStream.Write(buffer1, 0, read2);
                        }

                        fileStream.Write(buffer, 0, read);
                    }
                }
            }

            //tao video
            String codeRender = "-i " + pathImage + " -i " + pathLogo + " -filter_complex overlay=main_w-overlay_w-10:10 -codec:a copy IMGLG.Jpeg";
            //String createVideo = "-loop 1 -i " + pathImage + " -i " + pathMp3 + " -c:v libx264 -s 1280x720 -c:a copy -shortest outVideo.mp4";
            //String addogo2 = String.Format(" -i "+ pathVideo + " -i logo.png -filter_complex [0:v][1:v]overlay = 10:10 -codec:a copy outVideoKassss.mp4");
            process = new Process();
            ProcessStartInfo info = new ProcessStartInfo(pathRun, codeRender);

            info.CreateNoWindow = true;
            info.UseShellExecute = false;
            //info.RedirectStandardError = true;
            info.RedirectStandardOutput = true;

            process.StartInfo = info;

            process.EnableRaisingEvents = true;
            process.Exited += new EventHandler(process_Exited2_1);
            process.Start();
        }

        int getIndextCut(string outTest, int indextNew)
        {
            if(outTest.Length < lengtSplit)
            {
                return 0;
            }
            char checkSpace = outTest.ElementAt(indextNew);
            if (checkSpace == ' ')
            {
                return indextNew;
            }
            else
            {
                return getIndextCut(outTest, indextNew - 1);
            }
        }
        void process_Exited2_1(object sender, EventArgs e)
        {
            // ghep mp3 va image
            String createVideo = "-loop 1 -i " + pathImageLogo + " -i " + pathMp3 + " -c:v libx264 -s 1280x720 -c:a copy -shortest outVideo.mp4";
            //String addogo2 = String.Format(" -i "+ pathVideo + " -i logo.png -filter_complex [0:v][1:v]overlay = 10:10 -codec:a copy outVideoKassss.mp4");
            process = new Process();
            ProcessStartInfo info = new ProcessStartInfo(pathRun, createVideo);

            info.CreateNoWindow = true;
            info.UseShellExecute = false;
            //info.RedirectStandardError = true;
            info.RedirectStandardOutput = true;

            process.StartInfo = info;

            process.EnableRaisingEvents = true;
            process.Exited += new EventHandler(process_Exited3);
            process.Start();
        }
        void process_Exited3(object sender, EventArgs e)
        {
            //them text neu can
            if (lbstatus.InvokeRequired)
            {
                lbstatus.Invoke(new MethodInvoker(delegate {; lbstatus.Text = "merge tong intro voi clip tua de "; }));
            }
            process.Dispose();
            string[] lines = System.IO.File.ReadAllLines(mergeIntro2video);
            String codeRender = String.Format(lines[0], pathIntro, pathTieuDe, "finalVideoTemp.mp4");//, 
            //String mergeIntro = "-y -i " + pathIntro + " -i " + pathTieuDe + " -i " + pathVideoLogo + @" -filter_complex ""[1:v]scale=1280:720,setdar=16/9 [vmain]; [1:a]volume=1.6 [amain]; [0:v]scale=1280:720,setdar=16/9 [vintro]; [2:v]scale=1280:720 [voutro]; [vintro][0:a][vmain][amain][voutro][2:a]concat=n=3:v=1:a=1"" -vcodec libx264 -pix_fmt yuv420p -r 30 -g 60 -b:v 1400k -profile:v main -level 3.1 -acodec libmp3lame -b:a 128k -ar 44100 -bufsize 500000k -preset superfast finalVideo.mp4";
            process = new Process();
            ProcessStartInfo info = new ProcessStartInfo(pathRun, codeRender);


            info.CreateNoWindow = true;
            info.UseShellExecute = false;
            info.RedirectStandardOutput = true;
            process.StartInfo = info;
            process.EnableRaisingEvents = true;
            process.Exited += new EventHandler(process_Exited3_1);
            process.Start();
        }
        void process_Exited3_1(object sender, EventArgs e)
        {
            //them text neu can
            xoaMain();
            if (lbstatus.InvokeRequired)
            {
                lbstatus.Invoke(new MethodInvoker(delegate {; lbstatus.Text = "merge tong intro voi clip tua de "; }));
            }
            process.Dispose();
            string[] lines = System.IO.File.ReadAllLines(mergeIntro2video);
            String codeRender = String.Format(lines[0], finalVideoTemp, outVideo, "finalVideo.mp4");//, 
            //String mergeIntro = "-y -i " + pathIntro + " -i " + pathTieuDe + " -i " + pathVideoLogo + @" -filter_complex ""[1:v]scale=1280:720,setdar=16/9 [vmain]; [1:a]volume=1.6 [amain]; [0:v]scale=1280:720,setdar=16/9 [vintro]; [2:v]scale=1280:720 [voutro]; [vintro][0:a][vmain][amain][voutro][2:a]concat=n=3:v=1:a=1"" -vcodec libx264 -pix_fmt yuv420p -r 30 -g 60 -b:v 1400k -profile:v main -level 3.1 -acodec libmp3lame -b:a 128k -ar 44100 -bufsize 500000k -preset superfast finalVideo.mp4";
            process = new Process();
            ProcessStartInfo info = new ProcessStartInfo(pathRun, codeRender);


            info.CreateNoWindow = true;
            info.UseShellExecute = false;
            info.RedirectStandardOutput = true;
            process.StartInfo = info;
            process.EnableRaisingEvents = true;
            process.Exited += new EventHandler(process_Exited4);
            process.Start();
        }
        void process_Exited4(object sender, EventArgs e)
        {
            process.Dispose();
            //tao thumb
            String codeRender = " -i " + pathImageLogo + @" -filter:v ""[in]";//
            //dem so chu tile
            string[] terms = textTile.Trim().Split(' ');
            int numWord = 4;
            int countLine= terms.Length/ numWord;
            int sodu = terms.Length % numWord;
            if (sodu != 0)
                countLine++;
            string tempStrS = "drawtext=box=0:fontfile=" + font + ":text ='";
            string tempStrE = "':fontcolor=white:shadowcolor=blue:shadowx=5:shadowy=10:fontsize=84:borderw=7:x=(w-text_w)/2:y=(h-text_h-line_h)/";
            switch (countLine)
            {
                case 1:
                    string line = textTile.Trim();
                    codeRender += tempStrS + line + tempStrE + "2";
                    break;
                case 2:
                    string line1 = "";
                    for(int i = 0; i < numWord; i++)
                    {
                        line1 += terms[i] + " ";
                    }
                    string line2 = "";
                    for (int i = numWord; i < terms.Length; i++)
                    {
                        line2 += terms[i] + " ";
                    }
                    codeRender += tempStrS + line1 + tempStrE + "2.3";
                    codeRender += "," + tempStrS + line2 + tempStrE + "1.7";

                    break;
                case 3:
                    line1 = "";
                    line2 = "";
                    string line3 = "";
                    for (int i = 0; i < numWord; i++)
                    {
                        line1 += terms[i] + " ";
                    }
                    for (int i = numWord; i < numWord*2; i++)
                    {
                        line2 += terms[i] + " ";
                    }
                    for (int i = numWord * 2; i < terms.Length; i++)
                    {
                        line3 += terms[i] + " ";
                    }
                    codeRender += tempStrS + line1 + tempStrE + "2.9";
                    codeRender += "," + tempStrS + line2 + tempStrE + "2";
                    codeRender += "," + tempStrS + line3 + tempStrE + "1.5";

                    break;
                case 4:
                    line1 = "";
                    line2 = "";
                    line3 = "";
                    string line4 = "";
                    for (int i = 0; i < numWord; i++)
                    {
                        line1 += terms[i] + " ";
                    }
                    for (int i = numWord; i < numWord * 2; i++)
                    {
                        line2 += terms[i] + " ";
                    }
                    for (int i = numWord*2; i < numWord * 3; i++)
                    {
                        line3 += terms[i] + " ";
                    }
                    for (int i = numWord * 3; i < terms.Length; i++)
                    {
                        line4 += terms[i] + " ";
                    }

                    codeRender += tempStrS + line1 + tempStrE + "3.4";
                    codeRender += "," + tempStrS + line2 + tempStrE + "2.3";
                    codeRender += "," + tempStrS + line3 + tempStrE + "1.7";
                    codeRender += "," + tempStrS + line4 + tempStrE + "1.35";
                    break;
                case 5:
                    line1 = "";
                    line2 = "";
                    line3 = "";
                    line4 = "";
                    string line5 = "";
                    for (int i = 0; i < numWord; i++)
                    {
                        line1 += terms[i] + " ";
                    }
                    for (int i = numWord; i < numWord * 2; i++)
                    {
                        line2 += terms[i] + " ";
                    }
                    for (int i = numWord * 2; i < numWord * 3; i++)
                    {
                        line3 += terms[i] + " ";
                    }
                    for (int i = numWord * 3; i < numWord * 4; i++)
                    {
                        line4 += terms[i] + " ";
                    }
                    for (int i = numWord * 4; i < terms.Length; i++)
                    {
                        line5 += terms[i] + " ";
                    }

                    codeRender += tempStrS + line1 + tempStrE + "4.8";
                    codeRender += "," + tempStrS + line2 + tempStrE + "2.9";
                    codeRender += "," + tempStrS + line3 + tempStrE + "2";
                    codeRender += "," + tempStrS + line4 + tempStrE + "1.5";
                    codeRender += "," + tempStrS + line5 + tempStrE + "1.2";
                    break;
                case 6:
                    line1 = "";
                    line2 = "";
                    line3 = "";
                    line4 = "";
                    line5 = "";
                    string line6 = "";
                    for (int i = 0; i < numWord; i++)
                    {
                        line1 += terms[i] + " ";
                    }
                    for (int i = numWord; i < numWord * 2; i++)
                    {
                        line2 += terms[i] + " ";
                    }
                    for (int i = numWord * 2; i < numWord * 3; i++)
                    {
                        line3 += terms[i] + " ";
                    }
                    for (int i = numWord * 3; i < numWord * 4; i++)
                    {
                        line4 += terms[i] + " ";
                    }
                    for (int i = numWord * 4; i < numWord * 5; i++)
                    {
                        line5 += terms[i] + " ";
                    }
                    for (int i = numWord * 5; i < terms.Length; i++)
                    {
                        line6 += terms[i] + " ";
                    }
                    codeRender += tempStrS + line1 + tempStrE + "6.3";
                    codeRender += "," + tempStrS + line4 + tempStrE + "3.4";
                    codeRender += "," + tempStrS + line2 + tempStrE + "2.3";
                    codeRender += "," + tempStrS + line3 + tempStrE + "1.7";
                    codeRender += "," + tempStrS + line4 + tempStrE + "1.35";
                    codeRender += "," + tempStrS + line4 + tempStrE + "1.1";
                    break;
            }

            codeRender += @"[out]"" -codec:a copy thumb.Jpeg";

            process = new Process();
            ProcessStartInfo info = new ProcessStartInfo(pathRun, codeRender);


            info.CreateNoWindow = true;
            info.UseShellExecute = false;
            info.RedirectStandardOutput = true;
            process.StartInfo = info;
            process.EnableRaisingEvents = true;
            process.Exited += new EventHandler(process_Exited5);
            process.Start();
           
        }
        void process_Exited5(object sender, EventArgs e)
        {
            xoaClipTam();
            if (File.Exists(finalVideo) && File.Exists(pathThumb))
            {
                try
                {
                    Thread a = new Thread(UpdaloadVideo);
                    a.Start();
                    /*
                    DateTime now = DateTime.Now;
                    String pathDesk = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Video.mp4";
                    String pathDeskThumb = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\thumb.Jpeg";
                    File.Copy(finalVideo, pathDesk);
                    File.Copy(pathThumb, pathDeskThumb);
                    File.Delete(finalVideo);
                    File.Delete(pathThumb);
                    */
                }
                catch
                {
                    return;
                }
            }
        }


        //upload bai bao
        void UpdaloadVideo()
        {
            printMes("Bat dau upload video");
            Console.WriteLine("==============================");

            try
            {
                string Title  = videoTitle;
                String textTemp = "";
                String[] textGet = txInputText.Split(' ');
                for (int i = 0; (i < textGet.Length && i < 150); i++)
                    textTemp += " " + textGet[i];
                string Description = textTemp;
                string[] tags = tag.Text.Split(',');
                /*
                string Pat = pathIntro;
                //string PathName= finalVideo;
                string Title = "Default Video Title";
                string Description = "Default Video Description";
                string[] tags = new string[] { "tag1", "tag2" };
                */
                RunUpload(Title, Description, tags, @"finalVideo.mp4").Wait();
            }
            catch (AggregateException ex)
            {
                foreach (var e in ex.InnerExceptions)
                {
                    Console.WriteLine("Error: " + e.Message);
                }
            }
        }
        private async Task RunUpload(string Title, string Description, string[] Tags,string PathName)
        {
            UserCredential credential;
            using (var stream = new FileStream("client_secrets.json", FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    // This OAuth 2.0 access scope allows an application to upload files to the
                    // authenticated user's YouTube channel, but doesn't allow other types of access.
                    new[] { YouTubeService.Scope.YoutubeUpload },
                    "user",
                    CancellationToken.None
                );
            }

            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = Assembly.GetExecutingAssembly().GetName().Name
            });
            //
            printMes("Set tham so");
            var video = new Video();
            video.Snippet = new VideoSnippet();
            video.Snippet.Title = Title;
            video.Snippet.Description = Description;
            video.Snippet.Tags = Tags;
            video.Snippet.CategoryId = "22"; // See https://developers.google.com/youtube/v3/docs/videoCategories/list
            video.Status = new VideoStatus();
            video.Status.PrivacyStatus = "public"; // or "private" or "public"
            var filePath = PathName; // Replace with path to actual movie file.

            using (var fileStream = new FileStream(filePath, FileMode.Open))
            {
                var videosInsertRequest = youtubeService.Videos.Insert(video, "snippet,status", fileStream, "video/*");
                videosInsertRequest.ProgressChanged += videosInsertRequest_ProgressChanged;
                videosInsertRequest.ResponseReceived += videosInsertRequest_ResponseReceived;

                await videosInsertRequest.UploadAsync();
            }
        }
        void videosInsertRequest_ProgressChanged(Google.Apis.Upload.IUploadProgress progress)
        {
            switch (progress.Status)
            {
                case UploadStatus.Uploading:
                    Console.WriteLine("{0} bytes sent.", progress.BytesSent);
                    break;

                case UploadStatus.Failed:
                    Console.WriteLine("An error prevented the upload from completing.\n{0}", progress.Exception);
                    break;
            }
        }

        void videosInsertRequest_ResponseReceived(Video video)
        {
            Console.WriteLine("------------------------ket thuc");
            printMes("Upload hoan thanh");
            //khong can thiet dong bo, 30p'moi goi tien trinh chay lai
            setTextToBlank();
            //bat dau upload thumb
            RunUploadThumb(video.Id);
            Console.WriteLine("Video id '{0}' was successfully uploaded.", video.Id);
        }

        private async Task RunUploadThumb(string videoId)
        {
            //khi upload thumb ghi nhan da dang clip
            linkArrayLinkBuff3day[indexSize++] = linkUpload;
            writeTextLink(linkUpload);
            UserCredential credential;
            using (var stream = new FileStream("client_secrets.json", FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    // This OAuth 2.0 access scope allows an application to upload files to the
                    // authenticated user's YouTube channel, but doesn't allow other types of access.
                    new[] { YouTubeService.Scope.YoutubeUpload },
                    "user",
                    CancellationToken.None
                );
            }

            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = Assembly.GetExecutingAssembly().GetName().Name
            });

            using (var tStream = new FileStream(pathThumb, FileMode.Open))
            {
                var tInsertRequest = youtubeService.Thumbnails.Set(videoId,  tStream, "image/jpeg");
                //tInsertRequest.ProgressChanged += ProgressChanged;
                await tInsertRequest.UploadAsync();
            }
            // xoa file chinh, thumb
            if (File.Exists(finalVideo) && File.Exists(pathThumb))
            {
                try
                {
                    File.Delete(finalVideo);
                    File.Delete(pathThumb);
                }
                catch
                {
                    return;
                }
            }
            if (lbstatus.InvokeRequired)
            {
                lbstatus.Invoke(new MethodInvoker(delegate {; lbstatus.Text = "Hoan thanh upload"; }));
            }

        }
        static void xoaMain()
        {
            if (File.Exists(finalVideo) && File.Exists(pathThumb))
            {
                try
                {
                    File.Delete(finalVideo);
                    File.Delete(pathThumb);
                }
                catch
                {
                    return;
                }
            }
        }
        static void xoaClipTam()
        {
            if (File.Exists(pathMp3))
            {
                try
                {
                    File.Delete(pathMp3);
                }
                catch
                {
                    return;
                }
            }
            if (File.Exists(pathImage))
            {
                try
                {
                    File.Delete(pathImage);
                }
                catch
                {
                    return;
                }
            }
            if (File.Exists(pathImageLogo))
            {
                try
                {
                    File.Delete(pathImageLogo);
                }
                catch
                {
                    return;
                }
            }
            if (File.Exists(pathTieuDeMp3))
            {
                try
                {
                    File.Delete(pathTieuDeMp3);
                }
                catch
                {
                    return;
                }
            }
            if (File.Exists(pathMp3))
            {
                try
                {
                    File.Delete(pathMp3);
                }
                catch
                {
                    return;
                }
            }
            if (File.Exists(pathTieuDeChuaText))
            {
                try
                {
                    File.Delete(pathTieuDeChuaText);
                }
                catch
                {
                    return;
                }
            }
            if (File.Exists(pathTieuDe))
            {
                try
                {
                    File.Delete(pathTieuDe);
                }
                catch
                {
                    return;
                }
            }
            if (File.Exists(finalVideoTemp))
            {
                try
                {
                    File.Delete(finalVideoTemp);
                }
                catch
                {
                    return;
                }
            }
            if (File.Exists(outVideo))
            {
                try
                {
                    File.Delete(outVideo);
                }
                catch
                {
                    return;
                }
            }
        }

        private void txLink_TextChanged(object sender, EventArgs e)
        {

        }

        void printMes(string mess)
        {
            if (lbstatus.InvokeRequired)
            {
                lbstatus.Invoke(new MethodInvoker(delegate {; lbstatus.Text = mess; }));
            }
        }

        void setTextToBlank()
        {
            if (txLink.InvokeRequired)
            {
                txLink.Invoke(new MethodInvoker(delegate {; txLink.Text = ""; }));
            }

            if (txTieude.InvokeRequired)
            {
                txTieude.Invoke(new MethodInvoker(delegate {; txTieude.Text = ""; }));
            }

            if (rtInput.InvokeRequired)
            {
                rtInput.Invoke(new MethodInvoker(delegate {; rtInput.Text = ""; }));
            }
            if (lbstatus.InvokeRequired)
            {
                lbstatus.Invoke(new MethodInvoker(delegate {; lbstatus.Text = ""; }));
            }
            if (link.InvokeRequired)
            {
                link.Invoke(new MethodInvoker(delegate {; link.Text = ""; }));
            }
            if (tag.InvokeRequired)
            {
                tag.Invoke(new MethodInvoker(delegate {; tag.Text = ""; }));
            }
        }
    }
}
