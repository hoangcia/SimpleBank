using System;
using Xunit;
using SimpleBank.Web.Controllers;
using SimpleBank.DataService.Entity;
using SimpleBank.DataService.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Moq;
using Microsoft.AspNetCore.TestHost;
using System.Net.Http;
using SimpleBank.Web;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Specialized;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Http.Features;
using SimpleBank.DataService.Services;
using SimpleBank.DataService.Dto;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace SimpleBank.UnitTests
{
    public class AccountServiceTests
    {
        private SimpleBankContext _context;
        private DbContextOptionsBuilder<SimpleBankContext> _dbContextBuilder;

        public AccountServiceTests()
        {
            InitDbContextOptionBuilder();
            InitDbContext();
            ClearDb();            
            InitializeDb();
        }
        public void ClearDb()
        {
            _context.Transactions.RemoveRange(_context.Transactions.ToArray());
            _context.Accounts.RemoveRange(_context.Accounts.ToArray());
            _context.SaveChanges();
        }
        public void InitializeDb()
        {
            var accounts = new Account[]
            {
                new Account{Email="0101@pc.com", FullName="Church Chill", Balance=100M, CreatedDate = DateTime.Now, Number="0101", Password="64ad3fb166ddb41a2ca24f1803b8b722", Address="Street 11" },
                new Account{Email="1212@pc.com", FullName="John Due", Balance=100M, CreatedDate = DateTime.Now, Number="1212", Password="64ad3fb166ddb41a2ca24f1803b8b722", Address="Street 22" },
                new Account{Email="2323@pc.com", FullName="Dave", Balance=100M, CreatedDate = DateTime.Now, Number="2323", Password="64ad3fb166ddb41a2ca24f1803b8b722", Address="Street 33" },
                new Account{Email="3434@pc.com", FullName="Peter Parker", Balance=100M, CreatedDate = DateTime.Now, Number="3434", Password="64ad3fb166ddb41a2ca24f1803b8b722", Address="Street 44" },
                new Account{Email="4545@pc.com", FullName="Pholandos", Balance=100M, CreatedDate = DateTime.Now, Number="4545", Password="64ad3fb166ddb41a2ca24f1803b8b722", Address="Street 55" }
            };

            _context.Accounts.AddRange(accounts);
            int changed = _context.SaveChanges();
        }
        public void InitDbContextOptionBuilder()
        {
            _dbContextBuilder = new DbContextOptionsBuilder<SimpleBankContext>()/*.UseInMemoryDatabase(Guid.NewGuid().ToString())*/.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=SimpleBankDb_Test;Trusted_Connection=True;MultipleActiveResultSets=true")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));
        }
        public void InitDbContext()
        {                       
            var context = new SimpleBankContext(_dbContextBuilder.Options);
            context.Database.EnsureCreated();
            _context = context;
        }
        public SimpleBankContext NewDbContext()
        {            
            var context = new SimpleBankContext(_dbContextBuilder.Options);
            context.Database.EnsureCreated();
            return context;
        }
        [Fact]
        public void CheckValidDepositAmount_Null()
        {
            
            string msg = AccountService.ValidateDepositTransaction(null);

            Assert.Equal("Invalid amount", msg);

        }
        [Fact]
        public void CheckValidDepositAmount_NegativeAmount()
        {
            Transaction trans = new Transaction
            {
                Amount = -10
            };
            string msg = AccountService.ValidateDepositTransaction(trans);

            Assert.Equal("Invalid amount", msg);

        }
        [Fact]
        public void CheckValidDepositAmount_PositiveAmount()
        {
            Transaction trans = new Transaction
            {
                Amount = 10
            };
            string msg = AccountService.ValidateDepositTransaction(trans);

            Assert.Equal("", msg);

        }
        [Fact]
        public void CheckValidWithdrawAmount_Null()
        {

            string msg = AccountService.ValidateWithdrawTransaction(null);

            Assert.Equal("Invalid amount", msg);

        }
        [Fact]
        public void CheckValidWithdrawAmount_NegativeAmount()
        {
            Transaction trans = new Transaction
            {
                Amount = -10
            };
            string msg = AccountService.ValidateWithdrawTransaction(trans);

            Assert.Equal("Invalid amount", msg);

        }
        [Fact]
        public void CheckValidWithdrawAmount_PositiveAmount()
        {
            Transaction trans = new Transaction
            {
                FromAccount = new Account { Balance = 10},
                Amount = 10
            };
            string msg = AccountService.ValidateWithdrawTransaction(trans);

            Assert.Equal("", msg);

        }
        [Fact]
        public void CheckValidWithdrawAmount_Balance()
        {
            Transaction trans = new Transaction
            {
                FromAccount = new Account { Balance = 9 },
                Amount = 10
            };
            string msg = AccountService.ValidateWithdrawTransaction(trans);

            Assert.Equal("Invalid amount", msg);

        }

        [Fact]
        public async Task CheckAccountBalanceAfterTransfer()
        {
            Account toAcc = await _context.Accounts.SingleAsync(a => a.Email == "0101@pc.com");
            Account fromAcc = await _context.Accounts.SingleAsync(a => a.Email == "1212@pc.com");

            Transaction trans = new Transaction
            {
                FromAccount = fromAcc,
                ToAccount = toAcc,
                Amount = 2M,
                Type = TransactionType.Transfer,
                CreatedDate = DateTime.Now
            };

            TransactionDto transDto = await (new AccountService(_context)).ExecuteTransaction(trans);
            Account updatedToAcc = await _context.Accounts.SingleAsync(a => a.Email == "0101@pc.com");
            Account updatedFromAcc = await _context.Accounts.SingleAsync(a => a.Email == "1212@pc.com");

            Assert.NotNull(transDto);
            Assert.Equal(98M, updatedFromAcc.Balance);
            Assert.Equal(102M, updatedToAcc.Balance);
        }
        [Fact]
        public async Task CheckAccountBalanceAfterDeposit()
        {
            Account toAcc = await _context.Accounts.SingleAsync(a => a.Email == "2323@pc.com");
            Transaction trans = new Transaction
            {
                ToAccount = toAcc,                
                Amount = 2M,
                Type = TransactionType.Deposit,
                CreatedDate = DateTime.Now
            };

            TransactionDto transDto = await (new AccountService(_context)).ExecuteTransaction(trans);
            Account updatedAcc = await _context.Accounts.SingleAsync(a => a.Email == "2323@pc.com");
            Assert.NotNull(transDto);
            Assert.Equal(102M, updatedAcc.Balance);
        }
        [Fact]
        public async Task CheckAccountBalanceAfterWithdraw()
        {
            Account fromAcc = await _context.Accounts.SingleAsync(a => a.Email == "3434@pc.com");
            Transaction trans = new Transaction
            {
                FromAccount = fromAcc,                
                Amount = 2M,
                Type = TransactionType.Withdraw,
                CreatedDate = DateTime.Now
            };

            TransactionDto transDto = await (new AccountService(_context)).ExecuteTransaction(trans);
            Account updatedAcc = await _context.Accounts.SingleAsync(a => a.Email == "3434@pc.com");
            Assert.NotNull(transDto);
            Assert.Equal(98M, updatedAcc.Balance);            
        }
        [Fact]
        public async Task TestInvalidTransaction()
        {
            Transaction unknownTrans = new Transaction
            {
                Type = TransactionType.Unknown
            };
            Assert.Null(await (new AccountService(_context)).ExecuteTransaction(null));
            Assert.Null(await (new AccountService(_context)).ExecuteTransaction(unknownTrans));
        }

        //Concurrency case 1: Create 2 contexts to update the same account
        [Fact]
        public async Task TestConcurrentEditAccount_Case1()
        {

            SimpleBankContext dbContext1 = NewDbContext();
            SimpleBankContext dbContext2 = NewDbContext();
            AccountService accountService1 = new AccountService(dbContext1);
            AccountService accountService2 = new AccountService(dbContext2);            

            Account acc1 = await dbContext1.Accounts.AsNoTracking().SingleAsync(acc => acc.Email == "4545@pc.com");
            acc1.FullName = "Name1";

            Account acc2 = await dbContext2.Accounts.AsNoTracking().SingleAsync(acc => acc.Email == "4545@pc.com");
            acc2.FullName = "Name2";


            var ctx1 = accountService1.UpdateAccount(acc1);
            var ctx2 = accountService2.UpdateAccount(acc2);

            var result = await Task.WhenAll(ctx1, ctx2);

            //Assert if any concurency error
            Assert.True(result.Any(t => t.Errors.Any()));
            //Assert if the one of 2 context succcessfully updated the account
            Account updatedAcc = await _context.Accounts.AsNoTracking().SingleAsync(acc => acc.Email == "4545@pc.com");
            Assert.True(updatedAcc.FullName == "Name1" || updatedAcc.FullName == "Name2");
        }
        
    }
}
