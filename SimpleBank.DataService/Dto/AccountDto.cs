using SimpleBank.DataService.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleBank.DataService.Dto
{
    public class AccountDto: AbstractDto
    {
        public AccountDto() : base()
        {

        }

        public Account Account { get; set; }
    }
}
