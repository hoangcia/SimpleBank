using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SimpleBank.DataService.Data;
using SimpleBank.DataService.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace SimpleBank.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly SimpleBankContext _context;

        public HomeController(SimpleBankContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SignedInUserEmail")))
            {
                return Redirect("/Account/Index");
            }
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        public IActionResult ViewTransaction()
        {
            var transactions = _context.Transactions
                .Include(trans => trans.FromAccount)
                .Include(trans => trans.ToAccount)
                .ToList<Transaction>();
            
            return View(transactions);
        }

    }
}
