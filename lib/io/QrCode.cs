using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Web;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using com.google.zxing;
using com.google.zxing.common;
using com.google.zxing.qrcode;
using Lib.helper;
using com.google.zxing.qrcode.decoder;
using System.Text;
using Lib.http;
using Lib.core;

namespace Lib.io
{
    /// <summary>
    /// Summary description for QrCode
    /// </summary>
    public class QrCode
    {
        /// <summary>
        /// 生成二维码的bitmap
        /// </summary>
        /// <param name="content"></param>
        /// <param name="size"></param>
        /// <param name="img_path"></param>
        /// <returns></returns>
        public Bitmap GetBitmap(string content, int size = 230, string img_path = null)
        {
            content = ConvertHelper.GetString(content);

            var writer = new MultiFormatWriter();
            //参数(如果不把容错能力设置高一些，添加logo后就无法识别)
            var hints = new Hashtable();
            hints.Add(EncodeHintType.ERROR_CORRECTION, ErrorCorrectionLevel.H);//容错能力
            hints.Add(EncodeHintType.CHARACTER_SET, "UTF-8");//字符集
            var byteMatrix = writer.encode(content, BarcodeFormat.QR_CODE, size, size, hints);
            //生成bitmap
            var bm = byteMatrix.ToBitmap();
            /*
             var bm = new Bitmap(size, size, PixelFormat.Format32bppArgb);
            for (int x = 0; x < size; ++x)
            {
                for (int y = 0; y < size; ++y)
                {
                    bm.SetPixel(x, y, byteMatrix.get_Renamed(x, y) != -1 ? Color.Black : Color.White);
                }
            }
             */
            //如果有小图片就绘制
            if (ValidateHelper.IsPlumpString(img_path))
            {
                if (!File.Exists(img_path))
                {
                    throw new Exception("二维码水印图片不存在");
                }
                //如果图片已经有被索引的像素，就删除原来的bm，重新生成
                bm = ImageHelper.RemovePixelIndexed(bm);
                using (var g = Graphics.FromImage(bm))
                {
                    using (var logo = Image.FromFile(img_path))
                    {
                        using (var smallLogo = logo.GetThumbnailImage(bm.Width / 5, bm.Height / 5, null, IntPtr.Zero))
                        {
                            //把压缩后的图片绘制到二维码上
                            g.DrawImage(smallLogo, (bm.Width - smallLogo.Width) / 2, (bm.Height - smallLogo.Height) / 2);
                        }
                    }
                }
            }
            return bm;
        }

        public void WriteToFile(string content, string file_path, string img_path = null)
        {
            using (var bm = this.GetBitmap(content, img_path: img_path))
            {
                bm.Save(file_path, ImageFormat.Png);
            }
        }

        /// <summary>
        /// 识别二维码
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public string DistinguishQrImage(Bitmap bm)
        {
            if (bm == null)
            {
                throw new Exception("bitmap is null");
            }
            using (bm)
            {
                var source = new RGBLuminanceSource(bm, bm.Width, bm.Height);
                var bbm = new BinaryBitmap(new HybridBinarizer(source));
                var result = new MultiFormatReader().decode(bbm);
                return result.Text;
            }
        }
        /// <summary>
        /// 识别二维码
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public string DistinguishQrImage(string img_path)
        {
            using (var bm = new Bitmap(img_path))
            {
                return DistinguishQrImage(bm);
            }
        }
        /// <summary>
        /// 识别二维码
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public string DistinguishQrImage(byte[] b)
        {
            using (var stream = new MemoryStream(b))
            {
                string str = DistinguishQrImage(new Bitmap(stream));
                return str;
            }
        }
    }
}