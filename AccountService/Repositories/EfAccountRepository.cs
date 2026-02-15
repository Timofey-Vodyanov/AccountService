using Microsoft.EntityFrameworkCore;
using AccountService.Models;
using AccountService.Data;

namespace AccountService.Repositories;

public class EfAccountRepository : IAccountRepository
{
    private readonly ApplicationDbContext _context;

    public EfAccountRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Account> CreateAccount(Account account)
    {
        await _context.Accounts.AddAsync(account);
        await _context.SaveChangesAsync();
        return account;
    }

    public async Task<Account?> GetAccountById(Guid id)
    {
        return await _context.Accounts
            .Include(a => a.Transactions)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Account>> GetAccountsByOwner(Guid ownerId)
    {
        return await _context.Accounts
            .Where(a => a.OwnerId == ownerId)
            .Include(a => a.Transactions)
            .ToListAsync();
    }

    public async Task<IEnumerable<Account>> GetAllAccounts()
    {
        return await _context.Accounts
            .Include(a => a.Transactions)
            .ToListAsync();
    }

    public async Task UpdateAccount(Account account)
    {
        _context.Accounts.Update(account);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAccount(Guid id)
    {
        var account = await _context.Accounts.FindAsync(id);
        if (account != null)
        {
            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();
        }
    }

    public async Task AddTransaction(Transaction transaction)
    {
        await _context.Transactions.AddAsync(transaction);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsForAccount(Guid accountId, DateTime? from, DateTime? to)
    {
        var query = _context.Transactions
            .Where(t => t.AccountId == accountId)
            .AsQueryable();

        if (from.HasValue)
            query = query.Where(t => t.Timestamp >= from.Value);
        if (to.HasValue)
            query = query.Where(t => t.Timestamp <= to.Value);

        return await query.OrderBy(t => t.Timestamp).ToListAsync();
    }

    public async Task<bool> AccountExists(Guid id)
    {
        return await _context.Accounts.AnyAsync(a => a.Id == id);
    }

    public async Task<Transaction?> GetTransactionById(Guid id)
    {
        return await _context.Transactions.FindAsync(id);
    }
}