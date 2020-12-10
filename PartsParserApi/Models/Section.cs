using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartsParserApi.Models
{
    public class Section
    {
        public int ID { get; set; }
        public string DetailNumber { get; set; }
        public string Name { get; set; }
        public string CountPerModel { get; set; }
        public string DetailPicturePatch { get; set; }
        public string SectionPicturePatch { get; set; }
        public string Price { get; set; }
    }
}
