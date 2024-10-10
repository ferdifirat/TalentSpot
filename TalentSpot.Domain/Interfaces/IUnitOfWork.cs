namespace TalentSpot.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        Task<bool> CompleteAsync();
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}
