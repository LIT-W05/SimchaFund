using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcApplication12.Models;
using SimchaFund.Data;

namespace MvcApplication12.Controllers
{
    public class SimchasController : Controller
    {
        public ActionResult Index()
        {
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
            }
            var mgr = new SimchaFundManager(Properties.Settings.Default.ConStr);
            var viewModel = new SimchaIndexViewModel();
            viewModel.TotalContributors = mgr.GetContributorCount();
            viewModel.Simchas = mgr.GetAllSimchas();
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult New(string name, DateTime date)
        {
            var mgr = new SimchaFundManager(Properties.Settings.Default.ConStr);
            Simcha simcha = new Simcha { Name = name, Date = date };
            mgr.AddSimcha(simcha);
            TempData["Message"] = "New Simcha Created! Id: " + simcha.Id;
            return RedirectToAction("index");
        }

        public ActionResult Contributions(int simchaId)
        {
            var mgr = new SimchaFundManager(Properties.Settings.Default.ConStr);
            Simcha simcha = mgr.GetSimchaById(simchaId);
            IEnumerable<SimchaContributor> contributors = mgr.GetSimchaContributorsEasy(simchaId);

            var viewModel = new ContributionsViewModel
            {
                Contributors = contributors,
                Simcha = simcha
            };

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult UpdateContributions(List<ContributionInclusion> contributors, int simchaId)
        {
            var mgr = new SimchaFundManager(Properties.Settings.Default.ConStr);
            mgr.UpdateSimchaContributions(simchaId, contributors);
            TempData["Message"] = "Simcha updated successfully";
            return RedirectToAction("Index");
        }

    }
}
