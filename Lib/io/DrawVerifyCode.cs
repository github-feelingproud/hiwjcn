using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace Lib.io
{
    public enum VerifyCodeTypeEnum : int
    {
        数字 = 1,
        字母 = 2,
        混合 = 3,
        汉字 = 4
    }

    /// <summary>
    ///用于生成多种样式的验证码
    /// </summary>
    public class DrawVerifyCode
    {
        #region 各种参数
        private readonly Random ran = new Random(DateTime.Now.Millisecond);
        private delegate string RandomFunc();

        /// <summary>
        /// 噪线的条数，默认5条
        /// </summary>
        public int LineCount { get; set; }

        /// <summary>
        /// 生成的code
        /// </summary>
        public string Code { get; private set; }

        /// <summary>
        /// 字符类型，数字，字母，混合，汉字
        /// </summary>
        public VerifyCodeTypeEnum CodeType { get; set; }

        /// <summary>
        /// 字体大小，【默认15px】为了使字符居中显示，请设置一个合适的值
        /// </summary>
        public int FontSize { get; set; }

        /// <summary>
        /// 验证码字符数
        /// </summary>
        public int CharCount { get; set; }
        /// <summary>
        /// 宽
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// 高
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// 构造函数，设置response对象
        /// </summary>
        public DrawVerifyCode()
        {
            this.CharCount = 4;
            this.FontSize = 15;
            this.LineCount = 5;
            this.CodeType = VerifyCodeTypeEnum.混合;
            this.Height = 40;
            this.Width = 110;

            this.Code = string.Empty;
        }

        #endregion

        #region 主体程序
        public byte[] GetImageBytes()
        {
            RandomFunc randomchar;
            //判断使用中文或者英文
            switch (CodeType)
            {
                case VerifyCodeTypeEnum.字母: randomchar = randomChar_en; break;
                case VerifyCodeTypeEnum.数字: randomchar = randomChar_num; break;
                case VerifyCodeTypeEnum.汉字: randomchar = randomChar_cn; break;
                default: randomchar = randomChar_en; break;
            }
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
                            int x1 = 0, y1 = 0, x2 = 0, y2 = 0;
                            for (int k = 0; k < LineCount; ++k)
                            {
                                x1 = ran.Next(Width);
                                y1 = ran.Next(Height);
                                x2 = ran.Next(Width);
                                y2 = ran.Next(Height);
                                g.DrawLine(new Pen(randomColor()), x1, y1, x2, y2);
                            }
                        }
                        //画验证码
                        for (int i = 0; i < CharCount; ++i)
                        {
                            string c = randomchar();
                            this.Code += c;//把验证码保存起来
                            float x = 0, y = 0;
                            getCharPosition(i + 1, ref x, ref y);//计算字符的位置
                            g.DrawString(c, randomFont(), new SolidBrush(randomColor()), x, y);
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

        #region 各种随机，各种算法
        /// <summary>
        /// 计算字符的位置
        /// </summary>
        /// <param name="i">第几个字符</param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void getCharPosition(int i, ref float x, ref float y)
        {
            float spaceX = (Width - CharCount * FontSize) / (CharCount + 1);
            float spaceY = (Height - FontSize) / 2;
            x = spaceX * i + FontSize * (i - 1);
            y = spaceY;
        }
        /// <summary>
        /// 产生随机英文数字字符
        /// </summary>
        private string randomChar_en()
        {
            var chars = new List<char>();
            void FindChar(char start, char end)
            {
                for (var i = start; i <= end; ++i)
                {
                    chars.Add((char)i);
                }
            }
            FindChar('a', 'z');
            FindChar('A', 'Z');
            FindChar('0', '9');
            return chars[ran.Next(chars.Count)].ToString();
        }
        /// <summary>
        /// 产生随机中文字符
        /// </summary>
        /// <returns></returns>
        private string randomChar_cn()
        {
            var gb = Encoding.GetEncoding("gb2312");
            //调用函数产生4个随机中文汉字编码
            object[] bytes = CreateRegionCode(1);
            //根据汉字编码的字节数组解码出中文汉字
            return gb.GetString((byte[])Convert.ChangeType(bytes[0], typeof(byte[])));
        }
        /// <summary>
        /// 获取随机的中文编码
        /// </summary>
        /// <param name="strlength"></param>
        /// <returns></returns>
        private object[] CreateRegionCode(int strlength)
        {
            //定义一个字符串数组储存汉字编码的组成元素
            string[] Base = new String[16] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "a", "b", "c", "d", "e", "f" };
            //这里不要定义随机对象，因为每请求一次都会重新生成随机对象，这样就会出现重复的字符串
            //定义一个object数组用来
            object[] bytes = new object[strlength];
            /*每循环一次产生一个含两个元素的十六进制字节数组，并将其放入bject数组中
             每个汉字有四个区位码组成
             区位码第1位和区位码第2位作为字节数组第一个元素
             区位码第3位和区位码第4位作为字节数组第二个元素
            */
            int r1, r2, r3, r4;
            for (int i = 0; i < strlength; i++)
            {
                //区位码第1位
                r1 = ran.Next(11, 14);
                string str_r1 = Base[r1].Trim();
                //区位码第2位
                if (r1 == 13)
                {
                    r2 = ran.Next(0, 7);
                }
                else
                {
                    r2 = ran.Next(0, 16);
                }
                string str_r2 = Base[r2].Trim();
                //区位码第3位
                r3 = ran.Next(10, 16);
                string str_r3 = Base[r3].Trim();
                //区位码第4位
                if (r3 == 10)
                {
                    r4 = ran.Next(1, 16);
                }
                else if (r3 == 15)
                {
                    r4 = ran.Next(0, 15);
                }
                else
                {
                    r4 = ran.Next(0, 16);
                }
                string str_r4 = Base[r4].Trim();
                //定义两个字节变量存储产生的随机汉字区位码
                byte byte1 = Convert.ToByte(str_r1 + str_r2, 16);
                byte byte2 = Convert.ToByte(str_r3 + str_r4, 16);
                //将两个字节变量存储在字节数组中
                byte[] str_r = new byte[] { byte1, byte2 };
                //将产生的一个汉字的字节数组放入object数组中
                bytes[i] = str_r;
            }
            return bytes;
        }
        /// <summary>
        /// 返回随机数字
        /// </summary>
        /// <returns></returns>
        private string randomChar_num()
        {
            char[] chars = { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' };
            return chars[ran.Next(chars.Length)].ToString();
        }
        /// <summary>
        /// 获取随机字体
        /// </summary>
        /// <returns></returns>
        private Font randomFont()
        {
            if (CodeType == VerifyCodeTypeEnum.汉字)
            {
                FontSize = 15;
            }
            string[] fonts = { "Times New Roman", "MS Mincho", "Gungsuh", "PMingLiU", "Impact" };
            return new Font(fonts[ran.Next(fonts.Length)], FontSize);
        }
        /// <summary>
        /// 获取随机颜色
        /// </summary>
        /// <returns></returns>
        private Color randomColor()
        {
            var colors = new Color[] { Color.Purple, Color.Black, Color.DarkBlue, Color.DarkRed };
            return colors[ran.Next(colors.Length)];
        }

        #endregion
    }
}

