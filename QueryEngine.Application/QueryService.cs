using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryEngine.Application
{
    public class QueryService : IQueryService<QueryService>
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public QueryService(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }
        public async Task<bool> ProcessRequest()
        {
            var header = httpContextAccessor.HttpContext.Request.Headers;
            return await Task.FromResult(true);
        }
    }

    public class QueryServiceB : IQueryService<QueryServiceB>
    {
        public Task<bool> ProcessRequest()
        {
            return Task.FromResult(true);
        }
    }
}
