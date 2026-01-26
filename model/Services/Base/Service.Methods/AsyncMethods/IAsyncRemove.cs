namespace Services.Base.Service.Methods.AsyncMethods;

public interface IAsyncRemove
{
    Task<bool> DeleteAsync(Guid guid);
}