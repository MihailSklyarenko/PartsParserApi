using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using PartsParserApi.Models;
using HtmlAgilityPack;
using System.Net.Http;
using System.Text.RegularExpressions;
using AngleSharp.Html.Parser;
using AngleSharp.Dom;

namespace PartsParserApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParserController : ControllerBase
    {
        public string URL { get; private set; } = "https://www.avtoall.ru/catalog/paz-20/avtobusy-36/paz_672m-393/";
        private HttpClient httpClient;
        private HtmlParser parser;
        TreeNodeContext db;

        public ParserController(TreeNodeContext context)
        {
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:83.0) Gecko/20100101 Firefox/83.0");
            db = context;
            parser = new HtmlParser();
        }

        [HttpGet]
        public List<TreeNode> Get()
        {
            List<TreeNode> parsedTree = ParseTree();

            foreach (var item in parsedTree)
            {
                db.TreeNodes.Add(item);
            }
            db.SaveChanges();
            return parsedTree;
        }

        private List<TreeNode> ParseTree()
        {
            List<TreeNode> result = new List<TreeNode>();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(GetHTML());
            var MainTree = doc.DocumentNode.SelectSingleNode("//*[@id=\"autoparts_tree\"]");
            var nodesCount = MainTree.ChildNodes[1].ChildNodes.Count;
            for (int i = 1; i < nodesCount; i++)
            {
                var firstNode = MainTree.ChildNodes[1].ChildNodes[i];
                string firstNodeName = firstNode.ChildNodes[1].InnerText;
                TreeNode firstTreeNode = new TreeNode();
                firstTreeNode.Text = firstNodeName;

                for (int j = 0; j < firstNode.ChildNodes[3].ChildNodes.Count; j++)
                {
                    var secondNode = firstNode.ChildNodes[3].ChildNodes[j];
                    string secondNodeName = secondNode.ChildNodes[1].InnerText;
                    TreeNode secondTreeNode = new TreeNode();
                    secondTreeNode.Text = secondNodeName;
                    secondTreeNode.Parent = firstTreeNode;

                    for (int k = 0; k < secondNode.ChildNodes[3].ChildNodes.Count; k++)
                    {
                        var thirdNode = secondNode.ChildNodes[3].ChildNodes[k];
                        string thirdNodeName = thirdNode.ChildNodes[1].InnerText;
                        var link = thirdNode.ChildNodes[1].Attributes[1].Value;
                        TreeNode thirdTreeNode = new TreeNode();
                        thirdTreeNode.Text = thirdNodeName;
                        thirdTreeNode.Sections = ParseDetailsInSection(thirdTreeNode, link);                        
                        thirdTreeNode.Parent = secondTreeNode;
                        secondTreeNode.Nodes.Add(thirdTreeNode);
                    }
                    firstTreeNode.Nodes.Add(secondTreeNode);                    
                }
                result.Add(firstTreeNode);



                //if (result.Count >= 1) return result; //убрать в релизе
            }
            return result;
        }

        private string GetHTML()
        {
            return httpClient.GetStringAsync(URL).Result;
        }

        private List<Section> ParseDetailsInSection(TreeNode treeNode, string linkToSection)
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
                    section.Available = true;
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
                section.Available = false;
                section.ParentNode = treeNode;
                result.Add(section);
            }
            return result;
        }
    }    
}
