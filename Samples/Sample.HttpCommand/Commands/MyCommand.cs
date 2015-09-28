using Jellyfish.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.HttpCommand.Commands
{
    class MyCommand : ServiceCommand<string>
    {
        private string _id;

        public MyCommand(IJellyfishContext ctx, string id) 
            : base(ctx, "MyCommand")
        {
            _id = id;
        }

        protected override Task<string> GetFallback()
        {
            return Task.FromResult("nothing");
        }

        protected override async Task<string> Run(CancellationToken token)
        {
            await Task.Delay(200, token);

            // Simple http request
            var response = await Jellyfish.Commands.Http.HttpClientBuilder.Create(new Uri("https://www.myget.org/F/jellyfish/api/v2"), null)
                            .ExecuteAsync("/FindPackagesById()?id='" + _id + "'", token);

            return response.Content;
        }
    }
}
