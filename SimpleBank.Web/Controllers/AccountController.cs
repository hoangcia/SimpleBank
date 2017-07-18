using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SimpleBank.DataService.Data;
using SimpleBank.DataService.Entity;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Text;
using SimpleBank.Web.Utils;

namespace SimpleBank.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly SimpleBankContext _context;        

        public AccountController(SimpleBankContext context)
        {
            _context = context;    
        }

        // GET: Account

        public async Task<IActionResult> Index()
        {
            //get signed in user
            
            var signedInUserEmail = HttpContext.Session.GetString("SignedInUserEmail");
            if (string.IsNullOrEmpty(signedInUserEmail)) return Redirect("/Home/Index");
            var signedInAcc = await _context.Accounts.SingleOrDefaultAsync<Account>(acc => acc.Email.ToLower() == signedInUserEmail.ToLower());
            return View(signedInAcc);
        }

        // GET: Account/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts
                .SingleOrDefaultAsync(m => m.ID == id);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }
        // GET: Account/Edit
        [HttpGet]
        public async Task<IActionResult> Edit()
        {            
            string signedInUserEmail = HttpContext.Session.GetString("SignedInUserEmail");

            var account = await _context.Accounts
                .AsNoTracking()
                .SingleOrDefaultAsync(m => m.Email.ToLower() == signedInUserEmail.ToLower());
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }
        // GET: Account/Edit/5
        //Handle concurency update for Account model
        [HttpPost]
        public async Task<IActionResult> Edit(int? id, byte[] rowVersion)
        {
            if (id == null)
            {
                return NotFound();
            }
            string signedInUserEmail = HttpContext.Session.GetString("SignedInUserEmail");


            var accountToUpdate = await _context.Accounts
                .SingleOrDefaultAsync(m => m.Email.ToLower() == signedInUserEmail.ToLower());


            if (accountToUpdate == null )
            {
                if(accountToUpdate.ID != id) 
                    return NotFound();
                                
                ModelState.AddModelError(string.Empty, "Could not save changes. The account was not found.");
                return View();
            }

            _context.Entry(accountToUpdate).Property("RowVersion").OriginalValue = rowVersion;            

            if (await TryUpdateModelAsync<Account>(accountToUpdate, "", m => m.FullName, m => m.Address))
            {
                try
                {                    
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var exceptionEntry = ex.Entries.Single();
                    var clientValues = (Account)exceptionEntry.Entity;
                    var databaseEntry = exceptionEntry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        ModelState.AddModelError(string.Empty, "Could not save changes. The account was deleted.");
                    }
                    else
                    {
                        var databaseValues = (Account)databaseEntry.ToObject();
                        if(databaseValues.FullName != clientValues.FullName)
                        {
                            ModelState.AddModelError("FullName", $"Current value: {databaseValues.FullName}");
                        }
                        if (databaseValues.Address != clientValues.Address)
                        {
                            ModelState.AddModelError("Address", $"Current value: {databaseValues.Address}");
                        }                        

                        ModelState.AddModelError(string.Empty, @"The record you attemped to edit was modified by another transaction after you got the original value.");

                        accountToUpdate.RowVersion = (byte[])databaseValues.RowVersion;
                        ModelState.Remove("RowVersion");
                    }

                }
            }

            return View("Edit", accountToUpdate);
        }
        [HttpPost]
        public IActionResult SignIn(string username, string password)
        {

            string hashedPassword = !string.IsNullOrEmpty(password)? Common.GenerateHashedPassword(password) : string.Empty;
            Account acc = _context.Accounts.SingleOrDefault(account => account.Email == username && account.Password.ToLower() == hashedPassword.ToLower());
            if (acc != null)
            {
                HttpContext.Session.SetString("SignedInUserEmail", username.ToLower());
                return View("Index", acc);
            }
                        
            return Redirect("/Home/Index");
            
        }

        public IActionResult SignOut()
        {
            HttpContext.Session.Remove("SignedInUserEmail");
            return Redirect("/Home/Index");
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(string email, string name, string password)
        {

            string number = Common.GenerateNumber(email, name);
            string hashedPassword = !string.IsNullOrEmpty(password) ? Common.GenerateHashedPassword(password) : string.Empty;

            Account acc = new Account
            {
                Email = email,
                FullName = name,
                Balance = 0M,
                CreatedDate = DateTime.Now,
                Number = number,
                Password = hashedPassword
            };
             _context.Accounts.Add(acc);
            await _context.SaveChangesAsync();
            HttpContext.Session.SetString("SignedInUserEmail", email.ToLower());
            return View("Index", acc);
        }

        public IActionResult Deposit()
        {

            return View();
        }
        public IActionResult Withdraw()
        {
            return View();
        }
        public IActionResult Transfer()
        {
            return View();
        }

        [HttpPost]        
        public async Task<IActionResult> Deposit(Transaction transaction)
       {            
            string signedInUserEmail = HttpContext.Session.GetString("SignedInUserEmail");
            Account signedInUser = _context.Accounts.SingleOrDefault(acc => acc.Email.ToLower() == signedInUserEmail.ToLower());
            transaction.Type = TransactionType.Deposit;
            transaction.CreatedDate = DateTime.Now;
            transaction.ToAccount = signedInUser;

            string errMsg = ValidateDepositTransaction(transaction);
            if (!string.IsNullOrEmpty(errMsg))
            {
                ViewBag.ErrMsg = errMsg;
                return View("TransactionError");
            }

            ExecuteTransaction(transaction);

            _context.Transactions.Add(transaction);

            await _context.SaveChangesAsync();

            return View("TransactionSuccess");
        }
        [HttpPost]
        public async Task<IActionResult> Withdraw(Transaction transaction)
        {            

            string signedInUserEmail = HttpContext.Session.GetString("SignedInUserEmail");
            Account signedInUser = _context.Accounts.SingleOrDefault(acc => acc.Email.ToLower() == signedInUserEmail.ToLower());
            transaction.Type = TransactionType.Withdraw;
            transaction.CreatedDate = DateTime.Now;
            transaction.FromAccount = signedInUser;

            string errMsg = ValidateWithdrawTransaction(transaction);
            if (!string.IsNullOrEmpty(errMsg))
            {
                ViewBag.ErrMsg = errMsg;
                return View("TransactionError");
            }

            ExecuteTransaction(transaction);

            _context.Transactions.Add(transaction);

            await _context.SaveChangesAsync();
            return View("TransactionSuccess");
        }
        [HttpPost]
        public async Task<IActionResult> Transfer(Transaction transaction)
        {            
            string signedInUserEmail = HttpContext.Session.GetString("SignedInUserEmail");
            Account signedInUser = _context.Accounts.SingleOrDefault(acc => acc.Email.ToLower() == signedInUserEmail.ToLower());
            Account toAccount = _context.Accounts.SingleOrDefault(acc => acc.Number == transaction.ToAccount.Number); 
            transaction.Type = TransactionType.Transfer;
            transaction.CreatedDate = DateTime.Now;
            transaction.FromAccount = signedInUser;
            transaction.ToAccount = toAccount;

            string errMsg = ValidateTransferTransaction(transaction);
            if (!string.IsNullOrEmpty(errMsg))
            {
                ViewBag.ErrMsg = errMsg;
                return View("TransactionError");
            }

            ExecuteTransaction(transaction);

            _context.Transactions.Add(transaction);

            await _context.SaveChangesAsync();
            return View("TransactionSuccess");
        }

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

        public static bool ExecuteTransaction(Transaction trans)
        {
            try
            {
                switch (trans.Type)
                {
                    case TransactionType.Deposit:
                        trans.ToAccount.Balance += trans.Amount;
                        break;
                    case TransactionType.Withdraw:
                        trans.FromAccount.Balance -= trans.Amount;
                        break;
                    case TransactionType.Transfer:
                        trans.FromAccount.Balance -= trans.Amount;
                        trans.ToAccount.Balance += trans.Amount;
                        break;
                    default: return false;

                }
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
            
        }
    }
}
