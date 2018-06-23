using Microsoft.AspNetCore.Routing;

namespace Lib.mvc.plugin
{
    public interface IRouteProvider
    {
        void RegisterRoutes(RouteCollection routes);

        int Priority { get; }
    }
}
