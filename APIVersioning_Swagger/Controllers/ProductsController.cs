
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace APIVersioning_Swagger.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/Products")]
    public class ProductsController : ODataController
    {

        [EnableQuery]
        public IEnumerable<string> Get()
        {
            return new[] { "SDDf", "dsfsdfs", "sfsdf", "sdfsdf" };
        }
    }
}
