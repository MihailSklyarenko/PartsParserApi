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
        SectionContext db;
        public GetPartsController(SectionContext context)
        {
            db = context;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Section>> Get(int id)
        {
            Section section = await db.Sections.FirstOrDefaultAsync(x => x.ID == id);
            if (section == null)
                return null;
            return new ObjectResult(section);
        }
    }
}
