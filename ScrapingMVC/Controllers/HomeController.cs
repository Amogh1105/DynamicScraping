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
            ModelState.Clear();
            List<string> model = new List<string>();
            model = dal.GetValues();
            ViewBag.CinCount = model[0];
            ViewBag.CompanyCount = model[1];
           
            return View();
        }

        public JsonResult DrawReportsOnDashBoard(string datevalue)
        {
            // return null;
                
                var results = dal.GetStatistics(datevalue);
                var jsonResult = Json(results, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            
           
                // return RedirectToAction("Error", "Home");
                //return Json(new
                //{
                //    redirectUrl = Url.Action("Error", "Home"),
                //    isRedirect = true
                //});
            }
        


        //public ActionResult EPFO()
        //{
        //    var folderRootPath = HttpContext.Server.MapPath("~/images/");

        //    string imagepath = "D:\\DairymanGit\\DynamicScraping\\ScrapingMVC\\images\\";
        //    dal.GenerateSnapshot(folderRootPath);
        //    return View();
        //}
        
        [ScrapingMVC.MvcApplication.NoDirectAccess]
        public ActionResult TotalItems()
        {
            BreadCrumb = "Admin/AllItems";
            CompanyDetails cd = new CompanyDetails();
            cd.customers = dal.GetTotalCompanyCINCount();
            return View(cd.customers);
        }
        public string BreadCrumb { set { @ViewBag.BreadCrumb = value; } }

    }
}