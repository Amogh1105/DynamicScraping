using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using ScrapingMVC.Models;
using Tesseract;
using System.Diagnostics;
using System.Text.RegularExpressions;
using ScrapingMVC.Helpers;


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
            cd.customers6 = dal.GetAnnualDetails(CinNumber, cd);


           var dd = cd.customers.Concat(cd.customers1);
           var ee= dd.Concat(cd.customers2);
           var ff=ee.Concat(cd.customers3);
           var gg=ff.Concat(cd.customers4);
           var hh = gg.Concat(cd.customers5);
           var ii = hh.Concat(cd.customers6);
            return View("Result",ii);
           
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



        public ActionResult EPFO()
        {
            var folderRootPath = HttpContext.Server.MapPath("~/images/");

            //string imagepath = "D:\\DairymanGit\\DynamicScraping\\ScrapingMVC\\images\\";
        var xd= dal.GenerateSnapshot(folderRootPath);
          using (var engine = new TesseractEngine(Server.MapPath(@"~/tessdata"), "eng",EngineMode.Default))
        {

    Page ocrPage = engine.Process(Pix.LoadFromFile(xd), PageSegMode.SingleBlock);
    var text = ocrPage.GetText();
    text = Regex.Replace(text, @"\t|\n|\r|\s", "");
    text = text.Trim(' ');
    char[] textArray = null;
    textArray = text.ToCharArray();

    ViewBag.res = text;
    ViewBag.Result = true;
    ViewBag.mean = String.Format("{0:p}", ocrPage.GetMeanConfidence());
    return View();

        }   
        }
        
        [ScrapingMVC.MvcApplication.NoDirectAccess]
        public ActionResult TotalItems()
        {
            BreadCrumb = "Admin/AllItems";
            CompanyDetails cd = new CompanyDetails();
            cd.customers = dal.GetTotalCompanyCINCount();
            return View(cd.customers);
        }

        [HttpGet]
        public ActionResult GST()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GST(string GSTSearch, FormCollection form)
        {
            string fiscalyear = form["FiscalYear"].ToString();

            var resBodyJson = GSTNDataHelper.GetPublicReturnStatus(GSTSearch, fiscalyear);
            List<ReturnStatus> eList = new List<ReturnStatus>();
            foreach (var item in resBodyJson)
            {
                var list = item.EFiledlist;
                if (list != null)
                {

                    foreach (var listitem in list)
                    {
                        ReturnStatus rt = new ReturnStatus();

                        rt.arn = listitem.arn;
                        rt.dof = listitem.dof;
                        rt.mof = listitem.mof;
                        rt.rtntype = listitem.rtntype;
                        rt.ret_prd = listitem.ret_prd;
                        rt.status = listitem.status;
                        rt.valid = listitem.valid;

                        eList.Add(rt);

                    }

                }
                else
                {
                    break;
                }


            }

            return View("ReturnStatus",eList);
        }

        public string BreadCrumb { set { @ViewBag.BreadCrumb = value; } }

    }
}