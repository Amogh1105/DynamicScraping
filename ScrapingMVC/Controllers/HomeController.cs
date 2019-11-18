using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using ScrapingMVC.Models;
using System.Diagnostics;

namespace ScrapingMVC.Controllers
{
    public class HomeController : Controller
    {
        DAL dal = new DAL();

        public ActionResult Index()
        {
            BreadCrumb = "Admin/CompanyDetails";

            return View();
        }

        [HttpPost]
        [ScrapingMVC.MvcApplication.NoDirectAccess]
        public ActionResult Index(string CinNumber,CompanyDetails cd)
        {
            BreadCrumb = "Admin/CompanyData";
            cd.customers = dal.GetNewDetails(CinNumber, cd);
            cd.customers1 = dal.GetShareCapital(CinNumber, cd);
            cd.customers2 = dal.GetComplianceDetails(CinNumber, cd);
            cd.customers3 = dal.GetDirectorDetails(CinNumber, cd);
            cd.customers4 = dal.GetChargesDetails(CinNumber, cd);
            cd.customers5 = dal.GetEstablishmentDetails(CinNumber, cd);



            //Process process = new Process();
            //string Locat = ConfigurationManager.AppSettings["EXELOG"];

            //string filepath = Locat + "\\" + "ConsoleBatchFile.exe";
            //process.StartInfo.FileName = filepath;
            //process.StartInfo.Verb = "runas";
            //process.StartInfo.CreateNoWindow = false;
            //process.StartInfo.UseShellExecute = true;

            //process.Start();
            //process.Close();


           var dd = cd.customers.Concat(cd.customers1);
           var ee= dd.Concat(cd.customers2);
           var ff=ee.Concat(cd.customers3);
           var gg=ff.Concat(cd.customers4);
           var hh = gg.Concat(cd.customers5);
            return View("Result",hh);
           
        }

       

        public ActionResult Dashboard()
        {
            BreadCrumb = "Admin/Dashboard";

            return View();
        }

        public ActionResult Contact()
        {

            return View();
        }

        public string BreadCrumb { set { @ViewBag.BreadCrumb = value; } }

    }
}