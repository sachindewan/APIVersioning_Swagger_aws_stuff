namespace APIVersioning_Swagger.Services
{
    public interface ITokenService
    {
        public Task<string> GeTokenAsync();
    }
}
