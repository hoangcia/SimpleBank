using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SimpleBank.DataService.Entity;
using SimpleBank.DataService.Data;
using SimpleBank.DataService.Dto;
using Microsoft.EntityFrameworkCore;

namespace SimpleBank.DataService.Services
{
    public class AccountService : IAccountService
    {
        SimpleBankContext _dbContext;

        public AccountService(SimpleBankContext ctx) {
            _dbContext = ctx;
        }
        public async Task<AccountDto> CreateAccount(Account user)
        {
            AccountDto accountDto = new AccountDto();
            accountDto.Account = user;
            _dbContext.Accounts.Add(user);
            try
            {
                var result = await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {

                accountDto.Errors.Add("CreateAccount", ex.Message);
                
                return accountDto;
            }

            return accountDto;
        }

        public async Task<TransactionDto> ExecuteTransaction(Transaction trans)
        {
            if (trans == null) return null;

            var transDto = new TransactionDto();

            Account toAccount = trans.ToAccount == null ? null : await _dbContext.Accounts.SingleOrDefaultAsync(acc => acc.ID == trans.ToAccount.ID);
            Account fromAccount = trans.FromAccount == null ? null: await _dbContext.Accounts.SingleOrDefaultAsync(acc => acc.ID == trans.FromAccount.ID);
            

            try {
                using (var dbTransaction = _dbContext.Database.BeginTransaction())
                {
                    switch (trans.Type)
                    {
                        case TransactionType.Deposit:
                            string validateMsg = ValidateDepositTransaction(trans);
                            if (!string.IsNullOrEmpty(validateMsg)){
                                transDto.Errors.Add("DepositTransaction", validateMsg);
                                return transDto;
                            }
                            toAccount.Balance += trans.Amount;
                            
                            break;
                        case TransactionType.Withdraw:
                            validateMsg = ValidateWithdrawTransaction(trans);
                            if (!string.IsNullOrEmpty(validateMsg))
                            {
                                transDto.Errors.Add("WithdrawTransaction", validateMsg);
                                return transDto;
                            }
                            fromAccount.Balance -= trans.Amount;
                            
                            break;
                        case TransactionType.Transfer:
                            validateMsg = ValidateTransferTransaction(trans);
                            if (!string.IsNullOrEmpty(validateMsg))
                            {
                                transDto.Errors.Add("TransferTransaction", validateMsg);
                                return transDto;
                            }
                            fromAccount.Balance -= trans.Amount;
                            toAccount.Balance += trans.Amount;                            

                            break;
                        default: return null;
                    }

                    trans.FromAccount = fromAccount;
                    trans.ToAccount = toAccount;
                    
                    if(toAccount != null) _dbContext.Entry(toAccount).Property("RowVersion").OriginalValue = toAccount.RowVersion;
                    if (fromAccount != null)  _dbContext.Entry(fromAccount).Property("RowVersion").OriginalValue = fromAccount.RowVersion;
                    
                    _dbContext.Transactions.Add(trans);
                    await _dbContext.SaveChangesAsync();
                    dbTransaction.Commit();
                }   
            }

            catch (DbUpdateConcurrencyException ex)
            {
                //wrong account info or balance
                if(trans.ToAccount != null) GenerateAccountErrorsLog("ToAccount", trans.ToAccount, ex, transDto.Errors);
                if (trans.FromAccount != null) GenerateAccountErrorsLog("FromAccount",trans.FromAccount, ex, transDto.Errors);                
            }
            transDto.Transaction = trans;
            return transDto;

        }
        
        public async Task<Account> GetAccount(string email, string password)
        {
            return await _dbContext.Accounts.SingleOrDefaultAsync(acc => acc.Email.ToLower() == email && acc.Password == password);
        }
        public async Task<Account> GetAccountByEmail(string email)
        {
            return await _dbContext.Accounts.AsNoTracking().SingleOrDefaultAsync(acc => acc.Email.ToLower() == email);
        }
        public async Task<Account> GetAccountByNumber(string number)
        {
            return await _dbContext.Accounts.AsNoTracking().SingleOrDefaultAsync(acc => acc.Number == number);
        }
        public async Task<Account> GetAccount(int id)
        {
            return await _dbContext.Accounts.SingleOrDefaultAsync(acc => acc.ID == id);
        }

        public async Task<List<Account>> GetListAllUser()
        {
            return await _dbContext.Accounts.ToListAsync();
        }

        public async Task<AccountDto> UpdateAccount(Account user)
        {
            var accountDto = new AccountDto();

            var foundAccount = await _dbContext.Accounts.SingleOrDefaultAsync(acc => acc.ID == user.ID);

            if(foundAccount == null)
            {
                return await CreateAccount(user);
            }
            else
            {
                foundAccount.Address = user.Address;
                foundAccount.FullName = user.FullName;

                // set the RowVersion value for the retrieved Account to detect concurrency
                _dbContext.Entry(foundAccount).Property("RowVersion").OriginalValue = user.RowVersion;

                try
                {
                    var result = await _dbContext.SaveChangesAsync();
                    accountDto.Account = foundAccount;
                    return accountDto;
                }
                catch(DbUpdateConcurrencyException ex)
                {
                    accountDto.Account = user;
                    GenerateAccountErrorsLog(string.Empty, foundAccount, ex, accountDto.Errors);
                    return accountDto;
                }
            }
        }
        #region Static Methods

        public static string ValidateDepositTransaction(Transaction trans)
        {
            if (trans == null || trans.Amount <= 0) return "Invalid amount";
            return string.Empty;
        }
        public static string ValidateTransferTransaction(Transaction trans)
        {
            var errMsg = string.Empty;

            if (trans == null || trans.Amount <= 0 || trans.FromAccount.Balance < trans.Amount) errMsg = "Invalid amount";
            if (trans.FromAccount == null || string.IsNullOrEmpty(trans.FromAccount.Number)) errMsg += " Invalid FromAccount";
            if (trans.ToAccount == null || string.IsNullOrEmpty(trans.ToAccount.Number)) errMsg += " Invalid ToAccount";

            return string.Empty;
        }
        public static string ValidateWithdrawTransaction(Transaction trans)
        {
            if (trans == null || trans.Amount <= 0 || trans.FromAccount.Balance < trans.Amount) return "Invalid amount";
            return string.Empty;
        }

        
        private Dictionary<string, string> GenerateAccountErrorsLog(string prefix, Account accountToUpdate, DbUpdateConcurrencyException ex, Dictionary<string, string> errors)
        {            
            var exceptionEntry = ex.Entries.Single();
            var clientValues = (Account)exceptionEntry.Entity;
            var databaseEntry = exceptionEntry.GetDatabaseValues();
            if (databaseEntry == null)
            {
                errors.Add("DeletedAccount", "Could not save changes. The account was deleted.");
            }
            else
            {
                var databaseValues = (Account)databaseEntry.ToObject();
                if (databaseValues.FullName != clientValues.FullName)
                {
                    if(string.IsNullOrEmpty(prefix)) errors.Add("FullName", $"Current value: {databaseValues.FullName}");
                    else errors.Add($"{prefix}.FullName", $"Current value: {databaseValues.FullName}");
                }
                if (databaseValues.Address != clientValues.Address)
                {
                    if (string.IsNullOrEmpty(prefix)) errors.Add("Address", $"Current value: {databaseValues.Address}");
                    else errors.Add($"{prefix}.Address", $"Current value: {databaseValues.Address}");
                }
                if (databaseValues.Balance != clientValues.Balance)
                {
                    if (string.IsNullOrEmpty(prefix)) errors.Add("Address", $"Current value: {databaseValues.Address}");
                    else errors.Add($"{prefix}.Balance", $"Current value: {databaseValues.Balance}");
                }

                accountToUpdate.RowVersion = (byte[])databaseValues.RowVersion;                
            }

            return errors;
        }
        
        #endregion
    }
}
