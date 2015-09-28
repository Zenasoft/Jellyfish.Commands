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
    public class ValuesController : Controller
    {
        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<string> Get(string id)
        {
            var ctx = (IJellyfishContext)this.Context.RequestServices.GetService(typeof(IJellyfishContext));

            var cmd = new MyCommand(ctx, id);
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
