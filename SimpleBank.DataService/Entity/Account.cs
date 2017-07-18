using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleBank.DataService.Entity
{
    public class Account
    {
        [Key]
        public int ID { get; set; }
        public string Number { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }
        public decimal Balance { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Address { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
