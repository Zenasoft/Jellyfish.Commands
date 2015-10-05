using Microsoft.AspNet.Builder;
using Jellyfish.Commands;

namespace Microsoft.Framework.DependencyInjection
{
    public static class JellyfishExtensions
    {
        public static IApplicationBuilder UseJellyfish(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<Jellyfish.Commands.EventStreamHandler>();
            return builder;
        }

        public static void AddJellyfish(this IServiceCollection services)
        {
            services.AddScoped<IJellyfishContext, JellyfishContext>();
        }
    }
}