using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartsParserApi.Models
{
    public class TreeNode
    {
        public int ID { get; set; }
        public string Text { get; set; }
        public TreeNode Parent { get; set; } = null;
        public List<TreeNode> Nodes { get; set; } = new List<TreeNode>();
        public List<Section> Sections { get; set; } = new List<Section>();

        public override string ToString()
        {
            return Text;
        }
    }
}
