using Lib.helper;
using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.Rendering;

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
        private Bitmap GetBitmap(string content, int size = 230, string img_path = null)
        {
            content = ConvertHelper.GetString(content);

            var option = new QrCodeEncodingOptions()
            {
                CharacterSet = "UTF-8",
                DisableECI = true,
                ErrorCorrection = ZXing.QrCode.Internal.ErrorCorrectionLevel.H,
                Width = size,
                Height = size,
                Margin = 1
            };

            var writer = new BarcodeWriter()
            {
                Format = BarcodeFormat.QR_CODE,
                Options = option
            };

            //生成bitmap
            var bm = writer.Write(content);

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

        private Bitmap GetBarCodexx(string content, int width = 300, int height = 50)
        {
            var options = new QrCodeEncodingOptions();
            options.CharacterSet = "UTF-8";
            options.Width = 300;
            options.Height = 50;
            options.Margin = 1;
            options.PureBarcode = false; // 是否是纯码，如果为 false，则会在图片下方显示数字

            var writer = new BarcodeWriter()
            {
                Format = BarcodeFormat.CODE_128,
                Options = options
            };

            return writer.Write(content);
        }

        public byte[] GetBitmapBytes(string content, int size = 230, string img_path = null)
        {
            using (var bm = GetBitmap(content, size, img_path))
            {
                return ConvertHelper.BitmapToBytes(bm);
            }
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
        private string DistinguishQrImage(Bitmap bm)
        {
            bm = bm ?? throw new Exception("bitmap is null");
            var reader = new BarcodeReader();
            reader.Options = new DecodingOptions() { CharacterSet = "UTF-8" };
            var res = reader.Decode(bm);
            return res.Text;
        }

        /// <summary>
        /// 识别二维码
        /// </summary>
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
        public string DistinguishQrImage(byte[] b)
        {
            using (var stream = new MemoryStream(b))
            {
                using (var bm = new Bitmap(stream))
                {
                    return DistinguishQrImage(bm);
                }
            }
        }
    }
}