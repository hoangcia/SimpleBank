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
using SimpleBank.DataService.Services;
using SimpleBank.DataService.Dto;

namespace SimpleBank.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        // GET: Account

        public async Task<IActionResult> Index()
        {
            //get signed in user
            
            var signedInUserEmail = HttpContext.Session.GetString("SignedInUserEmail");
            if (string.IsNullOrEmpty(signedInUserEmail)) return Redirect("/Home/Index");
            var signedInAcc = await _accountService.GetAccountByEmail(signedInUserEmail);
            return View(signedInAcc);
        }

        // GET: Account/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _accountService.GetAccount(id.Value);
                
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

            var account = await _accountService.GetAccountByEmail(signedInUserEmail);
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


            var accountToUpdate = new Account { ID = id.Value, RowVersion = rowVersion };
            await TryUpdateModelAsync(accountToUpdate);

            AccountDto accDto = await _accountService.UpdateAccount(accountToUpdate);

            foreach(var e in accDto.Errors)
            {
                ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                        + "was modified by another user after you got the original value. The "
                        + "edit operation was canceled and the current values in the database "
                        + "have been displayed. If you still want to edit this record, click "
                        + "the Save button again. Otherwise click the Back to Home hyperlink.");
                ModelState.AddModelError(e.Key, e.Value);

            }
            if (accDto.Errors.Any())
            {
                return View(accountToUpdate);
            }
            return View("Index", accDto.Account);
        }
        [HttpPost]
        public async Task<IActionResult> SignIn(string username, string password)
        {

            string hashedPassword = !string.IsNullOrEmpty(password)? Common.GenerateHashedPassword(password) : string.Empty;
            Account acc = await _accountService.GetAccount(username, hashedPassword);
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
             AccountDto accDto = await _accountService.CreateAccount(acc);
            
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
            Account signedInUser = await _accountService.GetAccountByEmail(signedInUserEmail);
            transaction.Type = TransactionType.Deposit;
            transaction.CreatedDate = DateTime.Now;
            transaction.ToAccount = signedInUser;

            TransactionDto transDto = await _accountService.ExecuteTransaction(transaction);
            foreach (var e in transDto.Errors)
            {
                ModelState.AddModelError(e.Key, e.Value);
            }
            return View("TransactionSuccess");
        }
        [HttpPost]
        public async Task<IActionResult> Withdraw(Transaction transaction)
        {

            string signedInUserEmail = HttpContext.Session.GetString("SignedInUserEmail");
            Account signedInUser = await _accountService.GetAccountByEmail(signedInUserEmail);
            transaction.Type = TransactionType.Withdraw;
            transaction.CreatedDate = DateTime.Now;
            transaction.FromAccount = signedInUser;

            TransactionDto transDto = await _accountService.ExecuteTransaction(transaction);
            if (transDto.Errors.Any())
            {
                foreach (var e in transDto.Errors)
                {
                    ModelState.AddModelError(e.Key, e.Value);
                }
                return View();
            }
            
            return View("TransactionSuccess");
        }
        [HttpPost]
        public async Task<IActionResult> Transfer(Transaction transaction)
        {            
            string signedInUserEmail = HttpContext.Session.GetString("SignedInUserEmail");
            Account signedInUser = await _accountService.GetAccountByEmail(signedInUserEmail);
            Account toAccount = await _accountService.GetAccountByNumber(transaction.ToAccount.Number.Trim()); 
            transaction.Type = TransactionType.Transfer;
            transaction.CreatedDate = DateTime.Now;
            transaction.FromAccount = signedInUser;
            transaction.ToAccount = toAccount;

            
            TransactionDto transDto = await _accountService.ExecuteTransaction(transaction);

            if (transDto.Errors.Any())
            {
                foreach (var e in transDto.Errors)
                {
                    ModelState.AddModelError(e.Key, e.Value);
                }
                return View();
            }
            return View("TransactionSuccess");
        }        
    }
}
