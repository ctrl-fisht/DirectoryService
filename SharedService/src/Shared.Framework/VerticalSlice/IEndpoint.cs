using Microsoft.AspNetCore.Routing;

namespace Shared.Framework.VerticalSlice;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}