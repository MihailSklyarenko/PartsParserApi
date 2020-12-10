using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PartsParserApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PartsParserApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ParseController : ControllerBase
    {
        public string URL { get; private set; } = "https://www.avtoall.ru/catalog/paz-20/avtobusy-36/paz_672m-393/";
        private HttpClient httpClient;
        private HtmlParser parser;
        SectionContext db;

        public ParseController(SectionContext context)
        {
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:83.0) Gecko/20100101 Firefox/83.0");
            db = context;
        }
    
        [HttpGet]
        public string Get()
        {            
            if (!db.Sections.Any())
            {
                List<List<Section>> sections = Parse();

                foreach (var item1 in sections)
                {
                    foreach (var item in item1)
                    {
                        db.Sections.Add(item);
                    }
                }
                db.SaveChanges();
            }

            return "OK";
        }

        public List<List<Section>> Parse()
        {
            string html = GetHTML();
            List<string> catalogLinks;

            parser = new HtmlParser();
            var parseResult = parser.ParseDocument(html).QuerySelectorAll("ul.catalog-groups-tree");
            catalogLinks = GetCatalogLinks(parseResult.First().InnerHtml);

            List<List<Section>> resultParseList = new List<List<Section>>();
            foreach (var link in catalogLinks)
            {
                resultParseList.Add(ParseDetailsInSection(link));
            }
            return resultParseList;
        }
        private string GetHTML()
        {
            return httpClient.GetStringAsync(URL).Result;
        }
        private List<string> GetCatalogLinks(string html)
        {
            List<string> result = new List<string>();
            foreach (Match m in Regex.Matches(html, @"<a\s*data-id=.gr[0-9]*.\s*href=.(.*?)"""))
            {
                result.Add(m.Groups[1].Value);
            }
            return result;
        }
        private List<Section> ParseDetailsInSection(string linkToSection)
        {
            List<Section> result = new List<Section>();
            URL = "https://www.avtoall.ru" + linkToSection;
            var sectHtml = GetHTML();

            var document = parser.ParseDocument(sectHtml);
            var availableParts = document.GetElementsByClassName("parts with-goods");
            var notAvailableParts = document.GetElementsByClassName("parts not-price");
            var currentSectionPicturePatch = document.All.GetElementById("picture_img").GetAttribute("src");

            if (availableParts.Count() > 0)
            {
                var availablePartsList = availableParts.First().GetElementsByClassName("item item-elem");
                foreach (var item in availablePartsList)
                {
                    Section section = new Section();
                    section.Name = item.GetElementsByClassName("item-name").First().TextContent;
                    section.CountPerModel = Regex.Match(availableParts[0].InnerHtml, @"quot;\"">([0-9]*)<").Groups[1].Value;
                    section.DetailNumber = availableParts.First().GetElementsByClassName("number")[1].TextContent;
                    section.Price = availableParts.First().GetElementsByClassName("price-internet")[0].TextContent;
                    section.SectionPicturePatch = currentSectionPicturePatch;
                    section.DetailPicturePatch = item.QuerySelectorAll("img.lazy").Select(el => el.GetAttribute("src")).First();
                    result.Add(section);
                }
            }

            var notAvailablePartsList = notAvailableParts.First().GetElementsByClassName("part");
            foreach (var item in notAvailablePartsList)
            {
                Section section = new Section();
                section.DetailNumber = item.GetElementsByClassName("number")[0].TextContent;
                section.Name = item.GetElementsByClassName("name")[0].TextContent;
                section.CountPerModel = item.GetElementsByClassName("count")[0].TextContent.Trim();
                section.SectionPicturePatch = currentSectionPicturePatch;
                result.Add(section);
            }

            return result;
        }
    }
}
