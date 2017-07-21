using SimpleBank.DataService.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleBank.DataService.Dto
{
    public class TransactionDto: AbstractDto
    {
        public TransactionDto():base()
        {
            
        }
        public Transaction Transaction { get; set; }
    }
}
