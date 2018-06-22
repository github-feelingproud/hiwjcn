﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.core
{
    public static class StaticData
    {
        public static class HttpClient_
        {
            public static readonly IReadOnlyList<string> UserAgentsFactory = new List<string>()
        {
            "Mozilla/5.0 (Linux; U; Android 2.3.6; en-us; Nexus S Build/GRK39F) AppleWebKit/533.1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1",
            "Avant Browser/1.2.789rel1 (http://www.avantbrowser.com)",
            "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US) AppleWebKit/532.5 (KHTML, like Gecko) Chrome/4.0.249.0 Safari/532.5",
            "Mozilla/5.0 (Windows; U; Windows NT 5.2; en-US) AppleWebKit/532.9 (KHTML, like Gecko) Chrome/5.0.310.0 Safari/532.9",
            "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US) AppleWebKit/534.7 (KHTML, like Gecko) Chrome/7.0.514.0 Safari/534.7",
            "Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US) AppleWebKit/534.14 (KHTML, like Gecko) Chrome/9.0.601.0 Safari/534.14",
            "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US) AppleWebKit/534.14 (KHTML, like Gecko) Chrome/10.0.601.0 Safari/534.14",
            "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US) AppleWebKit/534.20 (KHTML, like Gecko) Chrome/11.0.672.2 Safari/534.20",
            "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/534.27 (KHTML, like Gecko) Chrome/12.0.712.0 Safari/534.27",
            "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.1 (KHTML, like Gecko) Chrome/13.0.782.24 Safari/535.1",
            "Mozilla/5.0 (Windows NT 6.0) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.120 Safari/535.2",
            "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.7 (KHTML, like Gecko) Chrome/16.0.912.36 Safari/535.7",
            "Mozilla/5.0 (Windows; U; Windows NT 6.0 x64; en-US; rv:1.9pre) Gecko/2008072421 Minefield/3.0.2pre",
            "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.9.0.10) Gecko/2009042316 Firefox/3.0.10",
            "Mozilla/5.0 (Windows; U; Windows NT 6.0; en-GB; rv:1.9.0.11) Gecko/2009060215 Firefox/3.0.11 (.NET CLR 3.5.30729)",
            "Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US; rv:1.9.1.6) Gecko/20091201 Firefox/3.5.6 GTB5",
            "Mozilla/5.0 (Windows; U; Windows NT 5.1; tr; rv:1.9.2.8) Gecko/20100722 Firefox/3.6.8 ( .NET CLR 3.5.30729; .NET4.0E)",
            "Mozilla/5.0 (Windows NT 6.1; rv:2.0.1) Gecko/20100101 Firefox/4.0.1",
            "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:2.0.1) Gecko/20100101 Firefox/4.0.1",
            "Mozilla/5.0 (Windows NT 5.1; rv:5.0) Gecko/20100101 Firefox/5.0",
            "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:6.0a2) Gecko/20110622 Firefox/6.0a2",
            "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:7.0.1) Gecko/20100101 Firefox/7.0.1",
            "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:2.0b4pre) Gecko/20100815 Minefield/4.0b4pre",
            "Mozilla/4.0 (compatible; MSIE 5.5; Windows NT 5.0 )",
            "Mozilla/4.0 (compatible; MSIE 5.5; Windows 98; Win 9x 4.90)",
            "Mozilla/5.0 (Windows; U; Windows XP) Gecko MultiZilla/1.6.1.0a",
            "Mozilla/2.02E (Win95; U)",
            "Mozilla/3.01Gold (Win95; I)",
            "Mozilla/4.8 [en] (Windows NT 5.1; U)",
            "Mozilla/5.0 (Windows; U; Win98; en-US; rv:1.4) Gecko Netscape/7.1 (ax)",
            "HTC_Dream Mozilla/5.0 (Linux; U; Android 1.5; en-ca; Build/CUPCAKE) AppleWebKit/528.5  (KHTML, like Gecko) Version/3.1.2 Mobile Safari/525.20.1",
            "Mozilla/5.0 (hp-tablet; Linux; hpwOS/3.0.2; U; de-DE) AppleWebKit/534.6 (KHTML, like Gecko) wOSBrowser/234.40.1 Safari/534.6 TouchPad/1.0",
            "Mozilla/5.0 (Linux; U; Android 1.5; en-us; sdk Build/CUPCAKE) AppleWebkit/528.5  (KHTML, like Gecko) Version/3.1.2 Mobile Safari/525.20.1",
            "Mozilla/5.0 (Linux; U; Android 2.1; en-us; Nexus One Build/ERD62) AppleWebKit/530.17 (KHTML, like Gecko) Version/4.0 Mobile Safari/530.17",
            "Mozilla/5.0 (Linux; U; Android 2.2; en-us; Nexus One Build/FRF91) AppleWebKit/533.1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1",
            "Mozilla/5.0 (Linux; U; Android 1.5; en-us; htc_bahamas Build/CRB17) AppleWebKit/528.5  (KHTML, like Gecko) Version/3.1.2 Mobile Safari/525.20.1",
            "Mozilla/5.0 (Linux; U; Android 2.1-update1; de-de; HTC Desire 1.19.161.5 Build/ERE27) AppleWebKit/530.17 (KHTML, like Gecko) Version/4.0 Mobile Safari/530.17",
            "Mozilla/5.0 (Linux; U; Android 2.2; en-us; Sprint APA9292KT Build/FRF91) AppleWebKit/533.1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1",
            "Mozilla/5.0 (Linux; U; Android 1.5; de-ch; HTC Hero Build/CUPCAKE) AppleWebKit/528.5  (KHTML, like Gecko) Version/3.1.2 Mobile Safari/525.20.1",
            "Mozilla/5.0 (Linux; U; Android 2.2; en-us; ADR6300 Build/FRF91) AppleWebKit/533.1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1",
            "Mozilla/5.0 (Linux; U; Android 2.1; en-us; HTC Legend Build/cupcake) AppleWebKit/530.17 (KHTML, like Gecko) Version/4.0 Mobile Safari/530.17",
            "Mozilla/5.0 (Linux; U; Android 1.5; de-de; HTC Magic Build/PLAT-RC33) AppleWebKit/528.5  (KHTML, like Gecko) Version/3.1.2 Mobile Safari/525.20.1 FirePHP/0.3",
            "Mozilla/5.0 (Linux; U; Android 1.6; en-us; HTC_TATTOO_A3288 Build/DRC79) AppleWebKit/528.5  (KHTML, like Gecko) Version/3.1.2 Mobile Safari/525.20.1",
            "Mozilla/5.0 (Linux; U; Android 1.0; en-us; dream) AppleWebKit/525.10  (KHTML, like Gecko) Version/3.0.4 Mobile Safari/523.12.2",
            "Mozilla/5.0 (Linux; U; Android 1.5; en-us; T-Mobile G1 Build/CRB43) AppleWebKit/528.5  (KHTML, like Gecko) Version/3.1.2 Mobile Safari 525.20.1",
            "Mozilla/5.0 (Linux; U; Android 1.5; en-gb; T-Mobile_G2_Touch Build/CUPCAKE) AppleWebKit/528.5  (KHTML, like Gecko) Version/3.1.2 Mobile Safari/525.20.1",
            "Mozilla/5.0 (Linux; U; Android 2.0; en-us; Droid Build/ESD20) AppleWebKit/530.17 (KHTML, like Gecko) Version/4.0 Mobile Safari/530.17",
            "Mozilla/5.0 (Linux; U; Android 2.2; en-us; Droid Build/FRG22D) AppleWebKit/533.1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1",
            "Mozilla/5.0 (Linux; U; Android 2.0; en-us; Milestone Build/ SHOLS_U2_01.03.1) AppleWebKit/530.17 (KHTML, like Gecko) Version/4.0 Mobile Safari/530.17",
            "Mozilla/5.0 (Linux; U; Android 2.0.1; de-de; Milestone Build/SHOLS_U2_01.14.0) AppleWebKit/530.17 (KHTML, like Gecko) Version/4.0 Mobile Safari/530.17",
            "Mozilla/5.0 (Linux; U; Android 3.0; en-us; Xoom Build/HRI39) AppleWebKit/525.10  (KHTML, like Gecko) Version/3.0.4 Mobile Safari/523.12.2",
            "Mozilla/5.0 (Linux; U; Android 0.5; en-us) AppleWebKit/522  (KHTML, like Gecko) Safari/419.3",
            "Mozilla/5.0 (Linux; U; Android 1.1; en-gb; dream) AppleWebKit/525.10  (KHTML, like Gecko) Version/3.0.4 Mobile Safari/523.12.2",
            "Mozilla/5.0 (Linux; U; Android 2.0; en-us; Droid Build/ESD20) AppleWebKit/530.17 (KHTML, like Gecko) Version/4.0 Mobile Safari/530.17",
            "Mozilla/5.0 (Linux; U; Android 2.1; en-us; Nexus One Build/ERD62) AppleWebKit/530.17 (KHTML, like Gecko) Version/4.0 Mobile Safari/530.17",
            "Mozilla/5.0 (Linux; U; Android 2.2; en-us; Sprint APA9292KT Build/FRF91) AppleWebKit/533.1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1",
            "Mozilla/5.0 (Linux; U; Android 2.2; en-us; ADR6300 Build/FRF91) AppleWebKit/533.1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1",
            "Mozilla/5.0 (Linux; U; Android 2.2; en-ca; GT-P1000M Build/FROYO) AppleWebKit/533.1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1",
            "Mozilla/5.0 (Linux; U; Android 3.0.1; fr-fr; A500 Build/HRI66) AppleWebKit/534.13 (KHTML, like Gecko) Version/4.0 Safari/534.13",
            "Mozilla/5.0 (Linux; U; Android 3.0; en-us; Xoom Build/HRI39) AppleWebKit/525.10  (KHTML, like Gecko) Version/3.0.4 Mobile Safari/523.12.2",
            "Mozilla/5.0 (Linux; U; Android 1.6; es-es; SonyEricssonX10i Build/R1FA016) AppleWebKit/528.5  (KHTML, like Gecko) Version/3.1.2 Mobile Safari/525.20.1",
            "Mozilla/5.0 (Linux; U; Android 1.6; en-us; SonyEricssonX10i Build/R1AA056) AppleWebKit/528.5  (KHTML, like Gecko) Version/3.1.2 Mobile Safari/525.20.1",
        }.AsReadOnly();
        }


        /// <summary>
        /// Collection of MimeType Constants for using to avoid Typos
        /// If needed MimeTypes missing feel free to add
        /// </summary>
        public static class MimeTypes
        {
            public static string GetMimeType(string extension)
            {
                if (extension == null)
                {
                    throw new ArgumentNullException(nameof(extension));
                }

                if (!extension.StartsWith("."))
                {
                    extension = "." + extension;
                }

                //return _mappings.TryGetValue(extension, out var mime) ? mime : "application/octet-stream";

                return _mappings.Select(x => new { x.Key, x.Value }).Where(x => x.Key.ToLower() == extension.ToLower()).FirstOrDefault()?.Value ?? "application/octet-stream";
            }

            #region application/*

            public const string ApplicationForceDownload = "application/force-download";

            public const string ApplicationJson = "application/json";

            public const string ApplicationOctetStream = "application/octet-stream";

            public const string ApplicationPdf = "application/pdf";

            public const string ApplicationRssXml = "application/rss+xml";

            public const string ApplicationXWwwFormUrlencoded = "application/x-www-form-urlencoded";

            public const string ApplicationXZipCo = "application/x-zip-co";

            #endregion application/*
            
            #region image/*

            public const string ImageBmp = "image/bmp";

            public const string ImageGif = "image/gif";

            public const string ImageJpeg = "image/jpeg";

            public const string ImagePJpeg = "image/pjpeg";

            public const string ImagePng = "image/png";

            public const string ImageTiff = "image/tiff";

            #endregion image/*
            
            #region text/*

            public const string TextCss = "text/css";

            public const string TextCsv = "text/csv";

            public const string TextJavascript = "text/javascript";

            public const string TextPlain = "text/plain";

            public const string TextXlsx = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            #endregion text/*

            #region 扩展名和contenttype的映射

            private static readonly ReadOnlyDictionary<string, string> _mappings = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>()
        {
                    {".323", "text/h323"},
                    {".3g2", "video/3gpp2"},
                    {".3gp", "video/3gpp"},
                    {".3gp2", "video/3gpp2"},
                    {".3gpp", "video/3gpp"},
                    {".7z", "application/x-7z-compressed"},
                    {".aa", "audio/audible"},
                    {".AAC", "audio/aac"},
                    {".aaf", "application/octet-stream"},
                    {".aax", "audio/vnd.audible.aax"},
                    {".ac3", "audio/ac3"},
                    {".aca", "application/octet-stream"},
                    {".accda", "application/msaccess.addin"},
                    {".accdb", "application/msaccess"},
                    {".accdc", "application/msaccess.cab"},
                    {".accde", "application/msaccess"},
                    {".accdr", "application/msaccess.runtime"},
                    {".accdt", "application/msaccess"},
                    {".accdw", "application/msaccess.webapplication"},
                    {".accft", "application/msaccess.ftemplate"},
                    {".acx", "application/internet-property-stream"},
                    {".AddIn", "text/xml"},
                    {".ade", "application/msaccess"},
                    {".adobebridge", "application/x-bridge-url"},
                    {".adp", "application/msaccess"},
                    {".ADT", "audio/vnd.dlna.adts"},
                    {".ADTS", "audio/aac"},
                    {".afm", "application/octet-stream"},
                    {".ai", "application/postscript"},
                    {".aif", "audio/x-aiff"},
                    {".aifc", "audio/aiff"},
                    {".aiff", "audio/aiff"},
                    {".air", "application/vnd.adobe.air-application-installer-package+zip"},
                    {".amc", "application/x-mpeg"},
                    {".application", "application/x-ms-application"},
                    {".art", "image/x-jg"},
                    {".asa", "application/xml"},
                    {".asax", "application/xml"},
                    {".ascx", "application/xml"},
                    {".asd", "application/octet-stream"},
                    {".asf", "video/x-ms-asf"},
                    {".ashx", "application/xml"},
                    {".asi", "application/octet-stream"},
                    {".asm", "text/plain"},
                    {".asmx", "application/xml"},
                    {".aspx", "application/xml"},
                    {".asr", "video/x-ms-asf"},
                    {".asx", "video/x-ms-asf"},
                    {".atom", "application/atom+xml"},
                    {".au", "audio/basic"},
                    {".avi", "video/x-msvideo"},
                    {".axs", "application/olescript"},
                    {".bas", "text/plain"},
                    {".bcpio", "application/x-bcpio"},
                    {".bin", "application/octet-stream"},
                    {".bmp", "image/bmp"},
                    {".c", "text/plain"},
                    {".cab", "application/octet-stream"},
                    {".caf", "audio/x-caf"},
                    {".calx", "application/vnd.ms-office.calx"},
                    {".cat", "application/vnd.ms-pki.seccat"},
                    {".cc", "text/plain"},
                    {".cd", "text/plain"},
                    {".cdda", "audio/aiff"},
                    {".cdf", "application/x-cdf"},
                    {".cer", "application/x-x509-ca-cert"},
                    {".chm", "application/octet-stream"},
                    {".class", "application/x-java-applet"},
                    {".clp", "application/x-msclip"},
                    {".cmx", "image/x-cmx"},
                    {".cnf", "text/plain"},
                    {".cod", "image/cis-cod"},
                    {".config", "application/xml"},
                    {".contact", "text/x-ms-contact"},
                    {".coverage", "application/xml"},
                    {".cpio", "application/x-cpio"},
                    {".cpp", "text/plain"},
                    {".crd", "application/x-mscardfile"},
                    {".crl", "application/pkix-crl"},
                    {".crt", "application/x-x509-ca-cert"},
                    {".cs", "text/plain"},
                    {".csdproj", "text/plain"},
                    {".csh", "application/x-csh"},
                    {".csproj", "text/plain"},
                    {".css", "text/css"},
                    {".csv", "text/csv"},
                    {".cur", "application/octet-stream"},
                    {".cxx", "text/plain"},
                    {".dat", "application/octet-stream"},
                    {".datasource", "application/xml"},
                    {".dbproj", "text/plain"},
                    {".dcr", "application/x-director"},
                    {".def", "text/plain"},
                    {".deploy", "application/octet-stream"},
                    {".der", "application/x-x509-ca-cert"},
                    {".dgml", "application/xml"},
                    {".dib", "image/bmp"},
                    {".dif", "video/x-dv"},
                    {".dir", "application/x-director"},
                    {".disco", "text/xml"},
                    {".dll", "application/x-msdownload"},
                    {".dll.config", "text/xml"},
                    {".dlm", "text/dlm"},
                    {".doc", "application/msword"},
                    {".docm", "application/vnd.ms-word.document.macroEnabled.12"},
                    {".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
                    {".dot", "application/msword"},
                    {".dotm", "application/vnd.ms-word.template.macroEnabled.12"},
                    {".dotx", "application/vnd.openxmlformats-officedocument.wordprocessingml.template"},
                    {".dsp", "application/octet-stream"},
                    {".dsw", "text/plain"},
                    {".dtd", "text/xml"},
                    {".dtsConfig", "text/xml"},
                    {".dv", "video/x-dv"},
                    {".dvi", "application/x-dvi"},
                    {".dwf", "drawing/x-dwf"},
                    {".dwp", "application/octet-stream"},
                    {".dxr", "application/x-director"},
                    {".eml", "message/rfc822"},
                    {".emz", "application/octet-stream"},
                    {".eot", "application/octet-stream"},
                    {".eps", "application/postscript"},
                    {".etl", "application/etl"},
                    {".etx", "text/x-setext"},
                    {".evy", "application/envoy"},
                    {".exe", "application/octet-stream"},
                    {".exe.config", "text/xml"},
                    {".fdf", "application/vnd.fdf"},
                    {".fif", "application/fractals"},
                    {".filters", "Application/xml"},
                    {".fla", "application/octet-stream"},
                    {".flr", "x-world/x-vrml"},
                    {".flv", "video/x-flv"},
                    {".fsscript", "application/fsharp-script"},
                    {".fsx", "application/fsharp-script"},
                    {".generictest", "application/xml"},
                    {".gif", "image/gif"},
                    {".group", "text/x-ms-group"},
                    {".gsm", "audio/x-gsm"},
                    {".gtar", "application/x-gtar"},
                    {".gz", "application/x-gzip"},
                    {".h", "text/plain"},
                    {".hdf", "application/x-hdf"},
                    {".hdml", "text/x-hdml"},
                    {".hhc", "application/x-oleobject"},
                    {".hhk", "application/octet-stream"},
                    {".hhp", "application/octet-stream"},
                    {".hlp", "application/winhlp"},
                    {".hpp", "text/plain"},
                    {".hqx", "application/mac-binhex40"},
                    {".hta", "application/hta"},
                    {".htc", "text/x-component"},
                    {".htm", "text/html"},
                    {".html", "text/html"},
                    {".htt", "text/webviewhtml"},
                    {".hxa", "application/xml"},
                    {".hxc", "application/xml"},
                    {".hxd", "application/octet-stream"},
                    {".hxe", "application/xml"},
                    {".hxf", "application/xml"},
                    {".hxh", "application/octet-stream"},
                    {".hxi", "application/octet-stream"},
                    {".hxk", "application/xml"},
                    {".hxq", "application/octet-stream"},
                    {".hxr", "application/octet-stream"},
                    {".hxs", "application/octet-stream"},
                    {".hxt", "text/html"},
                    {".hxv", "application/xml"},
                    {".hxw", "application/octet-stream"},
                    {".hxx", "text/plain"},
                    {".i", "text/plain"},
                    {".ico", "image/x-icon"},
                    {".ics", "application/octet-stream"},
                    {".idl", "text/plain"},
                    {".ief", "image/ief"},
                    {".iii", "application/x-iphone"},
                    {".inc", "text/plain"},
                    {".inf", "application/octet-stream"},
                    {".inl", "text/plain"},
                    {".ins", "application/x-internet-signup"},
                    {".ipa", "application/x-itunes-ipa"},
                    {".ipg", "application/x-itunes-ipg"},
                    {".ipproj", "text/plain"},
                    {".ipsw", "application/x-itunes-ipsw"},
                    {".iqy", "text/x-ms-iqy"},
                    {".isp", "application/x-internet-signup"},
                    {".ite", "application/x-itunes-ite"},
                    {".itlp", "application/x-itunes-itlp"},
                    {".itms", "application/x-itunes-itms"},
                    {".itpc", "application/x-itunes-itpc"},
                    {".IVF", "video/x-ivf"},
                    {".jar", "application/java-archive"},
                    {".java", "application/octet-stream"},
                    {".jck", "application/liquidmotion"},
                    {".jcz", "application/liquidmotion"},
                    {".jfif", "image/pjpeg"},
                    {".jnlp", "application/x-java-jnlp-file"},
                    {".jpb", "application/octet-stream"},
                    {".jpe", "image/jpeg"},
                    {".jpeg", "image/jpeg"},
                    {".jpg", "image/jpeg"},
                    {".js", "application/x-javascript"},
                    {".json", "application/json"},
                    {".jsx", "text/jscript"},
                    {".jsxbin", "text/plain"},
                    {".latex", "application/x-latex"},
                    {".library-ms", "application/windows-library+xml"},
                    {".lit", "application/x-ms-reader"},
                    {".loadtest", "application/xml"},
                    {".lpk", "application/octet-stream"},
                    {".lsf", "video/x-la-asf"},
                    {".lst", "text/plain"},
                    {".lsx", "video/x-la-asf"},
                    {".lzh", "application/octet-stream"},
                    {".m13", "application/x-msmediaview"},
                    {".m14", "application/x-msmediaview"},
                    {".m1v", "video/mpeg"},
                    {".m2t", "video/vnd.dlna.mpeg-tts"},
                    {".m2ts", "video/vnd.dlna.mpeg-tts"},
                    {".m2v", "video/mpeg"},
                    {".m3u", "audio/x-mpegurl"},
                    {".m3u8", "audio/x-mpegurl"},
                    {".m4a", "audio/m4a"},
                    {".m4b", "audio/m4b"},
                    {".m4p", "audio/m4p"},
                    {".m4r", "audio/x-m4r"},
                    {".m4v", "video/x-m4v"},
                    {".mac", "image/x-macpaint"},
                    {".mak", "text/plain"},
                    {".man", "application/x-troff-man"},
                    {".manifest", "application/x-ms-manifest"},
                    {".map", "text/plain"},
                    {".master", "application/xml"},
                    {".mda", "application/msaccess"},
                    {".mdb", "application/x-msaccess"},
                    {".mde", "application/msaccess"},
                    {".mdp", "application/octet-stream"},
                    {".me", "application/x-troff-me"},
                    {".mfp", "application/x-shockwave-flash"},
                    {".mht", "message/rfc822"},
                    {".mhtml", "message/rfc822"},
                    {".mid", "audio/mid"},
                    {".midi", "audio/mid"},
                    {".mix", "application/octet-stream"},
                    {".mk", "text/plain"},
                    {".mmf", "application/x-smaf"},
                    {".mno", "text/xml"},
                    {".mny", "application/x-msmoney"},
                    {".mod", "video/mpeg"},
                    {".mov", "video/quicktime"},
                    {".movie", "video/x-sgi-movie"},
                    {".mp2", "video/mpeg"},
                    {".mp2v", "video/mpeg"},
                    {".mp3", "audio/mpeg"},
                    {".mp4", "video/mp4"},
                    {".mp4v", "video/mp4"},
                    {".mpa", "video/mpeg"},
                    {".mpe", "video/mpeg"},
                    {".mpeg", "video/mpeg"},
                    {".mpf", "application/vnd.ms-mediapackage"},
                    {".mpg", "video/mpeg"},
                    {".mpp", "application/vnd.ms-project"},
                    {".mpv2", "video/mpeg"},
                    {".mqv", "video/quicktime"},
                    {".ms", "application/x-troff-ms"},
                    {".msi", "application/octet-stream"},
                    {".mso", "application/octet-stream"},
                    {".mts", "video/vnd.dlna.mpeg-tts"},
                    {".mtx", "application/xml"},
                    {".mvb", "application/x-msmediaview"},
                    {".mvc", "application/x-miva-compiled"},
                    {".mxp", "application/x-mmxp"},
                    {".nc", "application/x-netcdf"},
                    {".nsc", "video/x-ms-asf"},
                    {".nws", "message/rfc822"},
                    {".ocx", "application/octet-stream"},
                    {".oda", "application/oda"},
                    {".odc", "text/x-ms-odc"},
                    {".odh", "text/plain"},
                    {".odl", "text/plain"},
                    {".odp", "application/vnd.oasis.opendocument.presentation"},
                    {".ods", "application/oleobject"},
                    {".odt", "application/vnd.oasis.opendocument.text"},
                    {".one", "application/onenote"},
                    {".onea", "application/onenote"},
                    {".onepkg", "application/onenote"},
                    {".onetmp", "application/onenote"},
                    {".onetoc", "application/onenote"},
                    {".onetoc2", "application/onenote"},
                    {".orderedtest", "application/xml"},
                    {".osdx", "application/opensearchdescription+xml"},
                    {".p10", "application/pkcs10"},
                    {".p12", "application/x-pkcs12"},
                    {".p7b", "application/x-pkcs7-certificates"},
                    {".p7c", "application/pkcs7-mime"},
                    {".p7m", "application/pkcs7-mime"},
                    {".p7r", "application/x-pkcs7-certreqresp"},
                    {".p7s", "application/pkcs7-signature"},
                    {".pbm", "image/x-portable-bitmap"},
                    {".pcast", "application/x-podcast"},
                    {".pct", "image/pict"},
                    {".pcx", "application/octet-stream"},
                    {".pcz", "application/octet-stream"},
                    {".pdf", "application/pdf"},
                    {".pfb", "application/octet-stream"},
                    {".pfm", "application/octet-stream"},
                    {".pfx", "application/x-pkcs12"},
                    {".pgm", "image/x-portable-graymap"},
                    {".pic", "image/pict"},
                    {".pict", "image/pict"},
                    {".pkgdef", "text/plain"},
                    {".pkgundef", "text/plain"},
                    {".pko", "application/vnd.ms-pki.pko"},
                    {".pls", "audio/scpls"},
                    {".pma", "application/x-perfmon"},
                    {".pmc", "application/x-perfmon"},
                    {".pml", "application/x-perfmon"},
                    {".pmr", "application/x-perfmon"},
                    {".pmw", "application/x-perfmon"},
                    {".png", "image/png"},
                    {".pnm", "image/x-portable-anymap"},
                    {".pnt", "image/x-macpaint"},
                    {".pntg", "image/x-macpaint"},
                    {".pnz", "image/png"},
                    {".pot", "application/vnd.ms-powerpoint"},
                    {".potm", "application/vnd.ms-powerpoint.template.macroEnabled.12"},
                    {".potx", "application/vnd.openxmlformats-officedocument.presentationml.template"},
                    {".ppa", "application/vnd.ms-powerpoint"},
                    {".ppam", "application/vnd.ms-powerpoint.addin.macroEnabled.12"},
                    {".ppm", "image/x-portable-pixmap"},
                    {".pps", "application/vnd.ms-powerpoint"},
                    {".ppsm", "application/vnd.ms-powerpoint.slideshow.macroEnabled.12"},
                    {".ppsx", "application/vnd.openxmlformats-officedocument.presentationml.slideshow"},
                    {".ppt", "application/vnd.ms-powerpoint"},
                    {".pptm", "application/vnd.ms-powerpoint.presentation.macroEnabled.12"},
                    {".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation"},
                    {".prf", "application/pics-rules"},
                    {".prm", "application/octet-stream"},
                    {".prx", "application/octet-stream"},
                    {".ps", "application/postscript"},
                    {".psc1", "application/PowerShell"},
                    {".psd", "application/octet-stream"},
                    {".psess", "application/xml"},
                    {".psm", "application/octet-stream"},
                    {".psp", "application/octet-stream"},
                    {".pub", "application/x-mspublisher"},
                    {".pwz", "application/vnd.ms-powerpoint"},
                    {".qht", "text/x-html-insertion"},
                    {".qhtm", "text/x-html-insertion"},
                    {".qt", "video/quicktime"},
                    {".qti", "image/x-quicktime"},
                    {".qtif", "image/x-quicktime"},
                    {".qtl", "application/x-quicktimeplayer"},
                    {".qxd", "application/octet-stream"},
                    {".ra", "audio/x-pn-realaudio"},
                    {".ram", "audio/x-pn-realaudio"},
                    {".rar", "application/octet-stream"},
                    {".ras", "image/x-cmu-raster"},
                    {".rat", "application/rat-file"},
                    {".rc", "text/plain"},
                    {".rc2", "text/plain"},
                    {".rct", "text/plain"},
                    {".rdlc", "application/xml"},
                    {".resx", "application/xml"},
                    {".rf", "image/vnd.rn-realflash"},
                    {".rgb", "image/x-rgb"},
                    {".rgs", "text/plain"},
                    {".rm", "application/vnd.rn-realmedia"},
                    {".rmi", "audio/mid"},
                    {".rmp", "application/vnd.rn-rn_music_package"},
                    {".roff", "application/x-troff"},
                    {".rpm", "audio/x-pn-realaudio-plugin"},
                    {".rqy", "text/x-ms-rqy"},
                    {".rtf", "application/rtf"},
                    {".rtx", "text/richtext"},
                    {".ruleset", "application/xml"},
                    {".s", "text/plain"},
                    {".safariextz", "application/x-safari-safariextz"},
                    {".scd", "application/x-msschedule"},
                    {".sct", "text/scriptlet"},
                    {".sd2", "audio/x-sd2"},
                    {".sdp", "application/sdp"},
                    {".sea", "application/octet-stream"},
                    {".searchConnector-ms", "application/windows-search-connector+xml"},
                    {".setpay", "application/set-payment-initiation"},
                    {".setreg", "application/set-registration-initiation"},
                    {".settings", "application/xml"},
                    {".sgimb", "application/x-sgimb"},
                    {".sgml", "text/sgml"},
                    {".sh", "application/x-sh"},
                    {".shar", "application/x-shar"},
                    {".shtml", "text/html"},
                    {".sit", "application/x-stuffit"},
                    {".sitemap", "application/xml"},
                    {".skin", "application/xml"},
                    {".sldm", "application/vnd.ms-powerpoint.slide.macroEnabled.12"},
                    {".sldx", "application/vnd.openxmlformats-officedocument.presentationml.slide"},
                    {".slk", "application/vnd.ms-excel"},
                    {".sln", "text/plain"},
                    {".slupkg-ms", "application/x-ms-license"},
                    {".smd", "audio/x-smd"},
                    {".smi", "application/octet-stream"},
                    {".smx", "audio/x-smd"},
                    {".smz", "audio/x-smd"},
                    {".snd", "audio/basic"},
                    {".snippet", "application/xml"},
                    {".snp", "application/octet-stream"},
                    {".sol", "text/plain"},
                    {".sor", "text/plain"},
                    {".spc", "application/x-pkcs7-certificates"},
                    {".spl", "application/futuresplash"},
                    {".src", "application/x-wais-source"},
                    {".srf", "text/plain"},
                    {".SSISDeploymentManifest", "text/xml"},
                    {".ssm", "application/streamingmedia"},
                    {".sst", "application/vnd.ms-pki.certstore"},
                    {".stl", "application/vnd.ms-pki.stl"},
                    {".sv4cpio", "application/x-sv4cpio"},
                    {".sv4crc", "application/x-sv4crc"},
                    {".svc", "application/xml"},
                    {".swf", "application/x-shockwave-flash"},
                    {".t", "application/x-troff"},
                    {".tar", "application/x-tar"},
                    {".tcl", "application/x-tcl"},
                    {".testrunconfig", "application/xml"},
                    {".testsettings", "application/xml"},
                    {".tex", "application/x-tex"},
                    {".texi", "application/x-texinfo"},
                    {".texinfo", "application/x-texinfo"},
                    {".tgz", "application/x-compressed"},
                    {".thmx", "application/vnd.ms-officetheme"},
                    {".thn", "application/octet-stream"},
                    {".tif", "image/tiff"},
                    {".tiff", "image/tiff"},
                    {".tlh", "text/plain"},
                    {".tli", "text/plain"},
                    {".toc", "application/octet-stream"},
                    {".tr", "application/x-troff"},
                    {".trm", "application/x-msterminal"},
                    {".trx", "application/xml"},
                    {".ts", "video/vnd.dlna.mpeg-tts"},
                    {".tsv", "text/tab-separated-values"},
                    {".ttf", "application/octet-stream"},
                    {".tts", "video/vnd.dlna.mpeg-tts"},
                    {".txt", "text/plain"},
                    {".u32", "application/octet-stream"},
                    {".uls", "text/iuls"},
                    {".user", "text/plain"},
                    {".ustar", "application/x-ustar"},
                    {".vb", "text/plain"},
                    {".vbdproj", "text/plain"},
                    {".vbk", "video/mpeg"},
                    {".vbproj", "text/plain"},
                    {".vbs", "text/vbscript"},
                    {".vcf", "text/x-vcard"},
                    {".vcproj", "Application/xml"},
                    {".vcs", "text/plain"},
                    {".vcxproj", "Application/xml"},
                    {".vddproj", "text/plain"},
                    {".vdp", "text/plain"},
                    {".vdproj", "text/plain"},
                    {".vdx", "application/vnd.ms-visio.viewer"},
                    {".vml", "text/xml"},
                    {".vscontent", "application/xml"},
                    {".vsct", "text/xml"},
                    {".vsd", "application/vnd.visio"},
                    {".vsi", "application/ms-vsi"},
                    {".vsix", "application/vsix"},
                    {".vsixlangpack", "text/xml"},
                    {".vsixmanifest", "text/xml"},
                    {".vsmdi", "application/xml"},
                    {".vspscc", "text/plain"},
                    {".vss", "application/vnd.visio"},
                    {".vsscc", "text/plain"},
                    {".vssettings", "text/xml"},
                    {".vssscc", "text/plain"},
                    {".vst", "application/vnd.visio"},
                    {".vstemplate", "text/xml"},
                    {".vsto", "application/x-ms-vsto"},
                    {".vsw", "application/vnd.visio"},
                    {".vsx", "application/vnd.visio"},
                    {".vtx", "application/vnd.visio"},
                    {".wav", "audio/wav"},
                    {".wave", "audio/wav"},
                    {".wax", "audio/x-ms-wax"},
                    {".wbk", "application/msword"},
                    {".wbmp", "image/vnd.wap.wbmp"},
                    {".wcm", "application/vnd.ms-works"},
                    {".wdb", "application/vnd.ms-works"},
                    {".wdp", "image/vnd.ms-photo"},
                    {".webarchive", "application/x-safari-webarchive"},
                    {".webtest", "application/xml"},
                    {".wiq", "application/xml"},
                    {".wiz", "application/msword"},
                    {".wks", "application/vnd.ms-works"},
                    {".WLMP", "application/wlmoviemaker"},
                    {".wlpginstall", "application/x-wlpg-detect"},
                    {".wlpginstall3", "application/x-wlpg3-detect"},
                    {".wm", "video/x-ms-wm"},
                    {".wma", "audio/x-ms-wma"},
                    {".wmd", "application/x-ms-wmd"},
                    {".wmf", "application/x-msmetafile"},
                    {".wml", "text/vnd.wap.wml"},
                    {".wmlc", "application/vnd.wap.wmlc"},
                    {".wmls", "text/vnd.wap.wmlscript"},
                    {".wmlsc", "application/vnd.wap.wmlscriptc"},
                    {".wmp", "video/x-ms-wmp"},
                    {".wmv", "video/x-ms-wmv"},
                    {".wmx", "video/x-ms-wmx"},
                    {".wmz", "application/x-ms-wmz"},
                    {".wpl", "application/vnd.ms-wpl"},
                    {".wps", "application/vnd.ms-works"},
                    {".wri", "application/x-mswrite"},
                    {".wrl", "x-world/x-vrml"},
                    {".wrz", "x-world/x-vrml"},
                    {".wsc", "text/scriptlet"},
                    {".wsdl", "text/xml"},
                    {".wvx", "video/x-ms-wvx"},
                    {".x", "application/directx"},
                    {".xaf", "x-world/x-vrml"},
                    {".xaml", "application/xaml+xml"},
                    {".xap", "application/x-silverlight-app"},
                    {".xbap", "application/x-ms-xbap"},
                    {".xbm", "image/x-xbitmap"},
                    {".xdr", "text/plain"},
                    {".xht", "application/xhtml+xml"},
                    {".xhtml", "application/xhtml+xml"},
                    {".xla", "application/vnd.ms-excel"},
                    {".xlam", "application/vnd.ms-excel.addin.macroEnabled.12"},
                    {".xlc", "application/vnd.ms-excel"},
                    {".xld", "application/vnd.ms-excel"},
                    {".xlk", "application/vnd.ms-excel"},
                    {".xll", "application/vnd.ms-excel"},
                    {".xlm", "application/vnd.ms-excel"},
                    {".xls", "application/vnd.ms-excel"},
                    {".xlsb", "application/vnd.ms-excel.sheet.binary.macroEnabled.12"},
                    {".xlsm", "application/vnd.ms-excel.sheet.macroEnabled.12"},
                    {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
                    {".xlt", "application/vnd.ms-excel"},
                    {".xltm", "application/vnd.ms-excel.template.macroEnabled.12"},
                    {".xltx", "application/vnd.openxmlformats-officedocument.spreadsheetml.template"},
                    {".xlw", "application/vnd.ms-excel"},
                    {".xml", "text/xml"},
                    {".xmta", "application/xml"},
                    {".xof", "x-world/x-vrml"},
                    {".XOML", "text/plain"},
                    {".xpm", "image/x-xpixmap"},
                    {".xps", "application/vnd.ms-xpsdocument"},
                    {".xrm-ms", "text/xml"},
                    {".xsc", "application/xml"},
                    {".xsd", "text/xml"},
                    {".xsf", "text/xml"},
                    {".xsl", "text/xml"},
                    {".xslt", "text/xml"},
                    {".xsn", "application/octet-stream"},
                    {".xss", "application/xml"},
                    {".xtp", "application/octet-stream"},
                    {".xwd", "image/x-xwindowdump"},
                    {".z", "application/x-compress"},
                    {".zip", "application/x-zip-compressed"},
        });

            #endregion
        }

        public static class HeaderNames
        {
            public const string Accept = "Accept";
            public const string IfNoneMatch = "If-None-Match";
            public const string IfRange = "If-Range";
            public const string IfUnmodifiedSince = "If-Unmodified-Since";
            public const string LastModified = "Last-Modified";
            public const string Location = "Location";
            public const string MaxForwards = "Max-Forwards";
            public const string Pragma = "Pragma";
            public const string ProxyAuthenticate = "Proxy-Authenticate";
            public const string ProxyAuthorization = "Proxy-Authorization";
            public const string Range = "Range";
            public const string IfModifiedSince = "If-Modified-Since";
            public const string Referer = "Referer";
            public const string Server = "Server";
            public const string SetCookie = "Set-Cookie";
            public const string TE = "TE";
            public const string Trailer = "Trailer";
            public const string TransferEncoding = "Transfer-Encoding";
            public const string Upgrade = "Upgrade";
            public const string UserAgent = "User-Agent";
            public const string Vary = "Vary";
            public const string Via = "Via";
            public const string Warning = "Warning";
            public const string RetryAfter = "Retry-After";
            public const string WebSocketSubProtocols = "Sec-WebSocket-Protocol";
            public const string IfMatch = "If-Match";
            public const string From = "From";
            public const string AcceptCharset = "Accept-Charset";
            public const string AcceptEncoding = "Accept-Encoding";
            public const string AcceptLanguage = "Accept-Language";
            public const string AcceptRanges = "Accept-Ranges";
            public const string Age = "Age";
            public const string Allow = "Allow";
            public const string Authorization = "Authorization";
            public const string CacheControl = "Cache-Control";
            public const string Connection = "Connection";
            public const string ContentDisposition = "Content-Disposition";
            public const string Host = "Host";
            public const string ContentEncoding = "Content-Encoding";
            public const string ContentLength = "Content-Length";
            public const string ContentLocation = "Content-Location";
            public const string ContentMD5 = "Content-MD5";
            public const string ContentRange = "Content-Range";
            public const string ContentType = "Content-Type";
            public const string Cookie = "Cookie";
            public const string Date = "Date";
            public const string ETag = "ETag";
            public const string Expires = "Expires";
            public const string Expect = "Expect";
            public const string ContentLanguage = "Content-Language";
            public const string WWWAuthenticate = "WWW-Authenticate";
        }
        
        public static class ClaimTypes
        {
            //
            // 摘要:
            //     指定授权的实例实体；http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationinstant
            //     的 URI 声明。
            public const string AuthenticationInstant = "http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationinstant";
            //
            // 摘要:
            //     为实体指定拒绝安全标识符 (SID) 要求，http://schemas.xmlsoap.org/ws/2005/05/identity/claims/denyonlysid
            //     的 URI 声明。deny-only SID 禁止指定的实体访问可保护对象。
            public const string DenyOnlySid = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/denyonlysid";
            //
            // 摘要:
            //     指定实体的电子邮件地址，http://schemas.xmlsoap.org/ws/2005/05/identity/claims/email 的 URI
            //     声明。
            public const string Email = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress";
            //
            // 摘要:
            //     指定实体的性别，http://schemas.xmlsoap.org/ws/2005/05/identity/claims/gender 的 URI 声明。
            public const string Gender = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/gender";
            //
            // 摘要:
            //     指定实体的给定名称，http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname 的 URI
            //     声明。
            public const string GivenName = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname";
            //
            // 摘要:
            //     指定哈希值，http://schemas.xmlsoap.org/ws/2005/05/identity/claims/system/hash 的 URI
            //     声明。
            public const string Hash = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/hash";
            //
            // 摘要:
            //     指定实体的住宅电话号码，http://schemas.xmlsoap.org/ws/2005/05/identity/claims/homephone 的
            //     URI 声明。
            public const string HomePhone = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/homephone";
            //
            // 摘要:
            //     指定区域实体驻留，http://schemas.xmlsoap.org/ws/2005/05/identity/claims/locality 的 URI
            //     声明。
            public const string Locality = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/locality";
            //
            // 摘要:
            //     指定实体的移动电话号码，http://schemas.xmlsoap.org/ws/2005/05/identity/claims/mobilephone
            //     的 URI 声明。
            public const string MobilePhone = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/mobilephone";
            //
            // 摘要:
            //     指定实体的名称，http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name 的 URI 声明。
            public const string Name = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";
            //
            // 摘要:
            //     指定实体的名称，http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier
            //     的 URI 声明。
            public const string NameIdentifier = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
            //
            // 摘要:
            //     指定实体的备用电话号码，http://schemas.xmlsoap.org/ws/2005/05/identity/claims/otherphone
            //     的 URI 声明。
            public const string OtherPhone = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/otherphone";
            //
            // 摘要:
            //     指定实体的邮政编码，http://schemas.xmlsoap.org/ws/2005/05/identity/claims/postalcode 的
            //     URI 声明。
            public const string PostalCode = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/postalcode";
            //
            // 摘要:
            //     指定 RSA 密钥，http://schemas.xmlsoap.org/ws/2005/05/identity/claims/rsa 的 URI 声明。
            public const string Rsa = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/rsa";
            //
            // 摘要:
            //     指定安全标识符 （SID），http://schemas.xmlsoap.org/ws/2005/05/identity/claims/sid 的URI
            //     声明。
            public const string Sid = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/sid";
            //
            // 摘要:
            //     指定服务主体名称 (SPN) 声明，http://schemas.xmlsoap.org/ws/2005/05/identity/claims/spn 的
            //     URI 声明。
            public const string Spn = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/spn";
            //
            // 摘要:
            //     指定省/直辖市/自治区实体驻留，http://schemas.xmlsoap.org/ws/2005/05/identity/claims/stateorprovince
            //     的 URI 声明。
            public const string StateOrProvince = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/stateorprovince";
            //
            // 摘要:
            //     指定实体的街道地址，http://schemas.xmlsoap.org/ws/2005/05/identity/claims/streetaddress
            //     的 URI 声明。
            public const string StreetAddress = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/streetaddress";
            //
            // 摘要:
            //     指定实体的姓氏，http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname 的 URI 声明。
            public const string Surname = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname";
            //
            // 摘要:
            //     确认系统实体，http://schemas.xmlsoap.org/ws/2005/05/identity/claims/system 的声明 URI。
            public const string System = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/system";
            //
            // 摘要:
            //     指定指纹，http://schemas.xmlsoap.org/ws/2005/05/identity/claims/thumbprint 的 URI 声明。指纹是
            //     X.509 证书的全局唯一 SHA-1 哈希。
            public const string Thumbprint = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/thumbprint";
            //
            // 摘要:
            //     指定用户主体名称 (UPN)，http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn 的 URI
            //     声明。
            public const string Upn = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn";
            //
            // 摘要:
            //     指定 URI，http://schemas.xmlsoap.org/ws/2005/05/identity/claims/uri 的 URI 声明。
            public const string Uri = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/uri";
            //
            // 摘要:
            //     指定实体的网页，http://schemas.xmlsoap.org/ws/2005/05/identity/claims/webpage 的 URI 声明。
            public const string Webpage = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/webpage";
            //
            // 摘要:
            //     获取声明的 URI，该 URI 指定与计算机名称关联的 DNS 名称或者与 X.509 证书的使用者或颁发者的备用名称关联的 DNS 名称，http://schemas.xmlsoap.org/ws/2005/05/identity/claims/dns
            //     。
            public const string Dns = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/dns";
            //
            // 摘要:
            //     指定实体的备用电话号码，http://schemas.xmlsoap.org/ws/2005/05/identity/claims/otherphone
            //     URI 声明。
            public const string DateOfBirth = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/dateofbirth";
            //
            // 摘要:
            //     指定对于国家/地区实体驻留，http://schemas.xmlsoap.org/ws/2005/05/identity/claims/authorizationdecision
            //     的 URI 声明。
            public const string Country = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/country";
            //
            // 摘要:
            //     指定对于实体的授权决定，http://schemas.xmlsoap.org/ws/2005/05/identity/claims/authorizationdecision
            //     的 URI 声明。
            public const string AuthorizationDecision = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/authorizationdecision";
            //
            // 摘要:
            //     指定授权的实体方法；http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod
            //     的 URI 声明。
            public const string AuthenticationMethod = "http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod";
            //
            // 摘要:
            //     指定 cookie 路径；http://schemas.microsoft.com/ws/2008/06/identity/claims/cookiepath
            //     的 URI 声明。
            public const string CookiePath = "http://schemas.microsoft.com/ws/2008/06/identity/claims/cookiepath";
            //
            // 摘要:
            //     指定 deny-only 主要 SID 的实体；http://schemas.microsoft.com/ws/2008/06/identity/claims/denyonlyprimarysid
            //     的 URI 声明。deny-only SID 禁止指定的实体访问可保护对象。
            public const string DenyOnlyPrimarySid = "http://schemas.microsoft.com/ws/2008/06/identity/claims/denyonlyprimarysid";
            //
            // 摘要:
            //     指定 deny-only 主要团队 SID 的实体；http://schemas.microsoft.com/ws/2008/06/identity/claims/denyonlyprimarygroupsid
            //     的 URI 声明。deny-only SID 禁止指定的实体访问可保护对象。
            public const string DenyOnlyPrimaryGroupSid = "http://schemas.microsoft.com/ws/2008/06/identity/claims/denyonlyprimarygroupsid";
            //
            // 摘要:
            //     http://schemas.microsoft.com/ws/2008/06/identity/claims/denyonlywindowsdevicegroup.
            public const string DenyOnlyWindowsDeviceGroup = "http://schemas.microsoft.com/ws/2008/06/identity/claims/denyonlywindowsdevicegroup";
            //
            // 摘要:
            //     http://schemas.microsoft.com/ws/2008/06/identity/claims/dsa。
            public const string Dsa = "http://schemas.microsoft.com/ws/2008/06/identity/claims/dsa";
            //
            // 摘要:
            //     http://schemas.microsoft.com/ws/2008/06/identity/claims/expiration。
            public const string Expiration = "http://schemas.microsoft.com/ws/2008/06/identity/claims/expiration";
            //
            // 摘要:
            //     http://schemas.microsoft.com/ws/2008/06/identity/claims/expired。
            public const string Expired = "http://schemas.microsoft.com/ws/2008/06/identity/claims/expired";
            //
            // 摘要:
            //     为团队指定 SID 的实体，http://schemas.microsoft.com/ws/2008/06/identity/claims/groupsid
            //     的 URI 声明。
            public const string GroupSid = "http://schemas.microsoft.com/ws/2008/06/identity/claims/groupsid";
            //
            // 摘要:
            //     http://schemas.microsoft.com/ws/2008/06/identity/claims/ispersistent。
            public const string IsPersistent = "http://schemas.microsoft.com/ws/2008/06/identity/claims/ispersistent";
            //
            // 摘要:
            //     指定实体主要团队 SID，http://schemas.microsoft.com/ws/2008/06/identity/claims/primarygroupsid
            //     的 URI 声明。
            public const string PrimaryGroupSid = "http://schemas.microsoft.com/ws/2008/06/identity/claims/primarygroupsid";
            //
            // 摘要:
            //     X.509 证书的识别名名称，http://schemas.xmlsoap.org/ws/2005/05/identity/claims/x500distinguishedname
            //     的 URI 声明。X.500 标准规定了用于定义 X.509 证书所使用的可分辨名称的方法。
            public const string X500DistinguishedName = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/x500distinguishedname";
            //
            // 摘要:
            //     指定实体的主要 SID，http://schemas.microsoft.com/ws/2008/06/identity/claims/primarysid
            //     的 URI 声明。
            public const string PrimarySid = "http://schemas.microsoft.com/ws/2008/06/identity/claims/primarysid";
            //
            // 摘要:
            //     指定序列号，http://schemas.microsoft.com/ws/2008/06/identity/claims/serialnumber 的
            //     URI 声明。
            public const string SerialNumber = "http://schemas.microsoft.com/ws/2008/06/identity/claims/serialnumber";
            //
            // 摘要:
            //     http://schemas.microsoft.com/ws/2008/06/identity/claims/userdata。
            public const string UserData = "http://schemas.microsoft.com/ws/2008/06/identity/claims/userdata";
            //
            // 摘要:
            //     http://schemas.microsoft.com/ws/2008/06/identity/claims/version。
            public const string Version = "http://schemas.microsoft.com/ws/2008/06/identity/claims/version";
            //
            // 摘要:
            //     指定 Windows 域帐户名，http://schemas.microsoft.com/ws/2008/06/identity/claims/windowsaccountname
            //     的 URI 声明。
            public const string WindowsAccountName = "http://schemas.microsoft.com/ws/2008/06/identity/claims/windowsaccountname";
            //
            // 摘要:
            //     http://schemas.microsoft.com/ws/2008/06/identity/claims/windowsdeviceclaim。
            public const string WindowsDeviceClaim = "http://schemas.microsoft.com/ws/2008/06/identity/claims/windowsdeviceclaim";
            //
            // 摘要:
            //     http://schemas.microsoft.com/ws/2008/06/identity/claims/windowsdevicegroup.
            public const string WindowsDeviceGroup = "http://schemas.microsoft.com/ws/2008/06/identity/claims/windowsdevicegroup";
            //
            // 摘要:
            //     http://schemas.microsoft.com/ws/2008/06/identity/claims/windowsuserclaim。
            public const string WindowsUserClaim = "http://schemas.microsoft.com/ws/2008/06/identity/claims/windowsuserclaim";
            //
            // 摘要:
            //     http://schemas.microsoft.com/ws/2008/06/identity/claims/windowsfqbnversion。
            public const string WindowsFqbnVersion = "http://schemas.microsoft.com/ws/2008/06/identity/claims/windowsfqbnversion";
            //
            // 摘要:
            //     http://schemas.microsoft.com/ws/2008/06/identity/claims/windowssubauthority。
            public const string WindowsSubAuthority = "http://schemas.microsoft.com/ws/2008/06/identity/claims/windowssubauthority";
            //
            // 摘要:
            //     指定用户主体名称 (UPN)；http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn 的 URI
            //     声明。
            public const string Anonymous = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/anonymous";
            //
            // 摘要:
            //     指定特定有关标识是否已授权的细节，http://schemas.xmlsoap.org/ws/2005/05/identity/claims/authenticated
            //     的 URI 声明。
            public const string Authentication = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/authentication";
            //
            // 摘要:
            //     指定实体的角色，http://schemas.microsoft.com/ws/2008/06/identity/claims/role 的 URI 声明。
            public const string Role = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
            //
            // 摘要:
            //     http://schemas.xmlsoap.org/ws/2009/09/identity/claims/actor。
            public const string Actor = "http://schemas.xmlsoap.org/ws/2009/09/identity/claims/actor";
        }
    }
}
