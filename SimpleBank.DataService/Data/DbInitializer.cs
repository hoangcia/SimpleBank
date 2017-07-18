using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimpleBank.DataService.Entity;

namespace SimpleBank.DataService.Data
{
    public static class DbInitializer
    {
        public static void Initialize(SimpleBankContext context)
        {
            context.Database.EnsureCreated();

            if (context.Accounts.Any())
            {
                return;
            }

            //default password: 1234abc
            var accounts = new Account[]
            {
                new Account{Email="0011@pc.com", FullName="Church Chill", Balance=90.83M, CreatedDate = DateTime.Now, Number="0011", Password="64ad3fb166ddb41a2ca24f1803b8b722" },
                new Account{Email="1122@pc.com", FullName="John Due", Balance=9090.85M, CreatedDate = DateTime.Now, Number="1122", Password="64ad3fb166ddb41a2ca24f1803b8b722" },
                new Account{Email="2233@pc.com", FullName="Dave", Balance=1890.92M, CreatedDate = DateTime.Now, Number="2233", Password="64ad3fb166ddb41a2ca24f1803b8b722" },
                new Account{Email="3344@pc.com", FullName="Peter Parker", Balance=1890.92M, CreatedDate = DateTime.Now, Number="3344", Password="64ad3fb166ddb41a2ca24f1803b8b722" }
            };

            foreach(Account acc in accounts)
            {
                context.Accounts.Add(acc);
            }
            context.SaveChanges();

        }
    }
}
