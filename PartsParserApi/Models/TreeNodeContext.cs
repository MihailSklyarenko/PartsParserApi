using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartsParserApi.Models
{
    public class TreeNodeContext : DbContext
    {
        public DbSet<TreeNode> TreeNodes { get; set; }
        public DbSet<Section> Sections { get; set; }
        public TreeNodeContext(DbContextOptions<TreeNodeContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
