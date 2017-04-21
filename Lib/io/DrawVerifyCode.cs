using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using Lib.extension;

namespace Lib.io
{
    /// <summary>
    ///用于生成多种样式的验证码
    /// </summary>
    public class DrawVerifyCode
    {
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
        /// <summary>
        /// 宽
        /// </summary>
        public int Width { get; set; } = 110;
        /// <summary>
        /// 高
        /// </summary>
        public int Height { get; set; } = 40;

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
        public byte[] GetImageBytes()
        {
            if (CharCount <= 0) { throw new Exception("字符数必须大于0"); }
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
                                var x1 = random.Next(Width);
                                var y1 = random.Next(Height);
                                var x2 = random.Next(Width);
                                var y2 = random.Next(Height);
                                g.DrawLine(new Pen(random.Choice(colors)), x1, y1, x2, y2);
                            }
                        }
                        //画验证码
                        for (int i = 0; i < CharCount; ++i)
                        {
                            var c = random.Choice(chars).ToString();
                            var font = new Font(random.Choice(fonts), FontSize);

                            //计算位置
                            var (x, y) = ComputePosition(i, font);

                            var angle = random.Next(-5, 5);
                            g.RotateTransform(angle);

                            g.DrawString(c, font, new SolidBrush(random.Choice(colors)), x, y);

                            g.RotateTransform(-angle);

                            this.Code += c;//把验证码保存起来
                        }

                        bm.Save(ms, ImageFormat.Png);
                        return ms.ToArray();
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
        private (float x, float y) ComputePosition(int i, Font font)
        {
            var font_h = font.Height;
            var font_w = font.Size;
            var box_w = Width / CharCount;

            var x = box_w * i + (box_w - font_w) / 2;

            var y = (Height - font_h) / 2;

            return (x, y);
        }
        #endregion
    }
}

