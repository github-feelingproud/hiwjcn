using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Hiwjcn.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.ServiceModel;
using System.IO;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Pens;
using SixLabors.ImageSharp.Drawing.Brushes;
using SixLabors.ImageSharp;

namespace Hiwjcn.Api.Controllers
{
    public class ffasdfasdgdsafa : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL("");
            optionsBuilder.UseNpgsql("");
            base.OnConfiguring(optionsBuilder);
        }

    }
    public class ServiceClient<T> : ClientBase<T>, IDisposable where T : class
    { }
    public class DrawVerifyCode
    {
        class CharItem
        {
            public string c { get; set; }
            public Font font { get; set; }
        }

        #region 各种参数
        private readonly Random random = new Random(DateTime.Now.Millisecond);

        /// <summary>
        /// 噪线的条数，默认5条
        /// </summary>
        public int LineCount { get; set; } = 5;

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
        public int CharCount { get; set; } = 4;

        private readonly List<Color> colors = new List<Color>() { Color.Purple, Color.Black, Color.DarkBlue, Color.DarkRed };
        private readonly List<string> fonts = new List<string>() { "Times New Roman", "MS Mincho", "Gungsuh", "PMingLiU", "Impact" };
        private readonly List<char> chars;

        /// <summary>
        /// 构造函数，设置response对象
        /// </summary>
        public DrawVerifyCode()
        {
            var chars = new List<char>();
            Action<char, char> FindChar = (start, end) =>
            {
                for (var i = start; i <= end; ++i)
                {
                    chars.Add((char)i);
                }
            };
            FindChar('a', 'z');
            FindChar('A', 'Z');
            FindChar('0', '9');

            chars.Remove('0');
            chars.Remove('O');
            chars.Remove('o');

            chars.Remove('l');
            chars.Remove('1');

            chars.Remove('9');
            chars.Remove('q');
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
            int width = 640;
            int height = 480;

            using (Image<Rgba32> img = new Image<Rgba32>(width, height))
            {
                //
                // do your drawing in here...
            }

            if (CharCount <= 0) { throw new Exception("字符数必须大于0"); }
            this.Code = string.Empty;

            var items = Com.Range(CharCount).Select(_ => new CharItem()
            {
                c = random.Choice(chars).ToString(),
                font = new Font(random.Choice(fonts), FontSize)
            }).ToList();

            //把验证码保存起来
            this.Code = items.Select(x => x.c).ConcatString();
            int Height = (int)(items.Select(x => x.font).Max(x => x.Height) * 1.3);
            int Width = (int)(Height * 0.8 * CharCount);
            //获取随机字体，颜色
            using (var bm = new Bitmap(Width, Height))
            {
                using (var g = Graphics.FromImage(bm))
                {
                    g.Clear(Color.White);
                    using (var ms = new MemoryStream())
                    {
                        //判断是否画噪线
                        if (LineCount > 0)
                        {
                            for (int k = 0; k < LineCount; ++k)
                            {
                                var x1 = random.Next(bm.Width);
                                var y1 = random.Next(bm.Height);
                                var x2 = random.Next(bm.Width);
                                var y2 = random.Next(bm.Height);
                                g.DrawLine(new Pen(random.Choice(colors)), x1, y1, x2, y2);
                            }
                        }
                        //画验证码
                        var i = 0;
                        foreach (var itm in items)
                        {
                            //计算位置
                            var (x, y) = ComputePosition(i++, itm.font, bm);

                            var angle = random.Next(-5, 5);
                            g.RotateTransform(angle);

                            g.DrawString(itm.c, itm.font, new SolidBrush(random.Choice(colors)), x, y);

                            g.RotateTransform(-angle);
                        }

                        bm.Save(ms, ImageFormats.Png);
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

    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
