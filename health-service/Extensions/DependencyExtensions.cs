using System;
using System.Collections.Generic;
using health_service.Models;
using Microsoft.Extensions.DependencyInjection;

namespace health_service.Extensions
{
    public static class DependenciesExtensions
    {
        public static IHealthChecksBuilder AddDependencies(
            this IHealthChecksBuilder builder,
            List<Dependency> dependencies)
        {
            foreach(var dep in dependencies)
            {
                string dep_name = dep.Name.ToLower();

                if(dep_name.StartsWith("url-"))
                    builder = builder.AddUrlGroup(new Uri(dep.Url), name: dep.Name);
            }

            return builder;
        }
    }
}