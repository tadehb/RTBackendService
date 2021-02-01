using Infrastructure.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace WebApi.Configs
{
    public static class DiWrapper
    {

        public static void DiWrapperConf(this IServiceCollection services, IConfiguration configuration)
        {
            Assembly[] assemblies = configuration.GetSection("Service:Assemblies")
                .GetChildren()
                .AsEnumerable()
                .Select(x => $"{x.Value}.dll")
                .Select(x => Directory.GetFiles(Directory.GetCurrentDirectory(), x).Single())
                .Select(x => Assembly.LoadFrom(x))
                .ToArray();

            Parallel.ForEach(assemblies, assembly =>
            {
                Type[] singles = assembly.GetTypes().Where(x => x.GetInterfaces().Contains(typeof(ISingleton))).ToArray();
                Type[] scopes = assembly.GetTypes().Where(x => x.GetInterfaces().Contains(typeof(IScoped))).ToArray();
                Type[] transients = assembly.GetTypes().Where(x => x.GetInterfaces().Contains(typeof(ITransient))).ToArray();

                Parallel.ForEach(singles, single => services.AddSingleton(single));
                Parallel.ForEach(scopes, scope => services.AddScoped(scope));
                Parallel.ForEach(transients, transient => services.AddTransient(transient));
            });
        }
    }
}
