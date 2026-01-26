namespace Repositories.Base.Crud.Methods.AsyncMethods;

public interface IAsyncDelete
{
    Task<bool> DeleteAsync(Guid guid);
}