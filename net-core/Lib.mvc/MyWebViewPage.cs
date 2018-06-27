using Lib.helper;
using System.Collections.Generic;
using System.Linq;

namespace Lib.mvc
{
    public abstract class MyWebViewPage : MyWebViewPage<dynamic> { }

    /// <summary>
    /// 自定义模板引擎，在这里实现多语言等功能
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class MyWebViewPage<T> : Microsoft.AspNetCore.Mvc.Razor.RazorPage<T>
    {
        public MyWebViewPage()
        {
            //
        }

        #region 方法
        public void SetTitle(string s)
        {
            ViewData["title"] = s;
        }
        public void SetKeywords(string s)
        {
            ViewData["keywords"] = s;
        }
        public void SetDescription(string s)
        {
            ViewData["description"] = s;
        }
        public string IncludePath(string file_name)
        {
            return $"~/Views/Shared/{file_name}.cshtml";
        }

        /// <summary>
        /// 获取非空对象
        /// </summary>
        public M GetModel<M>(string key) where M : new() => Com.NewIfNull<M>(key);

        /// <summary>
        /// 设置模板
        /// </summary>
        public void SetLayout(string name)
        {
            if (!ValidateHelper.IsPlumpString(name))
            {
                this.Layout = null;
                return;
            }
            this.Layout = IncludePath(name);
        }

        /// <summary>
        /// 从ViewData中拿到非空数据
        /// </summary>
        public DT GetNotNullViewData<DT>(string key) where DT : new() => Com.NewIfNull<DT>(ViewData[key]);
        #endregion

        protected List<LangModel> Language { get; set; }

        protected void LoadLangResource()
        {
            if (Language == null)
            {
                //只加载一次
                this.Language = LanguageHelper.LoadAndCacheLanguages();
                if (this.Language == null) { this.Language = new List<LangModel>(); }
            }
        }
        /// <summary>
        /// 多语言
        /// </summary>
        /// <param name="key"></param>
        /// <param name="deft"></param>
        /// <returns></returns>
        protected string Lang(string key, string deft = null)
        {
            LoadLangResource();
            var context = this.Context;

            LangModel cur_lang = null;

            var cookie_lang = context.Request.Cookies.GetCookie_(LanguageHelper.CookieName);

            if (ValidateHelper.IsPlumpString(cookie_lang))
            {
                cur_lang = this.Language.Where(x => x.Name == cookie_lang).FirstOrDefault();
            }
            if (cur_lang == null)
            {
                cur_lang = this.Language.Where(x => x.Default).FirstOrDefault();
                context.Response.Cookies.Delete(LanguageHelper.CookieName);
            }

            var word = cur_lang?.Dict?.Where(x => x.key == key)?.FirstOrDefault();
            if (word != null)
            {
                return word.value;
            }
            return deft;
        }
    }
}
