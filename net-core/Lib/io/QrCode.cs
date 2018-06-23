using Lib.extension;
using Lib.helper;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Drawing;
using SixLabors.ImageSharp.Processing.Filters;
using SixLabors.ImageSharp.Processing.Transforms;
using System;
using System.Drawing.Imaging;
using System.IO;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.QrCode.Internal;

namespace Lib.io
{
    /// <summary>
    /// 生成二维码和条码
    /// </summary>
    public class QrCode
    {
        public const int QRCODE_SIZE = 230;
        public const int BARCODE_SIZE_WIDTH = 300;
        public const int BARCODE_SIZE_HEIGHT = 100;

        public QrCode()
        {
            //初始化参数
        }

        public string Charset { get; set; } = "UTF-8";
        public ImageFormat Formart { get; set; } = ImageFormat.Png;
        public int Margin { get; set; } = 1;
        public ErrorCorrectionLevel ErrorCorrectionLevel { get; set; } = ErrorCorrectionLevel.H;

        /// <summary>
        /// 二维码
        /// </summary>
        public byte[] GetQrCodeBytes(string content,
            int size = QRCODE_SIZE)
        {
            content = ConvertHelper.GetString(content);

            var option = new QrCodeEncodingOptions()
            {
                CharacterSet = this.Charset,
                DisableECI = true,
                ErrorCorrection = this.ErrorCorrectionLevel ?? ErrorCorrectionLevel.H,
                Width = size,
                Height = size,
                Margin = this.Margin
            };

            var writer = new BarcodeWriterPixelData()
            {
                Format = BarcodeFormat.QR_CODE,
                Options = option
            };

            var pix = writer.Write(content);
            return pix.Pixels;
        }

        /// <summary>
        /// 条码
        /// </summary>
        public byte[] GetBarCodeBytes(string content,
            int width = BARCODE_SIZE_WIDTH, int height = BARCODE_SIZE_HEIGHT)
        {
            content = ConvertHelper.GetString(content);

            var options = new QrCodeEncodingOptions()
            {
                CharacterSet = this.Charset,
                Width = width,
                Height = height,
                Margin = this.Margin,
                // 是否是纯码，如果为 false，则会在图片下方显示数字
                PureBarcode = false,
            };

            var writer = new BarcodeWriterPixelData()
            {
                Format = BarcodeFormat.CODE_128,
                Options = options
            };

            var pix = writer.Write(content);
            return pix.Pixels;
        }

        /// <summary>
        /// 识别二维码
        /// </summary>
        public string ReadQrCodeText(byte[] b)
        {
            var reader = new BarcodeReader();
            reader.Options = new DecodingOptions()
            {
                CharacterSet = this.Charset,
                TryHarder = true
            };
            var res = reader.Decode(b);
            return res.Text;
        }

        [Obsolete("解码。。。。。？")]
        public string ReadBarCodeText(byte[] b)
        {
            var reader = new BarcodeReader();
            reader.Options = new DecodingOptions()
            {
                CharacterSet = this.Charset,
                TryHarder = true
            };
            var res = reader.Decode(b);
            return res.Text;
        }
    }

    public static class QrCodeExtension
    {

        /// <summary>
        /// 带图标二维码，加上图标后请把容错级别调高，这样可以提高识别成功率
        /// </summary>
        public static byte[] GetQrCodeWithIconBytes(this QrCode coder,
            string content, string icon_path, int size = QrCode.QRCODE_SIZE)
        {
            if (!File.Exists(icon_path)) { throw new Exception("二维码水印图片不存在"); }
            var bs = coder.GetQrCodeBytes(content, size);

            bs = coder.AddIcon(bs, icon_path);
            return bs;
        }

        /// <summary>
        /// 添加icon
        /// </summary>
        public static byte[] AddIcon(this QrCode coder, byte[] bs, string icon_path)
        {
            using (var ms = new MemoryStream())
            {
                using (var img = SixLabors.ImageSharp.Image.Load(bs))
                {
                    using (var icon = SixLabors.ImageSharp.Image.Load(icon_path))
                    {
                        icon.Mutate(x => x.Resize(width: img.Width / 5, height: img.Height / 5));

                        var location = new SixLabors.Primitives.Point()
                        {
                            X = (img.Width - icon.Width) / 2,
                            Y = (img.Height - icon.Height) / 2
                        };
                        img.Mutate(x => x.DrawImage(icon, 1, location));
                        img.Save(ms, new SixLabors.ImageSharp.Formats.Png.PngEncoder());
                    }
                }
                return ms.ToArray();
            }
        }

        /// <summary>
        /// 随机调色
        /// </summary>
        public static byte[] AddRandomHue(this QrCode coder, byte[] bs)
        {
            using (var ms = new MemoryStream())
            {
                using (var img = SixLabors.ImageSharp.Image.Load(bs))
                {
                    var ran = new Random((int)DateTime.Now.Ticks);
                    img.Mutate(x => x.Hue(ran.RealNext(360 - 1)));
                    img.Save(ms, new SixLabors.ImageSharp.Formats.Png.PngEncoder());
                }
                return ms.ToArray();
            }
        }

        /// <summary>
        /// 给二维码添加背景图片
        /// </summary>
        public static byte[] AddBackgroundImage(this QrCode coder, byte[] bs, byte[] background,
            (double width_scale, double height_scale)? scale = null)
        {
            scale = scale ?? (1.5, 1.5);

            using (var ms = new MemoryStream())
            {
                using (var qr = SixLabors.ImageSharp.Image.Load(bs))
                {
                    using (var img = SixLabors.ImageSharp.Image.Load(background))
                    {
                        var Width = (int)(qr.Width * scale.Value.width_scale);
                        var Height = (int)(qr.Height * scale.Value.height_scale);
                        img.Mutate(x => x.Resize(Width, Height));

                        var location = new SixLabors.Primitives.Point()
                        {
                            X = (img.Width - qr.Width) / 2,
                            Y = (img.Height - qr.Height) / 2
                        };
                        img.Mutate(x => x.DrawImage(qr, 1f, location));

                        img.Save(ms, new SixLabors.ImageSharp.Formats.Png.PngEncoder());
                    }
                }
                return ms.ToArray();
            }
        }
    }
}
