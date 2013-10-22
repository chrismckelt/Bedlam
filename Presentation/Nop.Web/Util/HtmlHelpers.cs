using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Reflection;
using System.Security.Policy;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

using System.IO;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Seo;
using Nop.Web.Models.Catalog;
using Nop.Web.Util;

namespace System.Web
{
    public static class HtmlHelpers
    {

        public static MvcHtmlString GetVersionedLink(this HtmlHelper htmlHelper, string url)
        {
            return GetVersionedLink(htmlHelper, url, "stylesheet", "text/css");
        }

        public static MvcHtmlString GetVersionedLink(this HtmlHelper htmlHelper, string url, string rel, string type)
        {
            return new MvcHtmlString(string.Format("<link href=\"{0}?ver={1}\" rel=\"{2}\" type=\"{3}\" />",
                url, Assembly.GetExecutingAssembly().GetName().Version, rel, type));
        }

        public static MvcHtmlString GetVersionedScript(this HtmlHelper htmlHelper, string url)
        {
            return GetVersionedScript(htmlHelper, url, "text/javascript");
        }

        public static MvcHtmlString GetVersionedScript(this HtmlHelper htmlHelper, string url, string type)
        {
            return new MvcHtmlString(string.Format("<script src=\"{0}?ver={1}\" type=\"{2}\"></script>",
                url, Assembly.GetExecutingAssembly().GetName().Version, type));
        }

        /// <summary>
        /// Custom version of ValidationMessageFor which prepends the error image.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="htmlHelper"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static MvcHtmlString ValidationMessageWithImageFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
        {
            MvcHtmlString message = htmlHelper.ValidationMessageFor(expression);
            if (message != null)
            {
                TagBuilder tag = new TagBuilder("img");
                tag.Attributes.Add("src", GetErrorIconUrl(htmlHelper).ToString());
                tag.Attributes.Add("alt", "Error");
                tag.Attributes.Add("title", ExpressionHelper.GetExpressionText(expression));
                string img = tag.ToString(TagRenderMode.SelfClosing);
                return new MvcHtmlString(Regex.Replace(message.ToHtmlString()
                    , "<span([^>]*)>([^<]*)</span>", "<span$1>" + img + "&nbsp$2</span>"));
            }
            return message;
        }

        public static MvcHtmlString ValidationMessageWithImage(this HtmlHelper helper, string name)
        {
            if (helper.ViewData.ModelState[name] == null || helper.ViewData.ModelState[name].Errors == null)
            {
                return new MvcHtmlString(string.Empty);
            }

            TagBuilder tag = new TagBuilder("img");

            tag.Attributes.Add("src", GetErrorIconUrl(helper).ToString());
            tag.Attributes.Add("alt", "Error");
            tag.Attributes.Add("title", helper.ViewData.ModelState[name].Errors[0].ErrorMessage);
            string img = tag.ToString(TagRenderMode.SelfClosing);
            string error = string.Format("<span class='error'>{0}&nbsp;{1}</span>", img, helper.ViewData.ModelState[name].Errors[0].ErrorMessage);
            return new MvcHtmlString(error);
        }

        public static MvcHtmlString GetErrorIconUrl(this HtmlHelper helper)
        {
            Uri requestUri = helper.ViewContext.HttpContext.Request.Url;
            string baseUrl = requestUri.Scheme + Uri.SchemeDelimiter + requestUri.Host + (requestUri.IsDefaultPort ? "" : ":" + requestUri.Port);

            string url = string.Format("{0}/{1}", baseUrl,
                                       "content/images/form/erroricon.png");

            return new MvcHtmlString(url);
        }

        public static string GetValueOrDefault(string val, string textForMissingValue)
        {
            if (!string.IsNullOrWhiteSpace(val))
                return (val);

            return (textForMissingValue);
        }

        public static MvcHtmlString GetExternalContent(this HtmlHelper helper, string filename)
        {
            return new MvcHtmlString(File.ReadAllText(HttpContext.Current.Server.MapPath("~/content/" + filename)));
        }

        public static string Show(bool show)
        {
            return show ? string.Empty : "display:none;";
        }

        public static string Capitalise(string word)
        {
            return System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(word.ToLower());

        }

        private static string CATEGORIES_CACHE_KEY = "CachedCategories";

