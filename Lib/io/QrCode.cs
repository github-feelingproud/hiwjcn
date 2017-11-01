using Lib.helper;
using Lib.extension;
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
        public string Charset { get; set; }
        public ImageFormat Formart { get; set; }

        private string _charset { get => this.Charset ?? "UTF-8"; }
        private ImageFormat _formart { get => this.Formart ?? ImageFormat.Png; }

        public byte[] GetQrCodeBytes(string content, int size = 230)
        {
            content = ConvertHelper.GetString(content);

            var option = new QrCodeEncodingOptions()
            {
                CharacterSet = this._charset,
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
            using (var bm = writer.Write(content))
            {
                return bm.ToBytes(this._formart);
            }
        }

        public byte[] GetQrCodeWithIconBytes(string content, string icon_path, int size = 230)
        {
            if (!File.Exists(icon_path)) { throw new Exception("二维码水印图片不存在"); }
            var bs = this.GetQrCodeBytes(content, size);

            using (var bm = ConvertHelper.BytesToBitmap(bs))
            {
                using (var g = Graphics.FromImage(bm))
                {
                    using (var logo = Image.FromFile(icon_path))
                    {
                        using (var smallLogo = logo.GetThumbnailImage(bm.Width / 5, bm.Height / 5, null, IntPtr.Zero))
                        {
                            //把压缩后的图片绘制到二维码上
                            g.DrawImage(smallLogo, (bm.Width - smallLogo.Width) / 2, (bm.Height - smallLogo.Height) / 2);
                        }
                    }
                }
                return bm.ToBytes(this._formart);
            }
        }

        public byte[] GetBarCodeBytes(string content, int width = 300, int height = 50)
        {
            content = ConvertHelper.GetString(content);

            var options = new QrCodeEncodingOptions()
            {
                CharacterSet = this._charset,
                Width = width,
                Height = height,
                Margin = 1,
                // 是否是纯码，如果为 false，则会在图片下方显示数字
                PureBarcode = false,
            };

            var writer = new BarcodeWriter()
            {
                Format = BarcodeFormat.CODE_128,
                Options = options
            };

            using (var bm = writer.Write(content))
            {
                return bm.ToBytes(this._formart);
            }
        }

        /// <summary>
        /// 识别二维码
        /// </summary>
        public string DistinguishQrImage(string img_path) =>
            this.DistinguishQrImage(File.ReadAllBytes(img_path));

        /// <summary>
        /// 识别二维码
        /// </summary>
        public string DistinguishQrImage(byte[] b)
        {
            using (var stream = new MemoryStream(b))
            {
                using (var bm = new Bitmap(stream))
                {
                    var reader = new BarcodeReader();
                    reader.Options = new DecodingOptions()
                    {
                        CharacterSet = this._charset,
                        TryHarder = true
                    };
                    var res = reader.Decode(bm);
                    return res.Text;
                }
            }
        }
    }
}