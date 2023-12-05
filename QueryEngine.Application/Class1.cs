namespace QueryEngine.Application
{
    public interface IQueryService<T> where T:class
    {
         Task<bool> ProcessRequest();
    }
}