         public static IList<BedlamModel> GetParentCategoryModels()
         {
             var categoryService = EngineContext.Current.Resolve<ICategoryService>();
             IList<Category> cats;

             if (HttpRuntime.Cache[CATEGORIES_CACHE_KEY] != null)
             {
                 cats = HttpRuntime.Cache[CATEGORIES_CACHE_KEY] as List<Category>;
                 if (!cats.Any())
                 {
                     cats = categoryService.GetAllCategories(string.Empty, 0, 1000, true) as IPagedList<Category>;
                     HttpRuntime.Cache[CATEGORIES_CACHE_KEY] = cats;
                 }

             }
             else
             {
                 cats = categoryService.GetAllCategories(string.Empty, 0, 1000, true) as IPagedList<Category>;
                 HttpRuntime.Cache[CATEGORIES_CACHE_KEY] = cats;
             }
             if (cats != null) cats = cats.Where(a => a.ParentCategoryId == 0).ToList();
             return cats.Select(category => new BedlamModel()
             {
                 Id = category.Id,
                 Name = category.Name,
                 SeName = category.GetSeName(),
                 ParentCategoryId = category.ParentCategoryId
             }).ToList();
         }

        public static IList<BedlamModel> GetCategoryModels()
        {
            var categoryService = EngineContext.Current.Resolve<ICategoryService>();
            IList<Category> cats;

            if (HttpRuntime.Cache[CATEGORIES_CACHE_KEY] != null)
            {
                cats = HttpRuntime.Cache[CATEGORIES_CACHE_KEY] as IPagedList<Category>;
            }
            else
            {
                cats = categoryService.GetAllCategories(string.Empty, 0, 1000, true) as IPagedList<Category>;
                HttpRuntime.Cache[CATEGORIES_CACHE_KEY] = cats;
            }

            return cats.Select(category => new BedlamModel()
                                               {
                                                   Id = category.Id, Name = category.GetLocalized(x => x.Name), SeName = category.GetSeName(), ParentCategoryId = category.ParentCategoryId
                                               }).ToList();
        }

        public static IList<BedlamModel> GetChildCategoryModels(int categoryId)
         {
             var categoryService = EngineContext.Current.Resolve<ICategoryService>();
             var cats = categoryService.GetAllCategoriesByParentCategoryId(categoryId);

             return cats.Select(category => new BedlamModel()
             {
                 Id = category.Id,
                 Name = category.GetLocalized(x => x.Name),
                 SeName = category.GetSeName(),
                 ParentCategoryId = category.ParentCategoryId
             }).ToList();
         }

        public static IList<ProductOverviewModel> GetProducts()
        {
            var overviewModels = HttpRuntime.Cache["AllProductOverviewModels"] as IList<ProductOverviewModel>;

            if (overviewModels == null)
            {
                var service = EngineContext.Current.Resolve<IProductService>();
                var products = service.GetAllProductsDisplayedOnHomePage();
                overviewModels = new List<ProductOverviewModel>();
                foreach (var product in products)
                {
                    var model = new ProductOverviewModel()
                        {
                            Id = product.Id,
                            Name = product.GetLocalized(x => x.Name),
                            ShortDescription = product.GetLocalized(x => x.ShortDescription),
                            FullDescription = product.GetLocalized(x => x.FullDescription),
                            SeName = product.GetSeName(),
                        };

                    overviewModels.Add(model);
                }

                HttpRuntime.Cache["AllProductOverviewModels"] = overviewModels;
            }

            return overviewModels;

        }

        public static Product GetProduct(int productId)
        {
            var products = HttpRuntime.Cache["AllProducts"] as IList<Product>;

           
            if (products == null)
            {
                Task.Run(() =>
                    {
                        var service = EngineContext.Current.Resolve<IProductService>();
                        products = service.GetAllProducts();
                        HttpRuntime.Cache["AllProducts"] = products;

                    });

                return EngineContext.Current.Resolve<IProductService>().GetProductById(productId);
            }

            return products.SingleOrDefault(a=>a.Id == productId);

        }

        public static Category GetCategory(int categoryId)
        {
            var categoryService = EngineContext.Current.Resolve<ICategoryService>();
            IList<Category> cats;

            if (HttpRuntime.Cache[CATEGORIES_CACHE_KEY] != null)
            {
                cats = HttpRuntime.Cache[CATEGORIES_CACHE_KEY] as List<Category>;
            }
            else
            {
                cats = categoryService.GetAllCategories() as List<Category>;
                HttpRuntime.Cache[CATEGORIES_CACHE_KEY] = cats;
            }

            return cats != null ? cats.FirstOrDefault(a => a.Id == categoryId) : null;
        }

        public static string GetCssClass(int currentCategoryId, int id, string css)
        {
            if (currentCategoryId == id)
                return css;

            return string.Empty;
        }
    }
}