using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace SimpleBank.UnitTests
{
    public class FakeHttpContext : HttpContext
    {
        private ISession _session;
        private IServiceProvider _serviceProvider;
        private HttpRequest _request;

        public FakeHttpContext()
        {
            _session = new FakeSession();            
        }
        

        public override IFeatureCollection Features => throw new NotImplementedException();

        public override HttpRequest Request => _request;

        public override HttpResponse Response => throw new NotImplementedException();

        public override ConnectionInfo Connection => throw new NotImplementedException();

        public override WebSocketManager WebSockets => throw new NotImplementedException();

        public override AuthenticationManager Authentication => throw new NotImplementedException();

        public override ClaimsPrincipal User { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override IDictionary<object, object> Items { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override IServiceProvider RequestServices { get => _serviceProvider; set => _serviceProvider = value; }
        public override CancellationToken RequestAborted { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override string TraceIdentifier { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override ISession Session { get => _session; set => _session = value; }

        public override void Abort()
        {
            throw new NotImplementedException();
        }
    }

    public class FakeSession : ISession
    {
        private Dictionary<string, object> _sessionItems;
        public FakeSession()
        {
            _sessionItems = new Dictionary<string, object>();
        }
        
        public bool IsAvailable => _sessionItems != null;

        public string Id => throw new NotImplementedException();

        public IEnumerable<string> Keys => _sessionItems.Keys;

        public void Clear()
        {
            _sessionItems.Clear();
        }

        public Task CommitAsync()
        {
            throw new NotImplementedException();
        }

        public Task LoadAsync()
        {
            throw new NotImplementedException();
        }

        public void Remove(string key)
        {
            _sessionItems.Remove(key);
        }

        public void Set(string key, byte[] value)
        {
            _sessionItems.Add(key, value);
        }

        public bool TryGetValue(string key, out byte[] value)
        {
            try
            {
                value = (byte[])_sessionItems[key];
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                value = null;
                return false;
            }

        }
    }

    public class FakeServiceProvider : IServiceProvider
    {
        public object GetService(Type serviceType)
        {
            throw new NotImplementedException();
        }
    }       

    
}
