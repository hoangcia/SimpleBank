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

namespace SimpleBank.UnitTests
{
    public class AccountControllerTests
    {
        private SimpleBankContext _context;
        /*
        private readonly TestServer _server;
        private readonly HttpClient _client1;
        private readonly HttpClient _client2;
        */
        public AccountControllerTests()
        {
            /*set up server*/
            /*
            _server = new TestServer(new WebHostBuilder()
            .UseStartup<Startup>());

            _client1 = _server.CreateClient();
            _client2 = _server.CreateClient();
            */
            InitDbContext();
            ClearDb();            
            InitializeDb();
        }
        public void ClearDb()
        {
            _context.Accounts.RemoveRange(_context.Accounts.ToArray());
            _context.SaveChanges();
        }
        public void InitializeDb()
        {
            var accounts = new Account[]
            {
                new Account{Email="0101@pc.com", FullName="Church Chill", Balance=90.83M, CreatedDate = DateTime.Now, Number="0101", Password="64ad3fb166ddb41a2ca24f1803b8b722", Address="Street 11" },
                new Account{Email="1212@pc.com", FullName="John Due", Balance=9090.85M, CreatedDate = DateTime.Now, Number="1212", Password="64ad3fb166ddb41a2ca24f1803b8b722", Address="Street 22" },
                new Account{Email="2323@pc.com", FullName="Dave", Balance=1890.92M, CreatedDate = DateTime.Now, Number="2323", Password="64ad3fb166ddb41a2ca24f1803b8b722", Address="Street 33" },
                new Account{Email="3434@pc.com", FullName="Peter Parker", Balance=1890.92M, CreatedDate = DateTime.Now, Number="3434", Password="64ad3fb166ddb41a2ca24f1803b8b722", Address="Street 44" }
            };

            _context.Accounts.AddRange(accounts);
            int changed = _context.SaveChanges();
        }
        public void InitDbContext()
        {
            var builder = new DbContextOptionsBuilder<SimpleBankContext>().UseInMemoryDatabase();
            
            var context = new SimpleBankContext(builder.Options);
            
            _context = context;
        }
        [Fact]
        public void CheckValidDepositAmount_Null()
        {
            
            string msg = AccountController.ValidateDepositTransaction(null);

            Assert.Equal("Invalid amount", msg);

        }
        [Fact]
        public void CheckValidDepositAmount_NegativeAmount()
        {
            Transaction trans = new Transaction
            {
                Amount = -10
            };
            string msg = AccountController.ValidateDepositTransaction(trans);

            Assert.Equal("Invalid amount", msg);

        }
        [Fact]
        public void CheckValidDepositAmount_PositiveAmount()
        {
            Transaction trans = new Transaction
            {
                Amount = 10
            };
            string msg = AccountController.ValidateDepositTransaction(trans);

            Assert.Equal("", msg);

        }
        [Fact]
        public void CheckValidWithdrawAmount_Null()
        {

            string msg = AccountController.ValidateWithdrawTransaction(null);

            Assert.Equal("Invalid amount", msg);

        }
        [Fact]
        public void CheckValidWithdrawAmount_NegativeAmount()
        {
            Transaction trans = new Transaction
            {
                Amount = -10
            };
            string msg = AccountController.ValidateWithdrawTransaction(trans);

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
            string msg = AccountController.ValidateWithdrawTransaction(trans);

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
            string msg = AccountController.ValidateWithdrawTransaction(trans);

            Assert.Equal("Invalid amount", msg);

        }

        [Fact]
        public void CheckAccountBalanceAfterTransfer()
        {
            Transaction trans = new Transaction
            {
                FromAccount = new Account { Balance = 10M },
                ToAccount = new Account { Balance = 5M },
                Amount = 2M,
                Type = TransactionType.Transfer
            };
            
            Assert.True(AccountController.ExecuteTransaction(trans));
            Assert.Equal(8M, trans.FromAccount.Balance);
            Assert.Equal(7M, trans.ToAccount.Balance);
        }
        [Fact]
        public void CheckAccountBalanceAfterDeposit()
        {
            Transaction trans = new Transaction
            {
                ToAccount = new Account { Balance = 10M },                
                Amount = 2M,
                Type = TransactionType.Deposit
            };

            Assert.True(AccountController.ExecuteTransaction(trans));            
            Assert.Equal(12M, trans.ToAccount.Balance);
        }
        [Fact]
        public void CheckAccountBalanceAfterWithdraw()
        {
            Transaction trans = new Transaction
            {
                FromAccount = new Account { Balance = 10M },                
                Amount = 2M,
                Type = TransactionType.Withdraw
            };
            
            Assert.True(AccountController.ExecuteTransaction(trans));
            Assert.Equal(8M, trans.FromAccount.Balance);            
        }
        [Fact]
        public void TestInvalidTransaction()
        {
            Transaction unknownTrans = new Transaction
            {
                Type = TransactionType.Unknown
            };
            Assert.False(AccountController.ExecuteTransaction(null));
            Assert.False(AccountController.ExecuteTransaction(unknownTrans));
        }

        //Concurrent case 1: Task1 is run first but the entity is not updated due to Task2 already updated the entity. The entity of Task1 is out of date.
        [Fact]
        public async void TestConcurrentEditAccount_Case1()
        {
                        
        }
        
    }
}
