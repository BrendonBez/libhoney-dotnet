using System;
using System.Diagnostics;
using System.Threading;
using Honeycomb.Models;
using Microsoft.AspNetCore.Http;

namespace Honeycomb.AspNetCore
{
    public interface IHoneycombEventScope : IDisposable
    {
        HoneycombEvent Event { get; }

        void Exception(Exception exception);
    }

    public interface IHoneycombEventScopeManager
    {
        IHoneycombEventScope CreateScope(out HoneycombEvent @event);

        HoneycombEvent? Current { get; }
    }

    public static class HoneycombEventScopeManagerExtensions
    {
        public static IHoneycombEventScope CreateScope(this IHoneycombEventScopeManager eventScopeManager)
        {
            return eventScopeManager.CreateScope(out _);
        }
    }

    internal sealed class HoneycombEventScopeManager : IHoneycombEventScopeManager
    {
        private const string ContextItemName = "Honeycomb_event";

        private readonly AsyncLocal<HoneycombEvent?> _asyncLocal = new AsyncLocal<HoneycombEvent?>();
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HoneycombEventScopeManager(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        private HttpContext? HttpContext => _httpContextAccessor.HttpContext;

        public IDisposable CreateScope(out HoneycombEvent @event)
        {
            return CreateScopeFromHttpContext(out @event) ?? CreateScopeFromAsyncLocal(out @event);
        }

        private IDisposable? CreateScopeFromHttpContext(out HoneycombEvent honeycombEvent)
        {

        }

        private IDisposable CreateScopeFromAsyncLocal(out HoneycombEvent honeycombEvent)
        {
            var current = GetCurrentFromAsyncLocal();

            void DisposeAction()
            {
                _asyncLocal.Value = current;
            }

            // create the honeycomb event

            var honeycomb = new HoneycombEvent();
            _asyncLocal.Value = event;

            honeycombEvent =

            return new Scope(DisposeAction);
        }

        public HoneycombEvent? Current => GetCurrentFromHttpContext() ?? GetCurrentFromAsyncLocal();

        private HoneycombEvent? GetCurrentFromHttpContext()
        {
            var httpContext = HttpContext;

            if (httpContext != null)
                if (httpContext.Items.TryGetValue(ContextItemName, out var value))
                    if (value is HoneycombEvent honeycombEvent)
                        return honeycombEvent;

            return null;
        }

        private HoneycombEvent? GetCurrentFromAsyncLocal()
        {
            return _asyncLocal.Value;
        }

        private sealed class HoneycombEventScope : IHoneycombEventScope
        {
            private readonly Action<TimeSpan> _disposeAction;
            private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

            public Scope(HoneycombEvent @event, Action disposeAction)
            {
                _honeycombEvent = honeycombEvent;
                _disposeAction = disposeAction;
            }

            public HoneycombEvent Event { get; }

            public Exception(Exception exception)
            {
                Event.Data.TryAdd("request.error", exception.Source);
                Event.Data.TryAdd("request.error_detail", exception.Message);
            }

            void IDisposable.Dispose()
            {

                // timing
                ev.Data.TryAdd("duration_ms", _stopwatch.ElapsedMilliseconds);

                // enqueue

                // disposeAction}
            }
        }
    }
}
