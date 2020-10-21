using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Honeycomb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Honeycomb.AspNetCore.Middleware
{
    public class HoneycombMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IOptions<HoneycombApiSettings> _settings;
        private readonly IHoneycombEventScopeManager _scopeManager;

        public HoneycombMiddleware(RequestDelegate next,
            IOptions<HoneycombApiSettings> settings,
            IHoneycombEventScopeManager scopeManager)
        {
            _next = next;
            _settings = settings;
            _scopeManager = scopeManager;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            using var scope = _scopeManager.CreateScope(out var ev);

            ev.DataSetName = _settings.Value.DefaultDataSet;

            // TODO: `Activity.Current.Id` might be better here? Or, say _settings.Value.GetTraceIdFrom = ...
            // https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.activity?view=netcore-3.1
            ev.Data.Add("trace.trace_id", context.TraceIdentifier);
            ev.Data.Add("request.path", context.Request.Path.Value);
            ev.Data.Add("request.method", context.Request.Method);
            ev.Data.Add("request.http_version", context.Request.Protocol);
            ev.Data.Add("request.content_length", context.Request.ContentLength);
            ev.Data.Add("request.header.x_forwarded_proto", context.Request.Scheme);
            ev.Data.Add("meta.local_hostname", Environment.MachineName);

            try
            {
                await _next.Invoke(context);

                ev.Data.TryAdd("name", $"{context.GetRouteValue("controller")}#{context.GetRouteValue("action")}");
                ev.Data.TryAdd("action", context.GetRouteValue("action"));
                ev.Data.TryAdd("controller", context.GetRouteValue("controller"));
                ev.Data.TryAdd("response.content_length", context.Response.ContentLength);
                ev.Data.TryAdd("response.status_code", context.Response.StatusCode);
            }
            catch (Exception ex)
            {
                scope.Exception(ex);

                throw;
            }
        }
    }
}
