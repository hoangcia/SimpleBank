using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleBank.DataService.Entity
{
    public class Transaction
    {
        [Key]
        public int ID { get; set; }
        public Account ToAccount { get; set; }
        public Account FromAccount { get; set; }
        public DateTime CreatedDate { get; set; }
        public decimal Amount { get; set; }
        public TransactionType Type {get; set;}
        public TransactionStatus Status { get; set; }

    }

    public enum TransactionType
    {
        Unknown,
        Deposit,
        Withdraw,
        Transfer
    }
    public enum TransactionStatus
    {
        Processing = 1,
        Success
    }
}
