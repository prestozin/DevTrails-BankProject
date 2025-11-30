using DevTrails___BankProject.Data;
using DevTrails___BankProject.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace DevTrails___BankProject.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly BankContext _context;
        private IDbContextTransaction _currentTransaction;

        public UnitOfWork(BankContext context)
        {
            _context = context;
        }
        public async Task BeginTransactionAsync()
        {
            if (_currentTransaction != null) return;
            _currentTransaction = await _context.Database.BeginTransactionAsync();
        }
        public async Task CommitAsync()
        {
            try
            {
                await _context.SaveChangesAsync(); 

                if (_currentTransaction != null)
                {
                    await _currentTransaction.CommitAsync();
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
            }
            catch (DbUpdateConcurrencyException exception)
            {
                await RollbackAsync();
                throw new InvalidOperationException("O saldo da conta foi modificado por outra transação. Por favor, tente novamente.");
            }
            catch
            {
                await RollbackAsync();
                throw;
            }
        }
        public async Task RollbackAsync()
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync();
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }
}
