using ImageProcessor;
using ImageProcessor.Imaging.Formats;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace Lib.io
{
    #region 知识补充
    //g.DrawArc 绘制一段弧线，它表示由一对坐标、宽度和高度指定的椭圆部分。 
    //g.DrawBezier 绘制由4个Point结构定义的贝塞尔样条。
    //g.DrawBeziers 用Point结构数组绘制一系列贝塞尔样条。
    //g.DrawClosedCurve 绘制由Point结构的数组定义的闭合基数样条。
    //g.DrawCurve 绘制经过一组指定的Point结构的基数样条。 
    //g.DrawEllipse 绘制一个由边框（该边框由一对坐标、高度和宽度指定）定义的椭圆。
    //g.DrawIcon 在指定坐标处绘制由指定的 Icon 表示的图像。 
    //g.DrawIconUnstretched 绘制指定的 Icon 表示的图像，而不缩放该图像。 
    //g.DrawImage 在指定位置并且按原始大小绘制指定的 Image。 
    //g.DrawImageUnscaled 在由坐标对指定的位置，使用图像的原始物理大小绘制指定的图像。 
    //g.DrawImageUnscaledAndClipped 在不进行缩放的情况下绘制指定的图像，并在需要时剪辑该图像以适合指定的矩形。
    //g.DrawLine 绘制一条连接由坐标对指定的两个点的线条。 
    //g.DrawLines 绘制一系列连接一组 Point 结构的线段。 
    //g.DrawPath 
    //g.DrawPie 绘制一个扇形，该形状由一个坐标对、宽度、高度以及两条射线所指定的椭圆定义。
    //g.DrawPolygon 绘制由一组 Point 结构定义的多边形。 
    //g.DrawRectangle 绘制由坐标对、宽度和高度指定的矩形。
    //g.DrawRectangles 绘制一系列由 Rectangle 结构指定的矩形。 
    //g.DrawString 在指定位置并且用指定的 Brush 和 Font 对象绘制指定的文本字符串.
    #endregion

    /// <summary>
    /// 图片处理
    /// 
    /// 使用imageprocessor处理图片
    /// http://imageprocessor.org/imageprocessor/imagefactory/resize/
    /// </summary>
    public static class ImageHelper
    {
        /// <summary>
        /// imageprogress 官方例子
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="newFilePath"></param>
        public static void _ResizeImage(string filePath, string newFilePath)
        {
            var photoBytes = File.ReadAllBytes(filePath);
            // Format is automatically detected though can be changed.
            var format = new JpegFormat { Quality = 70 };
            var size = new Size(150, 0);

            using (var inStream = new MemoryStream(photoBytes))
            {
                // Initialize the ImageFactory using the overload to preserve EXIF metadata.
                using (var imageFactory = new ImageFactory(preserveExifData: true))
                {
                    // Load, resize, set the format and quality and save an image.
                    imageFactory.Load(inStream)
                                .Resize(size)
                                .Format(format)
                                .Save(newFilePath);
                }
            }
        }

        /// <summary>
        /// resize img
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="newFilePath"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public static void ResizeImage(string filePath, string newFilePath, int width, int height)
        {
            var photoBytes = File.ReadAllBytes(filePath);
            var size = new Size(width, height);

            using (var inStream = new MemoryStream(photoBytes))
            {
                // Initialize the ImageFactory using the overload to preserve EXIF metadata.
                using (var imageFactory = new ImageFactory(preserveExifData: true))
                {
                    // Load, resize, set the format and quality and save an image.
                    imageFactory.Load(inStream).Resize(size).Save(newFilePath);
                }
            }
        }

        /// <summary>
        /// 计算压缩比例
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="max_width"></param>
        /// <param name="max_height"></param>
        /// <returns></returns>
        public static (int width, int height) NewSize(int width, int height,
            int? max_width = 500, int? max_height = 500)
        {
            var box = (w: width, h: height);
            if (max_width != null && box.w > max_width.Value)
            {
                box.w = max_width.Value;
                box.h = (max_width.Value / box.w) * box.h;
            }
            if (max_height != null && box.h > max_height.Value)
            {
                box.h = max_height.Value;
                box.w = (max_height.Value / box.h) * box.w;
            }
            return box;
        }

        /// <summary>
        /// 简易的字符画算法，还很不好用
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="rowSize"></param>
        /// <param name="colSize"></param>
        /// <returns></returns>
        public static string BitmapToAsciiArt(Bitmap bitmap, int rowSize, int colSize)
        {
            var result = new StringBuilder();
            char[] charset = { 'M', '8', '0', 'V', '1', 'i', ':', '*', '|', '.', ' ' };
            for (int h = 0; h < bitmap.Height / rowSize; ++h)
            {
                int offsetY = h * rowSize;
                for (int w = 0; w < bitmap.Width / colSize; w++)
                {
                    int offsetX = w * colSize;
                    float averBright = 0;
                    for (int j = 0; j < rowSize; ++j)
                    {
                        for (int i = 0; i < colSize; ++i)
                        {
                            try
                            {
                                Color color = bitmap.GetPixel(offsetX + i, offsetY + j);
                                averBright += color.GetBrightness();
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                averBright += 0;
                            }
                        }
                    }
                    averBright /= (rowSize * colSize);
                    int index = (int)(averBright * charset.Length);
                    if (index == charset.Length)
                    {
                        --index;
                    }
                    result.Append(charset[charset.Length - 1 - index]);
                }
                result.Append("\r\n");
            }
            return result.ToString();
        }

        public static bool IsPixelFormatIndexed(Image img)
        {
            if (img == null) { return false; }
            PixelFormat[] indexedPixelFormats = new PixelFormat[]
            {
                PixelFormat.Undefined, PixelFormat.DontCare,
                PixelFormat.Format16bppArgb1555, PixelFormat.Format1bppIndexed,
                PixelFormat.Format4bppIndexed,PixelFormat.Format8bppIndexed
            };
            return indexedPixelFormats.Contains(img.PixelFormat);
        }

        /// <summary>
        /// 如果原有的bitmap带有索引像素就销毁，重新生成一个
        /// </summary>
        /// <param name="bm"></param>
        /// <returns></returns>
        public static Bitmap RemovePixelIndexed(Bitmap bm)
        {
            if (IsPixelFormatIndexed(bm))
            {
                var newbitmap = new Bitmap(bm.Width, bm.Height, PixelFormat.Format32bppArgb);
                using (var g = Graphics.FromImage(newbitmap))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    g.DrawImage(bm, 0, 0);
                }
                bm.Dispose();
                return newbitmap;
            }
            return bm;
        }

        /// <summary>
        /// 生成缩略图的模式， WH-指定宽高缩放（可能变形） W-指定宽，高按比例  H-指定高，宽按比例 CUT-指定高宽裁减（不变形,推荐用这个）。
        /// </summary>
        public enum ThumbnailModeOption : byte
        {
            /// <summary>
            /// 指定宽高缩放（可能变形）
            /// </summary>
            WH,
            /// <summary>
            /// 指定宽，高按比例
            /// </summary>
            W,
            /// <summary>
            /// 指定高，宽按比例
            /// </summary>
            H,
            /// <summary>
            /// 指定高宽裁减（不变形,推荐用这个）
            /// </summary>
            CUT
        }

        /// <summary>
        /// 加图片水印的位置，TopLeft-左上角 TopCenter-上中间 TopRight-右上角 BottomLeft-左下角 BottomCenter-下中间 右下角-右下角 Middle-正中间。
        /// </summary>
        public enum WaterPositionOption : byte
        {
            /// <summary>
            /// 左上角
            /// </summary>
            LeftTop,
            /// <summary>
            /// 上中间
            /// </summary>
            CenterTop,
            /// <summary>
            /// 右上角
            /// </summary>
            RightTop,
            /// <summary>
            /// 左下角
            /// </summary>
            LeftBottom,
            /// <summary>
            /// 下中间
            /// </summary>
            CenterBottom,
            /// <summary>
            /// 右下角
            /// </summary>
            RightBottom,
            /// <summary>
            /// 正中间
            /// </summary>
            Middle
        }

        /// <summary>
        /// 获取图片格式。
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns></returns>
        public static ImageFormat GetImageFormat(string fileName)
        {
            string extension = fileName.Substring(fileName.LastIndexOf(".")).Trim().ToLower();

            switch (extension)
            {
                case ".jpg":
                    return System.Drawing.Imaging.ImageFormat.Jpeg;
                case ".jpeg":
                    return System.Drawing.Imaging.ImageFormat.Jpeg;
                case ".gif":
                    return System.Drawing.Imaging.ImageFormat.Gif;
                case ".png":
                    return System.Drawing.Imaging.ImageFormat.Png;
                case ".bmp":
                    return System.Drawing.Imaging.ImageFormat.Bmp;
                case ".ico":
                    return System.Drawing.Imaging.ImageFormat.Icon;
                default:
                    goto case ".jpg";
            }
        }

        /// <summary>
        /// 加水印图片并保存。
        /// </summary>
        /// <param name="originalImageStream">Stream</param>
        /// <param name="strFileName">源图路径（物理路径）</param>
        /// <param name="savePath">图片保存路径（物理路径）</param>
        /// <param name="waterPath">水印图路径（物理路径）</param>
        /// <param name="edge">水印图离原图边界的距离</param>
        /// <param name="position">加图片水印的位置</param>
        /// <returns>是否成功</returns>
        public static bool MakeWaterImage(Stream originalImageStream, string strFileName, string savePath, string waterPath, int edge, WaterPositionOption position)
        {
            bool success = false;

            int x = 0;
            int y = 0;
            Image waterImage = null;
            Image image = null;
            Bitmap bitmap = null;
            Graphics graphics = null;

            try
            {
                //加载原图
                image = Image.FromStream(originalImageStream);
                //加载水印图
                waterImage = Image.FromFile(waterPath);
                bitmap = new Bitmap(image);
                graphics = Graphics.FromImage(bitmap);

                int newEdge = edge;
                if (newEdge >= image.Width + waterImage.Width) newEdge = 10;

                switch (position)
                {
                    case WaterPositionOption.LeftTop:
                        x = newEdge;
                        y = newEdge;
                        break;
                    case WaterPositionOption.CenterTop:
                        x = (image.Width - waterImage.Width) / 2;
                        y = newEdge;
                        break;
                    case WaterPositionOption.RightTop:
                        x = image.Width - waterImage.Width - newEdge;
                        y = newEdge;
                        break;
                    case WaterPositionOption.LeftBottom:
                        x = newEdge;
                        y = image.Height - waterImage.Height - newEdge;
                        break;
                    case WaterPositionOption.CenterBottom:
                        x = (image.Width - waterImage.Width) / 2;
                        y = image.Height - waterImage.Height - newEdge;
                        break;
                    case WaterPositionOption.RightBottom:
                        x = image.Width - waterImage.Width - newEdge;
                        y = image.Height - waterImage.Height - newEdge;
                        break;
                    case WaterPositionOption.Middle:
                        x = (image.Width - waterImage.Width) / 2;
                        y = (image.Height - waterImage.Height) / 2;
                        break;
                    default:
                        goto case WaterPositionOption.RightBottom;
                }

                // 画水印图片
                graphics.DrawImage(waterImage, new Rectangle(x, y, waterImage.Width, waterImage.Height), 0, 0, waterImage.Width, waterImage.Height, GraphicsUnit.Pixel);

                // 关闭打开着的文件并保存（覆盖）新图片
                originalImageStream.Close();
                bitmap.Save(savePath, GetImageFormat(strFileName));

                success = true;
            }
            catch (Exception ex)
            {
                success = false;
                throw;
                //throw new Exception(ex.Message.Replace("'", " ").Replace("\n", " ").Replace("\\", "/"));
            }
            finally
            {
                if (graphics != null) graphics.Dispose();
                if (bitmap != null) bitmap.Dispose();
                if (image != null) image.Dispose();
                if (waterImage != null) waterImage.Dispose();
            }

            return success;
        }

        /// <summary>
        /// 生成缩略图并保存。
        /// </summary>
        /// <param name="originalImageStream">Stream</param>
        /// <param name="strFileName">源图路径（物理路径）</param>
        /// <param name="thumbnailPath">缩略图路径（物理路径）</param>
        /// <param name="maxWidth">缩略图最大宽度</param>
        /// <param name="maxheight">缩略图最大高度</param>
        /// <param name="mode">生成缩略图的方式</param>
        /// <returns>是否成功</returns>
        public static bool MakeThumbnail(Stream originalImageStream, string strFileName, string thumbnailPath, int maxWidth, int maxheight, ThumbnailModeOption mode)
        {
            bool success = false;

            int x = 0;
            int y = 0;
            int toW = maxWidth;
            int toH = maxheight;
            Image image = null;
            Image bitmap = null;
            Graphics graphics = null;
            try
            {
                image = Image.FromStream(originalImageStream);
                int w = image.Width;
                int h = image.Height;

                switch (mode)
                {
                    case ThumbnailModeOption.WH:
                        break;
                    case ThumbnailModeOption.W:
                        if (w < maxWidth)
                        {
                            toW = w;
                            toH = h;
                        }
                        else
                        {
                            toH = h * maxWidth / w;
                        }
                        break;
                    case ThumbnailModeOption.H:
                        if (h < maxheight)
                        {
                            toW = w;
                            toH = h;
                        }
                        else
                        {
                            toW = w * maxheight / h;
                        }
                        break;
                    case ThumbnailModeOption.CUT:
                        if (((double)w / (double)h) > ((double)toW / (double)toH))
                        {
                            w = h * toW / toH;
                            y = 0;
                            x = (image.Width - w) / 2;
                        }
                        else
                        {
                            h = w * toH / toW;
                            x = 0;
                            y = (image.Height - h) / 2;
                        }
                        break;
                    default:
                        goto case ThumbnailModeOption.CUT;
                }

                bitmap = new Bitmap(toW, toH);
                graphics = Graphics.FromImage(bitmap);
                graphics.InterpolationMode = InterpolationMode.High;   //设置高质量,低速度呈现平滑程度
                graphics.SmoothingMode = SmoothingMode.HighQuality;    //清空画布并以透明背景色填充
                graphics.Clear(Color.Transparent);

                // 在指定位置并且按指定大小绘制原图片的指定部分
                graphics.DrawImage(image, new Rectangle(0, 0, toW, toH), new Rectangle(x, y, w, h), GraphicsUnit.Pixel);
                // 保存缩略图
                bitmap.Save(thumbnailPath, GetImageFormat(strFileName));
                success = true;
            }
            catch (Exception e)
            {
                success = false;
                throw e;
            }
            finally
            {
                if (graphics != null) graphics.Dispose();
                if (bitmap != null) bitmap.Dispose();
                if (image != null) image.Dispose();
            }
            return success;
        }

        /// <summary>
        /// 增加图片文字水印
        /// </summary>
        /// <param name="filename">文件名</param>
        /// <param name="watermarkText">水印文字</param>
        /// <param name="watermarkStatus">图片水印位置</param>
        public static void AddImageSignText(Image img, string filename, string watermarkText, int watermarkStatus, int quality, string fontname, int fontsize)
        {
            Graphics g = Graphics.FromImage(img);
            Font drawFont = new Font(fontname, fontsize, FontStyle.Regular, GraphicsUnit.Pixel);
            SizeF crSize;
            crSize = g.MeasureString(watermarkText, drawFont);

            float xpos = 0;
            float ypos = 0;

            switch (watermarkStatus)
            {
                case 1:
                    xpos = (float)img.Width * (float).01;
                    ypos = (float)img.Height * (float).01;
                    break;
                case 2:
                    xpos = ((float)img.Width * (float).50) - (crSize.Width / 2);
                    ypos = (float)img.Height * (float).01;
                    break;
                case 3:
                    xpos = ((float)img.Width * (float).99) - crSize.Width;
                    ypos = (float)img.Height * (float).01;
                    break;
                case 4:
                    xpos = (float)img.Width * (float).01;
                    ypos = ((float)img.Height * (float).50) - (crSize.Height / 2);
                    break;
                case 5:
                    xpos = ((float)img.Width * (float).50) - (crSize.Width / 2);
                    ypos = ((float)img.Height * (float).50) - (crSize.Height / 2);
                    break;
                case 6:
                    xpos = ((float)img.Width * (float).99) - crSize.Width;
                    ypos = ((float)img.Height * (float).50) - (crSize.Height / 2);
                    break;
                case 7:
                    xpos = (float)img.Width * (float).01;
                    ypos = ((float)img.Height * (float).99) - crSize.Height;
                    break;
                case 8:
                    xpos = ((float)img.Width * (float).50) - (crSize.Width / 2);
                    ypos = ((float)img.Height * (float).99) - crSize.Height;
                    break;
                case 9:
                    xpos = ((float)img.Width * (float).99) - crSize.Width;
                    ypos = ((float)img.Height * (float).99) - crSize.Height;
                    break;
            }

            g.DrawString(watermarkText, drawFont, new SolidBrush(Color.White), xpos + 1, ypos + 1);
            g.DrawString(watermarkText, drawFont, new SolidBrush(Color.Black), xpos, ypos);

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            ImageCodecInfo ici = null;
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.MimeType.IndexOf("jpeg") > -1)
                {
                    ici = codec;
                }
            }
            EncoderParameters encoderParams = new EncoderParameters();
            long[] qualityParam = new long[1];
            if (quality < 0 || quality > 100)
            {
                quality = 80;
            }
            qualityParam[0] = quality;

            EncoderParameter encoderParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, qualityParam);
            encoderParams.Param[0] = encoderParam;

            if (ici != null)
            {
                img.Save(filename, ici, encoderParams);
            }
            else
            {
                img.Save(filename);
            }
            g.Dispose();
            //bmp.Dispose();
            img.Dispose();

        }

    }

}
