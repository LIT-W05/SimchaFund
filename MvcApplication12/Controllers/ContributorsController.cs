using System;
using System.Collections.Generic;
using System.Data.Common.EntitySql;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcApplication12.Models;
using SimchaFund.Data;

namespace MvcApplication12.Controllers
{
    public class ContributorsController : Controller
    {
        public ActionResult Index()
        {
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
            }
            var vm = new ContributorsIndexViewModel();
            var mgr = new SimchaFundManager(Properties.Settings.Default.ConStr);
            vm.Contributors = mgr.GetContributors();
            return View(vm);
        }

        [HttpPost]
        public ActionResult New(Contributor contributor, decimal initialDeposit)
        {
            var mgr = new SimchaFundManager(Properties.Settings.Default.ConStr);
            mgr.AddContributor(contributor);
            var deposit = new Deposit
            {
                Amount = initialDeposit,
                ContributorId = contributor.Id,
                Date = contributor.Date
            };
            mgr.AddDeposit(deposit);
            TempData["Message"] = "New Contributor Created! Id: " + contributor.Id;
            return RedirectToAction("index");
        }

        [HttpPost]
        public ActionResult Edit(Contributor contributor)
        {
            var mgr = new SimchaFundManager(Properties.Settings.Default.ConStr);
            mgr.UpdateContributor(contributor);
            TempData["Message"] = "Contributor updated successfully";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Deposit(Deposit deposit)
        {
            var mgr = new SimchaFundManager(Properties.Settings.Default.ConStr);
            mgr.AddDeposit(deposit);
            TempData["Message"] = "Deposit of $" + deposit.Amount + " recorded successfully";
            return RedirectToAction("Index");
        }

        public ActionResult History(int contribId)
        {
            var mgr = new SimchaFundManager(Properties.Settings.Default.ConStr);
            IEnumerable<Deposit> deposits = mgr.GetDepositsById(contribId);
            IEnumerable<Contribution> contributions = mgr.GetContributionsById(contribId);

            IEnumerable<Transaction> transactions = deposits.Select(d => new Transaction
            {
                Action = "Deposit",
                Amount = d.Amount,
                Date = d.Date
            }).Concat(contributions.Select(c => new Transaction
            {
                Action = "Contribution for the " + c.SimchaName + " simcha",
                Amount = -c.Amount,
                Date = c.Date
            })).OrderByDescending(t => t.Date);

            var vm = new HistoryViewModel();
            vm.Transactions = transactions;

            return View(vm);
        }
    }

    //public static class MyExtensions
    //{
    //    public static IEnumerable<TResult> MySelect<TSource, TResult>(this IEnumerable<TSource> source,
    //        Func<TSource, TResult> func)
    //    {
    //        List<TResult> result = new List<TResult>();
    //        foreach (TSource obj in source)
    //        {
    //            result.Add(func(obj));
    //        }

    //        return result;
    //    }
    //}
}
