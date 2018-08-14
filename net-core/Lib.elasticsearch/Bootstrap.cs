using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lib.elasticsearch
{
    public static class ESBootstrap
    {
        public static IServiceCollection UseElasticsearch(this IServiceCollection collection, 
            string servers, bool debug = false)
        {
            return collection;
        }
    }
}
