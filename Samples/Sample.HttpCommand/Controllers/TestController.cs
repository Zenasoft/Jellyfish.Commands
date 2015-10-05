using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Jellyfish.Commands;
using Sample.HttpCommand.Commands;

namespace Sample.HttpCommand.Controllers
{
    [Route("api/[controller]")]
    public class TestController : Controller
    {
        private IJellyfishContext _context;

        public TestController(IJellyfishContext ctx)
        {
            _context = ctx;
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<string> Get(string id)
        {
            var cmd = new MyCommand(_context, id);
            try
            {
                return await cmd.ExecuteAsync();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
