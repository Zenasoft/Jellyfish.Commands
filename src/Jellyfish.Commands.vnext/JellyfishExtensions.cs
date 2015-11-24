using Microsoft.AspNet.Builder;
using Jellyfish.Commands;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Framework.DependencyInjection
{
    public static class JellyfishExtensions
    {
        public static IApplicationBuilder UseJellyfish(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<EventStreamHandler>();
            var ctx = builder.ApplicationServices.GetService<IJellyfishContext>();
            InitializeServices( ctx, Assembly.GetCallingAssembly() );
            return builder;
        }

        public static void AddJellyfish(this IServiceCollection services)
        {
            services.AddScoped<IJellyfishContext, JellyfishContext>();
        }

        private static void InitializeServices(IJellyfishContext ctx, Assembly asm)
        {
            foreach(var t in asm.GetTypes())
            {
                if(!typeof( IServiceCommandInfo ).IsAssignableFrom( t ))
                    continue;

                var attr = t.GetCustomAttributes<CommandAttribute>( true ).FirstOrDefault();                
                ServiceCommandHelper.PrepareInternal( t, ctx, attr?.CommandGroup, attr?.CommandName );
            }
        }
    }
}