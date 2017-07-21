using SimpleBank.DataService.Dto;
using SimpleBank.DataService.Entity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBank.DataService.Services
{
    public interface IAccountService
    {
        Task<Account> GetAccount(string email, string password);
        Task<Account> GetAccountByEmail(string email);
        Task<Account> GetAccountByNumber(string number);
        Task<Account> GetAccount(int id);


        Task<List<Account>> GetListAllUser();

        Task<AccountDto> CreateAccount(Account user);
        Task<AccountDto> UpdateAccount(Account user);
        
        Task<TransactionDto> ExecuteTransaction(Transaction trans);        

    }
}
