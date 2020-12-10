using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartsParserApi.Models
{
    public class SectionContext : DbContext
    {
        public DbSet<Section> Sections { get; set; }
        public SectionContext(DbContextOptions<SectionContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
