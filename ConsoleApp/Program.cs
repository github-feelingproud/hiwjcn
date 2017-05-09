using Elasticsearch.Net;
using Fleck;
using Lib.core;
using Lib.extension;
using Lib.helper;
using Lib.io;
using Nest;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Threading;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var codeHelper = new DrawVerifyCode_xx();
            var path = "d:\\data_vin_bg_1";
            new DirectoryInfo(path).CreateIfNotExist();
            for (var i = 0; i < 500; ++i)
            {
                var p = Path.Combine(path, $"data_{i}");
                new DirectoryInfo(p).CreateIfNotExist();
                for (var j = 0; j < 1000; ++j)
                {
                    var (bs, with, height) = codeHelper.GetImageBytesAndSize();
                    var f = Path.Combine(p, $"{codeHelper.Code}_{Com.GetUUID()}.png");
                    using (var fs = new FileStream(f, FileMode.Create))
                    {
                        fs.Write(bs, 0, bs.Length);
                    }
                    Console.WriteLine(f);
                }
            }
            return;

            var server = new WebSocketServer("ws://0.0.0.0:8181");
            server.Start(socket =>
            {
                socket.OnOpen = () => { Console.WriteLine("Open"); };
                socket.OnClose = () => { Console.WriteLine("Close"); };
                socket.OnMessage = async msg => { await socket.Send(msg); };
            });
            Console.ReadLine();
        }

        private static readonly string indexName = "productlist";
        private static readonly string ES_SERVERS = "http://localhost:9200";

        private static readonly ConnectionSettings setting = new ConnectionSettings(new SniffingConnectionPool(ES_SERVERS.Split('|', ',').Select(x => new Uri(x))));

        private static void PrepareES(Func<ElasticClient, bool> func)
        {
            var client = new ElasticClient(setting);
            func.Invoke(client);
        }

        public class SunData
        {
            [Key]
            public virtual int IID { get; set; }
            public virtual string UID { get; set; }
            public virtual string Data { get; set; }
            public virtual string ImagePath { get; set; }
            public virtual string Labels { get; set; }
        }

        interface p<T>
        { }
        interface human
        { }
        class basep
        { }
        class pp : p<string>, human
        { }

        /// <summary>
        /// 索引磁盘上的文件
        /// </summary>
        private static void IndexFiles()
        {
            PrepareES(client =>
            {
                var indexName = nameof(DiskFileIndex).ToLower();
                try
                {
                    client.CreateIndexIfNotExists(indexName, x => x.GetCreateIndexDescriptor<DiskFileIndex>());
                }
                catch (Exception e)
                {
                    e.AddErrorLog();
                }

                var txtFiles = new string[] {
                    ".txt", ".cs", ".css", ".js", ".cshtml", ".html"
                    , ".readme", ".json", ".config", ".md" };

                var maxSize = Com.MbToB(1);

                Com.FindFiles("D:\\XXXXXXXXXXXXXXXXXXXX\\", (f) =>
                {
                    Thread.Sleep(100);
                    try
                    {
                        if (!txtFiles.Any(x => ConvertHelper.GetString(f.Name).ToLower().EndsWith(x))
                        || ConvertHelper.GetString(f.FullName).ToLower().Contains("elasticsearch")
                        || f.Length > maxSize)
                        {
                            Console.WriteLine($"跳过文件：{f.FullName}");
                            return;
                        }
                        var model = new DiskFileIndex();
                        model.Path = f.FullName;
                        model.Name = f.Name;
                        model.Extension = f.Extension;
                        model.Size = f.Length;
                        model.Content = File.ReadAllText(model.Path);
                        var data = new List<DiskFileIndex>() { model };
                        client.AddToIndex(indexName, data.ToArray());
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }, (c) =>
                {
                    Console.WriteLine($"剩余任务：{c}");
                    if (c > 5000) { Thread.Sleep(400); }
                });
                Console.ReadLine();
                return true;
            });
        }

    }


    /// <summary>
    ///用于生成多种样式的验证码
    /// </summary>
    public class DrawVerifyCode_xx
    {
        class CharItem
        {
            public string c { get; set; }
            public Font font { get; set; }
        }

        #region 各种参数
        private readonly Random random = new Random(DateTime.Now.Millisecond);

        /// <summary>
        /// 生成的code
        /// </summary>
        public string Code { get; private set; }

        /// <summary>
        /// 字体大小，【默认15px】为了使字符居中显示，请设置一个合适的值
        /// </summary>
        public int FontSize { get; set; } = 18;

        /// <summary>
        /// 验证码字符数
        /// </summary>
        public int CharCount { get; set; } = 17;

        private readonly List<Color> colors = new List<Color>() {
            Color.FromArgb(140,140,140),
            Color.FromArgb(160,160,160),

            Color.FromArgb(200, 200, 200),
            Color.FromArgb(210, 210, 210),
            Color.FromArgb(220, 220, 220),
            Color.FromArgb(230, 230, 230),
            Color.FromArgb(240, 240, 240),
        };
        private readonly List<Font> fonts = new List<Font>() { };
        private readonly List<char> chars;

        private readonly List<string> bgs = new List<string>();

        PrivateFontCollection pfc = new PrivateFontCollection();

        /// <summary>
        /// 构造函数，设置response对象
        /// </summary>
        public DrawVerifyCode_xx()
        {
            pfc.AddFontFile("d:\\xx.ttf");
            this.fonts = new List<Font>() { new Font(pfc.Families[0], FontSize) };

            var bg_dir = "d:\\data_bgs\\";
            this.bgs = Directory.GetFiles(bg_dir, "*.png").Select(x => Path.Combine(bg_dir, x)).ToList();

            var chars = new List<char>();
            Action<char, char> FindChar = (start, end) =>
            {
                for (var i = start; i <= end; ++i)
                {
                    chars.Add((char)i);
                }
            };
            FindChar('A', 'Z');
            FindChar('0', '9');
            this.chars = chars;

            this.Code = string.Empty;
        }

        #endregion

        #region 主体程序
        /// <summary>
        /// 获取图片bytes
        /// </summary>
        /// <returns></returns>
        public byte[] GetImageBytes()
        {
            return GetImageBytesAndSize().bs;
        }
        /// <summary>
        /// 获取图片bytes和宽高
        /// </summary>
        /// <returns></returns>
        public (byte[] bs, int width, int height) GetImageBytesAndSize()
        {
            if (CharCount <= 0) { throw new Exception("字符数必须大于0"); }
            this.Code = string.Empty;

            var items = new int[CharCount].Select(_ => new CharItem()
            {
                c = random.Choice(chars).ToString(),
                font = new Font(pfc.Families[0], FontSize)
            }).ToList();
            int Height = (int)(items.Select(x => x.font).Max(x => x.Height) * 1.3);
            int Width = (int)(Height * 0.8 * CharCount);
            //获取随机字体，颜色
            using (var bm = new Bitmap(Image.FromFile(random.Choice(this.bgs)), Width, Height))
            {
                using (var g = Graphics.FromImage(bm))
                {
                    //g.Clear(Color.White);
                    using (var ms = new MemoryStream())
                    {
                        //画验证码
                        var c = random.Choice(colors);
                        var i = 0;
                        foreach (var itm in items)
                        {
                            //计算位置
                            var (x, y) = ComputePosition(i++, itm.font, bm);


                            g.DrawString(itm.c, itm.font, new SolidBrush(c), x, y);


                            this.Code += itm.c;//把验证码保存起来
                        }

                        bm.Save(ms, ImageFormat.Png);
                        return (ms.ToArray(), Width, Height);
                        /*
                        byte[] bs = ms.ToArray();
                        response.OutputStream.Write(bs, 0, bs.Length);
                         * */
                    }
                }
            }
        }

        #endregion

        #region 方法
        private (float x, float y) ComputePosition(int i, Font font, Bitmap bm)
        {
            var font_h = font.Height;
            var font_w = font.Size;
            var box_w = bm.Width / CharCount;

            var x = box_w * i + (box_w - font_w) / 2;

            var y = (bm.Height - font_h) / 2;

            return (x, y);
        }
        #endregion
    }



    [ElasticsearchType(IdProperty = "Path", Name = "diskfileindex")]
    public class DiskFileIndex : IElasticSearchIndex
    {
        [String(Name = "Path", Index = FieldIndexOption.NotAnalyzed)]
        public virtual string Path { get; set; }

        [Number(Name = "Size", Index = NonStringIndexOption.NotAnalyzed)]
        public virtual long Size { get; set; }

        [String(Name = "Name", Index = FieldIndexOption.NotAnalyzed)]
        public virtual string Name { get; set; }

        [String(Name = "Extension", Index = FieldIndexOption.NotAnalyzed)]
        public virtual string Extension { get; set; }

        [String(Name = "Content", Analyzer = "ik_max_word", SearchAnalyzer = "ik_max_word")]
        public virtual string Content { get; set; }
    }

}