using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PartsParserApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartsParserApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GetPartsController : ControllerBase
    {
        TreeNodeContext db;
        public GetPartsController(TreeNodeContext context)
        {
            db = context;
        }

        public List<TreeNode> Get()
        {
            List<TreeNode> treeNodes = db.TreeNodes.Where(x => x.Parent == null).Select(x => x).ToList();
            if (treeNodes == null)
                return null;
            return treeNodes;
        }

        [HttpGet("{id}")]
        public List<TreeNode> Get(int id)
        {
            List<TreeNode> treeNodes = db.TreeNodes.Where(x => x.Parent.ID == id ).Select(x => x).ToList();
            if (treeNodes == null)
                return null;
            return treeNodes;
        }

        [HttpGet("{id}/{id2}")]
        public List<TreeNode> Get(int id, int id2)
        {
            List<TreeNode> treeNodes = db.TreeNodes.Where(x => x.Parent.ID == id2).ToList();

            if (treeNodes == null)
                return null;
            return treeNodes;
        }

        [HttpGet("{id}/{id2}/{id3}")]
        public List<Section> Get(int id,int id2, int id3)
        {
            var res = db.Sections.Where(x => x.ID == id3);

            List<Section> sections = db.Sections
                .Where(x => x.ParentNode.ID == id3)                
                .Select(x => x).ToList();
            if (sections == null)
                return null;
            return sections;
        }
    }
}