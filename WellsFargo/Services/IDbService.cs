namespace WellsFargo.Services
{
    public interface IDbService<T>
    {
        Task<bool> AddOrUpdate(IEnumerable<T> objects);
    }
}
