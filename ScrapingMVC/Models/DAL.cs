using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Configuration;
using MySql.Data.MySqlClient;
using System.Text;
using System.IO;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Collections;
using System.Drawing;
using System.Web.UI;
using Tesseract;

namespace ScrapingMVC.Models
{
    public class DAL
    {

        public void Scrapper()
        {
            using (var browser = new ChromeDriver())
            {

                // Go to the home page
                browser.Manage().Window.Maximize();
                browser.Navigate().GoToUrl("https://www.zaubacorp.com/");

                WebDriverWait wait = new WebDriverWait(browser, TimeSpan.FromSeconds(40));
                wait.Until(d => d.FindElement(By.Id("searchid")));
                System.Threading.Thread.Sleep(5000);


                //PROXY

                //browser.FindElementByXPath("//*[@id='url-form-wrap']/form/div[2]/button").Click();

                //System.Threading.Thread.Sleep(5000);


                //GET DATA FROM UNINSERTED TABLE

                string query = ConfigurationManager.ConnectionStrings["connString"].ConnectionString;
                using (MySqlConnection con = new MySqlConnection(query))
                {


                    MySqlCommand cmd = new MySqlCommand("select * from uninsertedcompanies order by uninsertedid desc", con);
                    cmd.CommandType = CommandType.Text;
                    con.Open();
                    List<CompanyDetails> customers = new List<CompanyDetails>();

                    CompanyDetails cd = new CompanyDetails();
                    MySqlDataAdapter md = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    md.Fill(dt);
                    foreach (DataRow dr in dt.Rows)
                    {

                        customers.Add(new CompanyDetails
                        {
                            primarykey = Convert.ToInt32(dr["uninsertedid"].ToString()),
                            companynamefromdb = dr["companyname"].ToString(),
                            cinvaluefromdb = dr["cin"].ToString()
                        });


                        var jpk = customers[0].primarykey;
                        var jcompanyname = customers[0].companynamefromdb;
                        var jcin = customers[0].cinvaluefromdb;


                        if (jcompanyname == "")
                        {

                            // Get the page elements
                            var userNameField = browser.FindElementByName("searchvalue");
                            userNameField.Clear();
                            userNameField.SendKeys(jcin);
                        }
                        else
                        {
                            var userNameField = browser.FindElementByName("searchvalue");
                            userNameField.Clear();

                            userNameField.SendKeys(jcompanyname);
                        }
                        System.Threading.Thread.Sleep(10000);
                        browser.FindElementByXPath("//div[@id='result']//div[@class='show'][1]").Click();

                        //Name or CIN IS CLICKED IN BROWSER!!

                        //SCRAP DATA
                        //COMPANY DETAILS STARTS
                        var companycin = browser.FindElementByXPath("//div[@class='col-lg-12 col-md-12 col-sm-12 col-xs-12'][last()-4]//table//thead//tr/td[2]").Text;
                        var title = browser.FindElementsByXPath("//div[@class='col-lg-12 col-md-12 col-sm-12 col-xs-12'][last()-4]//tbody//tr");

                        if (title != null)
                        {

                            DateTime now = DateTime.Now;
                            string datevalue = now.ToString("yyyy-MM-dd");

                            var headings = browser.FindElementsByXPath("//div[@class='col-lg-12 col-md-12 col-sm-12 col-xs-12'][last()-4]//tbody//tr//td");
                            var runtime = now;

                            MySqlCommand cmd2 = new MySqlCommand("INSERT INTO companydetails(cin,companyname,companystatus,roc,registrationnumber,companycategory,companysubcategory,companyclass,dateofincorporation,ageofcompany,activity,numberofmembers,runtime,datevalue) VALUES(@cin,@companyname,@companystatus,@roc,@registrationnumber,@companycategory,@companysubcategory,@companyclass,@dateofincorporation,@ageofcompany,@activity,@numberofmembers,@runtime, @datevalue)", con);
                            cmd2.CommandType = CommandType.Text;
                            cmd2.Parameters.AddWithValue("@cin", companycin);
                            cmd2.Parameters.AddWithValue("@companyname", headings[1].Text);
                            cmd2.Parameters.AddWithValue("@companystatus", headings[3].Text);
                            cmd2.Parameters.AddWithValue("@roc", headings[5].Text);
                            cmd2.Parameters.AddWithValue("@registrationnumber", headings[7].Text);
                            cmd2.Parameters.AddWithValue("@companycategory", headings[9].Text);
                            cmd2.Parameters.AddWithValue("@companysubcategory", headings[11].Text);
                            cmd2.Parameters.AddWithValue("@companyclass", headings[13].Text);
                            cmd2.Parameters.AddWithValue("@dateofincorporation", headings[15].Text);
                            cmd2.Parameters.AddWithValue("@ageofcompany", headings[17].Text);
                            cmd2.Parameters.AddWithValue("@activity", headings[19].Text);
                            cmd2.Parameters.AddWithValue("@numberofmembers", headings[21].Text);
                            cmd2.Parameters.AddWithValue("@runtime", now);
                            cmd2.Parameters.AddWithValue("@datevalue", datevalue);

                            cmd2.ExecuteNonQuery();


                            //Now Company Details are inserted leave 10s gap
                            System.Threading.Thread.Sleep(10000);


                            //SHARE CAPITAL STARTS
                            var demo = browser.FindElementsByXPath("//div[@class='col-lg-12 col-md-12 col-sm-12 col-xs-12'][last()-1]//tbody//tr//td");
                            var ac = demo[1].Text;
                            var pc = demo[3].Text;
                            var authorizedcapital = ac.Remove(0, 1);
                            var paidupcapital = pc.Remove(0, 1);
                            MySqlCommand cmd3 = new MySqlCommand("INSERT INTO sharecapital (cin,companyname,authorizedcapital,paidupcapital,runtime,datevalue)VALUES (@cin,@companyname,@authorizedcapital,@paidupcapital,@runtime, @datevalue)", con);
                            cmd3.CommandType = CommandType.Text;
                            cmd3.Parameters.AddWithValue("@cin", companycin);
                            cmd3.Parameters.AddWithValue("@companyname", headings[1].Text);
                            cmd3.Parameters.AddWithValue("@authorizedcapital", authorizedcapital);
                            cmd3.Parameters.AddWithValue("@paidupcapital", paidupcapital);
                            cmd3.Parameters.AddWithValue("@runtime", now);
                            cmd3.Parameters.AddWithValue("@datevalue", datevalue);

                            cmd3.ExecuteNonQuery();

                            //Now Share Capital Details are inserted leave 5s gap
                            System.Threading.Thread.Sleep(5000);


                            //ANNUAL COMPLIANCE STARTS



                            var demo1 = browser.FindElementsByXPath("//div[@class='col-lg-12 col-md-12 col-sm-12 col-xs-12'][position()=5]//tbody//tr//td");

                            var listingstatus = browser.FindElementByXPath("//div[@class='col-lg-12 col-md-12 col-sm-12 col-xs-12'][position()=5]//thead//tr//td[2]").Text;
                            MySqlCommand cmd4 = new MySqlCommand("INSERT INTO annualcompliance (cin,companyname,listingstatus,dateoflastgeneralmeeting,dateoflastestbalancesheet,runtime,datevalue)VALUES (@cin,@companyname,@listingstatus,@dateoflastgeneralmeeting,@dateoflastestbalancesheet,@runtime,@datevalue)", con);
                            cmd4.CommandType = CommandType.Text;
                            cmd4.Parameters.AddWithValue("@cin", companycin);
                            cmd4.Parameters.AddWithValue("@companyname", headings[1].Text);
                            cmd4.Parameters.AddWithValue("@listingstatus", listingstatus);
                            cmd4.Parameters.AddWithValue("@dateoflastgeneralmeeting", demo1[1].Text);

                            cmd4.Parameters.AddWithValue("@dateoflastestbalancesheet", demo1[3].Text);
                            cmd4.Parameters.AddWithValue("@runtime", now);
                            cmd4.Parameters.AddWithValue("@datevalue", datevalue);

                            cmd4.ExecuteNonQuery();

                            System.Threading.Thread.Sleep(5000);

                            //Now Annual Compliance Details are Inserted



                            //DIRECTOR DETAILS START


                            var directortable = browser.FindElementByXPath("//*[@id='block-system-main']/div[2]/div[1]/div[7]/table");
                            if (directortable.Displayed)
                            {
                                IReadOnlyCollection<IWebElement> rowsr = browser.FindElementsByXPath(("//*[@id='block-system-main']/div[2]/div[1]/div[7]/table//tbody//tr[@class='accordion-toggle main-row']"));
                                IReadOnlyCollection<IWebElement> colsc = browser.FindElementsByXPath(("//*[@id='block-system-main']/div[2]/div[1]/div[7]/table//tbody//tr[@class='accordion-toggle main-row']//td[not(@width)]"));

                                String strRowData = "";
                                // Traverse each row
                                foreach (var elemTr in rowsr)
                                {
                                    List<string> all = new List<string>();

                                    List<IWebElement> lstTdElem = new List<IWebElement>(elemTr.FindElements(By.XPath("td[not(@width)]")));
                                    if (lstTdElem.Count > 0)
                                    {
                                        // Traverse each column
                                        foreach (var elemTd in lstTdElem)
                                        {
                                            // "\t\t" is used for Tab Space between two Text
                                            strRowData = strRowData + elemTd.Text + "\t\t";
                                            all.Add(elemTd.Text);

                                        }

                                        MySqlCommand cmd5 = new MySqlCommand("INSERT  INTO directordetails (cin,companyname,din,directorname,designation,appointmentdate,link,runtime,datevalue)VALUES (@cin,@companyname,@din,@directorname,@designation,@appointmentdate,@link,@runtime,@datevalue)", con);
                                        cmd5.CommandType = CommandType.Text;
                                        cmd5.Parameters.AddWithValue("@cin", companycin);
                                        cmd5.Parameters.AddWithValue("@companyname", headings[1].Text);
                                        cmd5.Parameters.AddWithValue("@din", all[0]);
                                        cmd5.Parameters.AddWithValue("@directorname", all[1]);
                                        cmd5.Parameters.AddWithValue("@designation", all[2]);
                                        cmd5.Parameters.AddWithValue("@appointmentdate", all[3]);

                                        cmd5.Parameters.AddWithValue("@link", all[4]);
                                        cmd5.Parameters.AddWithValue("@runtime", now);
                                        cmd5.Parameters.AddWithValue("@datevalue", datevalue);

                                        cmd5.ExecuteNonQuery();

                                    }



                                }

                            }
                            else
                            {
                                //Director Table not exists
                            }


                            //CHARGES DATAA STARTS


                            var chargestable = browser.FindElementsByXPath("//table[@id='charges']");
                            var nofound = browser.FindElementByXPath("//table[@id='charges']//tbody//tr//td//p").Text;
                            if (nofound != "No charges found")
                            {
                                IReadOnlyCollection<IWebElement> rowsr = browser.FindElementsByXPath(("//table[@id='charges']//tbody//tr"));
                                IReadOnlyCollection<IWebElement> colsc = browser.FindElementsByXPath(("//table[@id='charges']//thead//tr//td"));

                                String strRowData = "";
                                // Traverse each row
                                foreach (var elemTr in rowsr)
                                {
                                    List<string> allvalues = new List<string>();

                                    List<IWebElement> lstTdElem = new List<IWebElement>(elemTr.FindElements(By.XPath("td")));
                                    if (lstTdElem.Count > 0)
                                    {
                                        // Traverse each column
                                        foreach (var elemTd in lstTdElem)
                                        {
                                            // "\t\t" is used for Tab Space between two Text
                                            strRowData = strRowData + elemTd.Text + "\t\t";
                                            allvalues.Add(elemTd.Text);

                                        }

                                        MySqlCommand cmd6 = new MySqlCommand("INSERT  INTO chargesdetails (cin,companyname,chargeid,creationdate,modificationdate,closuredate,assetsundercharge,amount,chargeholder,runtime,datevalue)VALUES (@cin,@companyname,@chargeid,@creationdate,@modificationdate,@closuredate,@assetsundercharge,@amount,@chargeholder,@runtime,@datevalue)", con);
                                        cmd6.CommandType = CommandType.Text;
                                        cmd6.Parameters.AddWithValue("@cin", companycin);
                                        cmd6.Parameters.AddWithValue("@companyname", headings[1].Text);
                                        cmd6.Parameters.AddWithValue("@chargeid", allvalues[0]);
                                        cmd6.Parameters.AddWithValue("@creationdate", allvalues[1]);
                                        cmd6.Parameters.AddWithValue("@modificationdate", allvalues[2]);
                                        cmd6.Parameters.AddWithValue("@closuredate", allvalues[3]);
                                        cmd6.Parameters.AddWithValue("@assetsundercharge", allvalues[4]);
                                        cmd6.Parameters.AddWithValue("@amount", allvalues[5]);
                                        cmd6.Parameters.AddWithValue("@chargeholder", allvalues[6]);
                                        cmd6.Parameters.AddWithValue("@runtime", now);
                                        cmd6.Parameters.AddWithValue("@datevalue", datevalue);

                                        cmd6.ExecuteNonQuery();

                                    }



                                }

                            }
                            else
                            {
                                //Charges Data not exists
                            }




                            var establishmenttable = browser.FindElementByXPath("//div[@class='col-12'][position()=3]//table");
                            IReadOnlyCollection<IWebElement> establishmentrows = browser.FindElementsByXPath(("//div[@class='col-12'][position()=3]//tbody//tr"));
                            var noestablishmentfound = browser.FindElementByXPath("//div[@class='col-12'][position()=3]//table//tbody//tr//td//p").Text;

                            if (noestablishmentfound != "No establishments found")
                            {
                                IReadOnlyCollection<IWebElement> rowsr = browser.FindElementsByXPath(("//div[@class='col-12'][position()=3]//tbody//tr"));
                                IReadOnlyCollection<IWebElement> colsc = browser.FindElementsByXPath(("//div[@class='col-12'][position()=3]//table//thead//td"));

                                String strRowData = "";
                                // Traverse each row
                                foreach (var elemTr in rowsr)
                                {
                                    List<string> all = new List<string>();

                                    List<IWebElement> lstTdElem = new List<IWebElement>(elemTr.FindElements(By.XPath("td")));
                                    if (lstTdElem.Count > 0)
                                    {
                                        // Traverse each column
                                        foreach (var elemTd in lstTdElem)
                                        {
                                            // "\t\t" is used for Tab Space between two Text
                                            strRowData = strRowData + elemTd.Text + "\t\t";
                                            all.Add(elemTd.Text);

                                        }

                                        MySqlCommand cmd7 = new MySqlCommand("INSERT  INTO establishmentdetails (cin,companyname,establishmentname,city,pincode,address,runtime,datevalue)VALUES (@cin,@companyname,@establishmentname,@city,@pincode,@address,@runtime,@datevalue)", con);
                                        cmd7.CommandType = CommandType.Text;
                                        cmd7.Parameters.AddWithValue("@cin", companycin);
                                        cmd7.Parameters.AddWithValue("@companyname", headings[1].Text);
                                        cmd7.Parameters.AddWithValue("@establishmentname", all[0]);
                                        cmd7.Parameters.AddWithValue("@city", all[1]);
                                        cmd7.Parameters.AddWithValue("@pincode", all[2]);
                                        cmd7.Parameters.AddWithValue("@address", all[3]);
                                        cmd7.Parameters.AddWithValue("@runtime", now);
                                        cmd7.Parameters.AddWithValue("@datevalue", datevalue);


                                        cmd7.ExecuteNonQuery();

                                    }



                                }

                            }
                            else
                            {
                                //Establishment Data not exists
                            }

                        }

                        else
                        {
                            // if title is null if table contains no data
                        }
                        browser.Close();

                        //browser.Quit();

                        browser.Dispose();

                        //Delete the entry from uninserted table

                        MySqlCommand cmd8 = new MySqlCommand("delete from uninsertedcompanies where uninsertedid = @uninsertedid", con);
                        cmd8.CommandType = CommandType.Text;
                        cmd8.Parameters.AddWithValue("@uninsertedid", jpk);
                        cmd8.ExecuteNonQuery();



                    }
                }
            }
        }


        public Bitmap Resize(Bitmap bmp, int newWidth, int newHeight)
        {
            Bitmap temp = (Bitmap)bmp;
            Bitmap bmap = new Bitmap(newWidth, newHeight, temp.PixelFormat);

            double nWidthFactor = (double)temp.Width / (double)newWidth;
            double nHeightFactor = (double)temp.Height / (double)newHeight;

            double fx, fy, nx, ny;
            int cx, cy, fr_x, fr_y;
            Color color1 = new Color();
            Color color2 = new Color();
            Color color3 = new Color();
            Color color4 = new Color();
            byte nRed, nGreen, nBlue;

            byte bp1, bp2;

            for (int x = 0; x < bmap.Width; ++x)
            {
                for (int y = 0; y < bmap.Height; ++y)
                {
                    fr_x = (int)Math.Floor(x * nWidthFactor);
                    fr_y = (int)Math.Floor(y * nHeightFactor);

                    cx = fr_x + 1;
                    if (cx >= temp.Width)
                        cx = fr_x;

                    cy = fr_y + 1;
                    if (cy >= temp.Height)
                        cy = fr_y;

                    fx = x * nWidthFactor - fr_x;
                    fy = y * nHeightFactor - fr_y;
                    nx = 1.0 - fx;
                    ny = 1.0 - fy;

                    color1 = temp.GetPixel(fr_x, fr_y);
                    color2 = temp.GetPixel(cx, fr_y);
                    color3 = temp.GetPixel(fr_x, cy);
                    color4 = temp.GetPixel(cx, cy);

                    // Blue
                    bp1 = (byte)(nx * color1.B + fx * color2.B);
                    bp2 = (byte)(nx * color3.B + fx * color4.B);
                    nBlue = (byte)(ny * (double)(bp1) + fy * (double)(bp2));

                    // Green
                    bp1 = (byte)(nx * color1.G + fx * color2.G);
                    bp2 = (byte)(nx * color3.G + fx * color4.G);
                    nGreen = (byte)(ny * (double)(bp1) + fy * (double)(bp2));

                    // Red
                    bp1 = (byte)(nx * color1.R + fx * color2.R);
                    bp2 = (byte)(nx * color3.R + fx * color4.R);
                    nRed = (byte)(ny * (double)(bp1) + fy * (double)(bp2));

                    bmap.SetPixel(x, y, System.Drawing.Color.FromArgb(255, nRed, nGreen, nBlue));
                }
            }

            //here i included the below to functions logic without the for loop to remove repetitive use of for loop but it did not work and taking the same time.
            bmap = SetGrayscale(bmap);
            bmap = RemoveNoise(bmap);

            return bmap;
        }

        //SetGrayscale
        public Bitmap SetGrayscale(Bitmap img)
        {
            Bitmap temp = (Bitmap)img;
            Bitmap bmap = (Bitmap)temp.Clone();
            Color c;
            for (int i = 0; i < bmap.Width; i++)
            {
                for (int j = 0; j < bmap.Height; j++)
                {
                    c = bmap.GetPixel(i, j);
                    byte gray = (byte)(.299 * c.R + .587 * c.G + .114 * c.B);

                    bmap.SetPixel(i, j, Color.FromArgb(gray, gray, gray));
                }
            }
            return (Bitmap)bmap.Clone();
        }

        //RemoveNoise
        public Bitmap RemoveNoise(Bitmap bmap)
        {
            for (var x = 0; x < bmap.Width; x++)
            {
                for (var y = 0; y < bmap.Height; y++)
                {
                    var pixel = bmap.GetPixel(x, y);
                    if (pixel.R < 162 && pixel.G < 162 && pixel.B < 162)
                        bmap.SetPixel(x, y, Color.Black);
                }
            }

            for (var x = 0; x < bmap.Width; x++)
            {
                for (var y = 0; y < bmap.Height; y++)
                {
                    var pixel = bmap.GetPixel(x, y);
                    if (pixel.R > 162 && pixel.G > 162 && pixel.B > 162)
                        bmap.SetPixel(x, y, Color.White);
                }
            }
            return bmap;
        }

        public string GenerateSnapshot(string filePath)
        {
            IWebDriver driver = new ChromeDriver();
            driver.Manage().Window.Maximize(); driver.Navigate().GoToUrl("https://unifiedportal-epfo.epfindia.gov.in/publicPortal/no-auth/misReport/home/loadEstSearchHome");
            System.Threading.Thread.Sleep(10000);

            var remElement = driver.FindElement(By.Id("captchaImg"));
            Point location = remElement.Location;

            var screenshot = (driver as ChromeDriver).GetScreenshot();
            using (MemoryStream stream = new MemoryStream(screenshot.AsByteArray))
            {
                using (Bitmap bitmap = new Bitmap(stream))
                {
                  var  bmap = SetGrayscale(bitmap);
                    bmap = RemoveNoise(bmap);
                    RectangleF part = new RectangleF(location.X, location.Y, remElement.Size.Width, remElement.Size.Height);
                    using (Bitmap bn = bmap.Clone(part, bmap.PixelFormat))
                    {
                        bn.Save(filePath + "CaptchImage.png", System.Drawing.Imaging.ImageFormat.Png);
                    }
                }
            }
            driver.Close();
            driver.Dispose();
            return filePath + "CaptchImage.png";
       
        }
            //reading text from images

        // using (var engine = new TesseractEngine(@"D:\DairymanGit\DynamicScraping\ScrapingMVC\tesseract-ocr\tessdata", "eng",EngineMode.Default))
        //    {

        //    Tesseract.Page ocrPage = engine.Process(Pix.LoadFromFile(filePath + "CaptchImage.png"), PageSegMode.AutoOnly);
        //    var captchatext = ocrPage.GetText();
        //    }
        //    driver.Close();
        //    driver.Dispose();
        //}

        public void MCAAnnualScrapper()
        {
            using (var browser = new ChromeDriver())
            {
                var options = new ChromeOptions();

                options.AddArgument("no-sandbox");
                // Go to the home page
                browser.Manage().Window.Maximize();
                //browser.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(20);
               
                    browser.Navigate().GoToUrl("http://mca.gov.in/mcafoportal/showCheckFilingStatus.do");
                
               

                WebDriverWait wait = new WebDriverWait(browser, TimeSpan.FromSeconds(40));
                wait.Until(d => d.FindElement(By.Id("companiesact1")));
                System.Threading.Thread.Sleep(5000);
         

                string query = ConfigurationManager.ConnectionStrings["connString"].ConnectionString;
                using (MySqlConnection con = new MySqlConnection(query))
                {


                    MySqlCommand cmd = new MySqlCommand("select * from uninsertedannualdetails order by uninsertedannualid desc", con);
                    cmd.CommandType = CommandType.Text;
                    con.Open();
                    List<CompanyDetails> customers = new List<CompanyDetails>();

                    CompanyDetails cd = new CompanyDetails();
                    MySqlDataAdapter md = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    md.Fill(dt);
                    foreach (DataRow dr in dt.Rows)
                    {

                        customers.Add(new CompanyDetails
                        {
                            primarykey = Convert.ToInt32(dr["uninsertedannualid"].ToString()),
                            companynamefromdb = dr["companyname"].ToString(),
                            cinvaluefromdb = dr["cin"].ToString()
                        });


                        var jpk = customers[0].primarykey;
                        var jcompanyname = customers[0].companynamefromdb;
                        var jcin = customers[0].cinvaluefromdb;


                        //If CIN IS GIVEN
                        if (jcompanyname == "")
                        {

                            // Get the page elements
                            var userNameField = browser.FindElementByName("companyID");
                            userNameField.Clear();
                            userNameField.SendKeys(jcin);
                        }

                            //IF COMPANY IS GIVEN
                        else
                        {
                            browser.FindElementByXPath("//*[@id='imgSearchIcon']").Click();
                            System.Threading.Thread.Sleep(5000);
                            var userNameField = browser.FindElementByName("searchcompanyname");
                            userNameField.Clear();
                            userNameField.SendKeys(jcompanyname);
                            //click search after giving companyname
                            browser.FindElementByXPath("//*[@id='findcindata']").Click();
                            System.Threading.Thread.Sleep(10000);
                            //browser.FindElement(By.XPath(@"//*[@id='cinlist']/tbody/tr/td[1]/a[contains(@onclick,'"+jcompanyname+"')]")).Click();

                            //click company name
                            browser.FindElementByXPath("//*[@id='cinlist']/tbody/tr/td[1]/a").Click();

                         

                        }
                        System.Threading.Thread.Sleep(5000);
                        //click submit button
                        browser.FindElementByXPath("//*[@id='submitBtn']").Click();

                        var rows = (browser.FindElementsByXPath("//table[@id='results']//tbody//tr[@class='table-row'][not(@style)]"));
	                    var cols = (browser.FindElementsByXPath("//table[@id='results']//tbody//tr[@class='table-header']/th"));

                        if (rows != null)
                        {
                            var companyname = browser.FindElementByXPath("//form[@id='checkFiling']//table[@class='input-forms']//tbody//tr[1]//td/input");
                            var companynames = companyname.GetAttribute("value");
                            var cin = browser.FindElementByXPath("//form[@id='checkFiling']//table[@class='input-forms']//tbody//tr[2]//td/input");
                            var cinvalue = cin.GetAttribute("value");
                            IReadOnlyCollection<IWebElement> rowsr = browser.FindElementsByXPath(("//table[@id='results']//tbody//tr[@class='table-row'][not(@style)]"));
                            IReadOnlyCollection<IWebElement> colsc = browser.FindElementsByXPath(("//table[@id='results']//tbody//tr[@class='table-header']/th"));
                            DateTime now = DateTime.Now;
                            string datevalue = now.ToString("yyyy-MM-dd");

                            String strRowData = "";
                            // Traverse each row
                            foreach (var elemTr in rowsr)
                            {


                                List<string> all = new List<string>();

                                List<IWebElement> lstTdElem = new List<IWebElement>(elemTr.FindElements(By.XPath("td")));
                                if (lstTdElem.Count > 0)
                                {
                                    // Traverse each column
                                    foreach (var elemTd in lstTdElem)
                                    {
                                        // "\t\t" is used for Tab Space between two Text
                                        strRowData = strRowData + elemTd.Text + "\t\t";
                                        all.Add(elemTd.Text);

                                    }

                                    MySqlCommand cmd9 = new MySqlCommand("INSERT  INTO annualdetails (cin,companyname,srn,eformname,eventdate,challancopy,runtime,datevalue)VALUES (@cin,@companyname,@srn,@eformname,@eventdate,@challancopy,@runtime,@datevalue)", con);
                                    cmd9.CommandType = CommandType.Text;
                                    cmd9.Parameters.AddWithValue("@cin", cinvalue);
                                    cmd9.Parameters.AddWithValue("@companyname", companynames);
                                    cmd9.Parameters.AddWithValue("@srn", all[0]);
                                    cmd9.Parameters.AddWithValue("@eformname", all[1]);
                                    cmd9.Parameters.AddWithValue("@eventdate", all[2]);
                                    cmd9.Parameters.AddWithValue("@challancopy", all[3]);
                                    cmd9.Parameters.AddWithValue("@runtime", now);
                                    cmd9.Parameters.AddWithValue("@datevalue", datevalue);
                                    cmd9.ExecuteNonQuery();

                                }



                            }

                            /////////////////////Insert Hidden Content///////////////////////////////////////////////////////////

                            var races_element1 = browser.FindElementByXPath("//table[@id='results']//tbody//tr[@class='table-row'][@style='display: none;']");
                            if (races_element1 != null)
                            {

                                IReadOnlyCollection<IWebElement> rowsr1 = browser.FindElementsByXPath(("//table[@id='results']//tbody//tr[@class='table-row'][@style='display: none;']"));
                                IReadOnlyCollection<IWebElement> colsc1 = browser.FindElementsByXPath(("//table[@id='results']//tbody//tr[@class='table-header']/th"));

                                String strRowData1 = "";
                                // Traverse each row
                                foreach (var elemTr in rowsr1)
                                {


                                    List<string> all = new List<string>();

                                    List<IWebElement> lstTdElem = new List<IWebElement>(elemTr.FindElements(By.XPath("td")));
                                    if (lstTdElem.Count > 0)
                                    {
                                        // Traverse each column
                                        foreach (var elemTd in lstTdElem)
                                        {
                                            // "\t\t" is used for Tab Space between two Text
                                            strRowData1 = strRowData1 + elemTd.Text + "\t\t";
                                            all.Add(elemTd.GetAttribute("textContent"));

                                        }

                                        MySqlCommand cmd9 = new MySqlCommand("INSERT  INTO annualdetails (cin,companyname,srn,eformname,eventdate,challancopy,runtime,datevalue)VALUES (@cin,@companyname,@srn,@eformname,@eventdate,@challancopy,@runtime,@datevalue)", con);
                                        cmd9.CommandType = CommandType.Text;
                                        cmd9.Parameters.AddWithValue("@cin", cinvalue);
                                        cmd9.Parameters.AddWithValue("@companyname", companynames);
                                        cmd9.Parameters.AddWithValue("@srn", all[0]);
                                        cmd9.Parameters.AddWithValue("@eformname", all[1]);
                                        cmd9.Parameters.AddWithValue("@eventdate", all[2]);
                                        cmd9.Parameters.AddWithValue("@challancopy", all[3]);
                                        cmd9.Parameters.AddWithValue("@runtime", now);
                                        cmd9.Parameters.AddWithValue("@datevalue", datevalue);
                                        cmd9.ExecuteNonQuery();

                                    }



                                }


                            } //end of if races element1
                        }

                        else
                        {
                            //No rows exists.
                        }
                        browser.Close();
                        //browser.Quit();

                        browser.Dispose();

                        //Delete the entry from uninserted table

                        MySqlCommand cmd8 = new MySqlCommand("delete from uninsertedannualdetails where uninsertedannualid = @uninsertedannualid", con);
                        cmd8.CommandType = CommandType.Text;
                        cmd8.Parameters.AddWithValue("@uninsertedannualid", jpk);
                        cmd8.ExecuteNonQuery();
                        

                    }
                }
            }
            }
        public List<CompanyDetails> GetNewDetails(string CompanyName, CompanyDetails cd)
        {

            string query = ConfigurationManager.ConnectionStrings["connString"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(query))
            {
                con.Open();

                List<CompanyDetails> customers = new List<CompanyDetails>();
                if (cd.cins == "CIN")
                {
                    MySqlCommand check_User_Name = new MySqlCommand("SELECT COUNT(*) FROM companydetails WHERE cin = @cin", con);
                    check_User_Name.Parameters.AddWithValue("@cin", CompanyName);
                    var CinExist = Convert.ToInt32(check_User_Name.ExecuteScalar());

                    if (CinExist > 0)
                    {
                        //CIN exist

                        MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT c.cin, c.companyname,c.companystatus,c.roc,c.registrationnumber,c.companycategory,c.companysubcategory,c.companyclass,c.dateofincorporation,c.ageofcompany,c.activity,c.numberofmembers from companydetails c  where c.cin = @cin", con);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@cin", CompanyName);
                        MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        sd.Fill(dt);
                        foreach (DataRow dr in dt.Rows)
                        {
                            customers.Add(
                                new CompanyDetails
                                {
                                    cin = dr["cin"].ToString(),
                                    companyname = dr["companyname"].ToString(),
                                    companystatus = dr["companystatus"].ToString(),
                                    roc = dr["roc"].ToString(),
                                    registrationnumber = dr["registrationnumber"].ToString(),
                                    companycategory = dr["companycategory"].ToString(),
                                    companysubcategory = dr["companysubcategory"].ToString(),
                                    companyclass = dr["companyclass"].ToString(),
                                    dateofincorporation = dr["dateofincorporation"].ToString(),
                                    ageofcompany = dr["ageofcompany"].ToString(),
                                    activity = dr["activity"].ToString(),
                                    numberofmembers = dr["numberofmembers"].ToString()


                                });
                        }
                        return customers;
                    }




                    else
                    {
                        //CIn doesn't exist.
                        DateTime now = DateTime.Now;
                        MySqlCommand myCommand = new MySqlCommand("Insert into uninsertedcompanies(companyname,cin,runtime) values(@companyname, @cin,@now)", con);
                        myCommand.Parameters.AddWithValue("@companyname", "");
                        myCommand.Parameters.AddWithValue("@cin", CompanyName);
                        myCommand.Parameters.AddWithValue("@now", now);

                        myCommand.ExecuteNonQuery();

                        MySqlCommand myCommands = new MySqlCommand("Insert into uninsertedannualdetails(companyname,cin,runtime) values(@companynames, @cins, @runtimes)", con);
                        myCommands.Parameters.AddWithValue("@companynames", "");
                        myCommands.Parameters.AddWithValue("@cins",CompanyName);
                        myCommands.Parameters.AddWithValue("@runtimes", now);
                        myCommands.ExecuteNonQuery();

                        Scrapper();

                        MCAAnnualScrapper();


                        //EXECUTE BAT





                        MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT c.cin, c.companyname,c.companystatus,c.roc,c.registrationnumber,c.companycategory,c.companysubcategory,c.companyclass,c.dateofincorporation,c.ageofcompany,c.activity,c.numberofmembers from companydetails c  where c.cin = @cin", con);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@cin", CompanyName);
                        MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        sd.Fill(dt);
                        foreach (DataRow dr in dt.Rows)
                        {
                            customers.Add(
                                new CompanyDetails
                                {
                                    cin = dr["cin"].ToString(),
                                    companyname = dr["companyname"].ToString(),
                                    companystatus = dr["companystatus"].ToString(),
                                    roc = dr["roc"].ToString(),
                                    registrationnumber = dr["registrationnumber"].ToString(),
                                    companycategory = dr["companycategory"].ToString(),
                                    companysubcategory = dr["companysubcategory"].ToString(),
                                    companyclass = dr["companyclass"].ToString(),
                                    dateofincorporation = dr["dateofincorporation"].ToString(),
                                    ageofcompany = dr["ageofcompany"].ToString(),
                                    activity = dr["activity"].ToString(),
                                    numberofmembers = dr["numberofmembers"].ToString()


                                });
                        }
                        return customers;





                    }


                }
                else if (cd.companynames == "Company Name") //If Company Name is Given 
                {
                    MySqlCommand check_User_Name = new MySqlCommand("SELECT COUNT(*) FROM companydetails WHERE companyname like @cin", con);
                    check_User_Name.Parameters.AddWithValue("@cin", "%"+CompanyName+"%");
                    var CompanyExist = Convert.ToInt32(check_User_Name.ExecuteScalar());

                    if (CompanyExist > 0)
                    {
                        //Company exist

                        MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT c.cin, c.companyname,c.companystatus,c.roc,c.registrationnumber,c.companycategory,c.companysubcategory,c.companyclass,c.dateofincorporation,c.ageofcompany,c.activity,c.numberofmembers from companydetails c  where c.companyname like @companyname", con);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@companyname", "%"+CompanyName+"%");
                        MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        sd.Fill(dt);
                        foreach (DataRow dr in dt.Rows)
                        {
                            customers.Add(
                                new CompanyDetails
                                {
                                    cin = dr["cin"].ToString(),
                                    companyname = dr["companyname"].ToString(),
                                    companystatus = dr["companystatus"].ToString(),
                                    roc = dr["roc"].ToString(),
                                    registrationnumber = dr["registrationnumber"].ToString(),
                                    companycategory = dr["companycategory"].ToString(),
                                    companysubcategory = dr["companysubcategory"].ToString(),
                                    companyclass = dr["companyclass"].ToString(),
                                    dateofincorporation = dr["dateofincorporation"].ToString(),
                                    ageofcompany = dr["ageofcompany"].ToString(),
                                    activity = dr["activity"].ToString(),
                                    numberofmembers = dr["numberofmembers"].ToString()


                                });
                        }
                        return customers;
                    }




                    else
                    {
                        //Company doesn't exist.
                        DateTime now = DateTime.Now;

                        MySqlCommand myCommand = new MySqlCommand("Insert into uninsertedcompanies(companyname,cin,runtime) values(@companyname, @cin, @runtime)", con);
                        myCommand.Parameters.AddWithValue("@companyname", CompanyName);
                        myCommand.Parameters.AddWithValue("@cin", "");
                        myCommand.Parameters.AddWithValue("@runtime", now);
                        myCommand.ExecuteNonQuery();

                        MySqlCommand myCommands = new MySqlCommand("Insert into uninsertedannualdetails(companyname,cin,runtime) values(@companynames, @cins, @runtimes)", con);
                        myCommands.Parameters.AddWithValue("@companynames", CompanyName);
                        myCommands.Parameters.AddWithValue("@cins", "");
                        myCommands.Parameters.AddWithValue("@runtimes", now);
                        myCommands.ExecuteNonQuery();


                        //Execute BAT
                        Scrapper();

                        MCAAnnualScrapper();



                        MySqlCommand cmd = new MySqlCommand("SELECT  DISTINCT c.cin, c.companyname,c.companystatus,c.roc,c.registrationnumber,c.companycategory,c.companysubcategory,c.companyclass,c.dateofincorporation,c.ageofcompany,c.activity,c.numberofmembers from companydetails c  where c.companyname like @companyname", con);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@companyname", "%"+CompanyName+ "%");
                        MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        sd.Fill(dt);
                        foreach (DataRow dr in dt.Rows)
                        {
                            customers.Add(
                                new CompanyDetails
                                {
                                    cin = dr["cin"].ToString(),
                                    companyname = dr["companyname"].ToString(),
                                    companystatus = dr["companystatus"].ToString(),
                                    roc = dr["roc"].ToString(),
                                    registrationnumber = dr["registrationnumber"].ToString(),
                                    companycategory = dr["companycategory"].ToString(),
                                    companysubcategory = dr["companysubcategory"].ToString(),
                                    companyclass = dr["companyclass"].ToString(),
                                    dateofincorporation = dr["dateofincorporation"].ToString(),
                                    ageofcompany = dr["ageofcompany"].ToString(),
                                    activity = dr["activity"].ToString(),
                                    numberofmembers = dr["numberofmembers"].ToString()


                                });
                        }
                        return customers;





                    } //end of else


                }
                return customers;

            }

        }



        // CAPITAL DETAILS


        public List<CompanyDetails> GetShareCapital(string CompanyName, CompanyDetails cd)
        {

            string query = ConfigurationManager.ConnectionStrings["connString"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(query))
            {
                con.Open();

                List<CompanyDetails> customers = new List<CompanyDetails>();
                if (cd.cins == "CIN")
                {
                    MySqlCommand check_User_Name = new MySqlCommand("SELECT COUNT(*) FROM sharecapital WHERE cin = @cin", con);
                    check_User_Name.Parameters.AddWithValue("@cin", CompanyName);
                    var CinExist = Convert.ToInt32(check_User_Name.ExecuteScalar());

                    if (CinExist > 0)
                    {
                        //CIN exist

                        MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT  s.authorizedcapital,s.paidupcapital from sharecapital s  where s.cin = @cin", con);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@cin", CompanyName);
                        MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        sd.Fill(dt);
                        foreach (DataRow dr in dt.Rows)
                        {
                            customers.Add(
                                new CompanyDetails
                                {
                                    authorizedcapital = dr["authorizedcapital"].ToString(),
                                    paidupcapital = dr["paidupcapital"].ToString()


                                });
                        }
                        return customers;
                    }




                    else
                    {
                        //CIn doesn't exist.
                        MySqlCommand myCommand = new MySqlCommand("Insert into uninsertedcompanies(companyname,cin) values(@companyname, @cin)", con);
                        myCommand.Parameters.AddWithValue("@companyname", "");
                        myCommand.Parameters.AddWithValue("@cin", CompanyName);
                        myCommand.ExecuteNonQuery();

                        MySqlCommand cmd = new MySqlCommand("select DISTINCT s.authorizedcapital,s.paidupcapital from sharecapital s  where s.cin = @cin", con);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@cin", CompanyName);
                        MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        sd.Fill(dt);
                        foreach (DataRow dr in dt.Rows)
                        {
                            customers.Add(
                                new CompanyDetails
                                {
                                    authorizedcapital = dr["authorizedcapital"].ToString(),
                                    paidupcapital = dr["paidupcapital"].ToString()


                                });
                        }
                        return customers;





                    }


                }
                else if (cd.companynames == "Company Name") //If Company Name is Given 
                {
                    MySqlCommand check_User_Name = new MySqlCommand("SELECT COUNT(*) FROM sharecapital WHERE companyname like @cin", con);
                    check_User_Name.Parameters.AddWithValue("@cin", "%"+CompanyName+"%");
                    var CompanyExist = Convert.ToInt32(check_User_Name.ExecuteScalar());

                    if (CompanyExist > 0)
                    {
                        //Company exist

                        MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT  s.authorizedcapital,s.paidupcapital from sharecapital s  where s.companyname like @companyname", con);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@companyname", "%"+CompanyName+"%");
                        MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        sd.Fill(dt);
                        foreach (DataRow dr in dt.Rows)
                        {
                            customers.Add(
                                new CompanyDetails
                                {
                                    authorizedcapital = dr["authorizedcapital"].ToString(),
                                    paidupcapital = dr["paidupcapital"].ToString()

                                });
                        }
                        return customers;
                    }




                    else
                    {
                        //Company doesn't exist.
                        MySqlCommand myCommand = new MySqlCommand("Insert into uninsertedcompanies(companyname,cin) values(@companyname, @cin)", con);
                        myCommand.Parameters.AddWithValue("@companyname", CompanyName);
                        myCommand.Parameters.AddWithValue("@cin", "");
                        myCommand.ExecuteNonQuery();

                        //Execute BAT

                        //string str_Path = HttpContext.Current.Server.MapPath("~/BatchFile/zauba.bat");
                        //ProcessStartInfo processInfo = new ProcessStartInfo(str_Path);
                        //processInfo.UseShellExecute = false;
                        //Process batchProcess = new Process();
                        //batchProcess.StartInfo = processInfo;
                        //batchProcess.Start();

                        //Scrapper();

                        MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT  s.authorizedcapital,s.paidupcapital from sharecapital s  where s.companyname like @companyname", con);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@companyname", "%"+CompanyName+"%");
                        MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        sd.Fill(dt);
                        foreach (DataRow dr in dt.Rows)
                        {
                            customers.Add(
                                new CompanyDetails
                                {
                                    authorizedcapital = dr["authorizedcapital"].ToString(),
                                    paidupcapital = dr["paidupcapital"].ToString()

                                });
                        }
                        return customers;





                    } //end of else


                }
                return customers;

            }

        }


        //ANNUAL COMPLIANCE DETAILS
        public List<CompanyDetails> GetComplianceDetails(string CompanyName, CompanyDetails cd)
        {

            string query = ConfigurationManager.ConnectionStrings["connString"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(query))
            {
                con.Open();

                List<CompanyDetails> customers = new List<CompanyDetails>();
                if (cd.cins == "CIN")
                {
                    MySqlCommand check_User_Name = new MySqlCommand("SELECT COUNT(*) FROM annualcompliance WHERE cin = @cin", con);
                    check_User_Name.Parameters.AddWithValue("@cin", CompanyName);
                    var CinExist = Convert.ToInt32(check_User_Name.ExecuteScalar());

                    if (CinExist > 0)
                    {
                        //CIN exist

                        MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT  a.listingstatus,a.dateoflastgeneralmeeting,a.dateoflastestbalancesheet from annualcompliance a  where a.cin = @cin", con);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@cin", CompanyName);
                        MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        sd.Fill(dt);
                        foreach (DataRow dr in dt.Rows)
                        {
                            customers.Add(
                                new CompanyDetails
                                {
                                    listingstatus = dr["listingstatus"].ToString(),
                                    dateoflastgeneralmeeting = dr["dateoflastgeneralmeeting"].ToString(),
                                    dateoflastestbalancesheet = dr["dateoflastestbalancesheet"].ToString()


                                });
                        }
                        return customers;
                    }




                    else
                    {
                        //CIn doesn't exist.
                        MySqlCommand myCommand = new MySqlCommand("Insert into uninsertedcompanies(companyname,cin) values(@companyname, @cin)", con);
                        myCommand.Parameters.AddWithValue("@companyname", "");
                        myCommand.Parameters.AddWithValue("@cin", CompanyName);
                        myCommand.ExecuteNonQuery();


                        MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT a.listingstatus,a.dateoflastgeneralmeeting,a.dateoflastestbalancesheet from annualcompliance a  where a.cin = @cin", con);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@cin", CompanyName);
                        MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        sd.Fill(dt);
                        foreach (DataRow dr in dt.Rows)
                        {
                            customers.Add(
                                new CompanyDetails
                                {
                                    listingstatus = dr["listingstatus"].ToString(),
                                    dateoflastgeneralmeeting = dr["dateoflastgeneralmeeting"].ToString(),
                                    dateoflastestbalancesheet = dr["dateoflastestbalancesheet"].ToString()

                                });
                        }
                        return customers;





                    }


                }
                else if (cd.companynames == "Company Name") //If Company Name is Given 
                {
                    MySqlCommand check_User_Name = new MySqlCommand("SELECT COUNT(*) FROM annualcompliance WHERE companyname like @cin", con);
                    check_User_Name.Parameters.AddWithValue("@cin", "%"+CompanyName+"%");
                    var CompanyExist = Convert.ToInt32(check_User_Name.ExecuteScalar());

                    if (CompanyExist > 0)
                    {
                        //Company exist

                        MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT  a.listingstatus,a.dateoflastgeneralmeeting,a.dateoflastestbalancesheet from annualcompliance a  where a.companyname like @companyname", con);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@companyname", "%"+CompanyName+"%");
                        MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        sd.Fill(dt);
                        foreach (DataRow dr in dt.Rows)
                        {
                            customers.Add(
                                new CompanyDetails
                                {
                                    listingstatus = dr["listingstatus"].ToString(),
                                    dateoflastgeneralmeeting = dr["dateoflastgeneralmeeting"].ToString(),
                                    dateoflastestbalancesheet = dr["dateoflastestbalancesheet"].ToString()
                                });
                        }
                        return customers;
                    }




                    else
                    {
                        //Company doesn't exist.
                        MySqlCommand myCommand = new MySqlCommand("Insert into uninsertedcompanies(companyname,cin) values(@companyname, @cin)", con);
                        myCommand.Parameters.AddWithValue("@companyname", CompanyName);
                        myCommand.Parameters.AddWithValue("@cin", "");
                        myCommand.ExecuteNonQuery();




                        MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT  a.listingstatus,a.dateoflastgeneralmeeting,a.dateoflastestbalancesheet from annualcompliance a  where a.companyname like @companyname", con);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@companyname", "%"+CompanyName+"%");
                        MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        sd.Fill(dt);
                        foreach (DataRow dr in dt.Rows)
                        {
                            customers.Add(
                                new CompanyDetails
                                {
                                    listingstatus = dr["listingstatus"].ToString(),
                                    dateoflastgeneralmeeting = dr["dateoflastgeneralmeeting"].ToString(),
                                    dateoflastestbalancesheet = dr["dateoflastestbalancesheet"].ToString()

                                });
                        }
                        return customers;





                    } //end of else


                }
                return customers;

            }

        }


        //DIRECTOR DETAILS

        public List<CompanyDetails> GetDirectorDetails(string CompanyName, CompanyDetails cd)
        {

            string query = ConfigurationManager.ConnectionStrings["connString"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(query))
            {
                con.Open();

                List<CompanyDetails> customers = new List<CompanyDetails>();
                if (cd.cins == "CIN")
                {
                    MySqlCommand check_User_Name = new MySqlCommand("SELECT COUNT(*) FROM companydetails WHERE cin = @cin", con);
                    check_User_Name.Parameters.AddWithValue("@cin", CompanyName);
                    var CinExist = Convert.ToInt32(check_User_Name.ExecuteScalar());

                    if (CinExist > 0)
                    {
                        //CIN exist

                        MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT d.din,d.directorname,d.designation,d.appointmentdate from directordetails d  where d.cin = @cin", con);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@cin", CompanyName);
                        MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        sd.Fill(dt);
                        foreach (DataRow dr in dt.Rows)
                        {
                            customers.Add(
                                new CompanyDetails
                                {
                                    din = dr["din"].ToString(),
                                    directorname = dr["directorname"].ToString(),
                                    designation = dr["designation"].ToString(),
                                    appointmentdate = dr["appointmentdate"].ToString()


                                });
                        }
                        return customers;
                    }




                    else
                    {
                        //CIn doesn't exist.
                        MySqlCommand myCommand = new MySqlCommand("Insert into uninsertedcompanies(companyname,cin) values(@companyname, @cin)", con);
                        myCommand.Parameters.AddWithValue("@companyname", "");
                        myCommand.Parameters.AddWithValue("@cin", CompanyName);
                        myCommand.ExecuteNonQuery();

                        //EXECUTE BAT
                        //Scrapper();


                        MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT  d.din,d.directorname,d.designation,d.appointmentdate from directordetails d  where d.cin = @cin", con);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@cin", CompanyName);
                        MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        sd.Fill(dt);
                        foreach (DataRow dr in dt.Rows)
                        {
                            customers.Add(
                                new CompanyDetails
                                {
                                    din = dr["din"].ToString(),
                                    directorname = dr["directorname"].ToString(),
                                    designation = dr["designation"].ToString(),
                                    appointmentdate = dr["appointmentdate"].ToString()


                                });
                        }
                        return customers;





                    }


                }
                else if (cd.companynames == "Company Name") //If Company Name is Given 
                {
                    MySqlCommand check_User_Name = new MySqlCommand("SELECT COUNT(*) FROM companydetails WHERE companyname like @cin", con);
                    check_User_Name.Parameters.AddWithValue("@cin", "%"+CompanyName+"%");
                    var CompanyExist = Convert.ToInt32(check_User_Name.ExecuteScalar());

                    if (CompanyExist > 0)
                    {
                        //Company exist

                        MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT  d.din,d.directorname,d.designation,d.appointmentdate from directordetails d  where d.companyname like @companyname", con);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@companyname", "%"+CompanyName+"%");
                        MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        sd.Fill(dt);
                        foreach (DataRow dr in dt.Rows)
                        {
                            customers.Add(
                                new CompanyDetails
                                {
                                    din = dr["din"].ToString(),
                                    directorname = dr["directorname"].ToString(),
                                    designation = dr["designation"].ToString(),
                                    appointmentdate = dr["appointmentdate"].ToString()

                                });
                        }
                        return customers;
                    }




                    else
                    {
                        //Company doesn't exist.
                        MySqlCommand myCommand = new MySqlCommand("Insert into uninsertedcompanies(companyname,cin) values(@companyname, @cin)", con);
                        myCommand.Parameters.AddWithValue("@companyname", CompanyName);
                        myCommand.Parameters.AddWithValue("@cin", "");
                        myCommand.ExecuteNonQuery();




                        MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT  d.din,d.directorname,d.designation,d.appointmentdate from directordetails d  where d.companyname like @companyname", con);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@companyname", "%"+CompanyName+"%");
                        MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        sd.Fill(dt);
                        foreach (DataRow dr in dt.Rows)
                        {
                            customers.Add(
                                new CompanyDetails
                                {
                                    din = dr["din"].ToString(),
                                    directorname = dr["directorname"].ToString(),
                                    designation = dr["designation"].ToString(),
                                    appointmentdate = dr["appointmentdate"].ToString()


                                });
                        }
                        return customers;





                    } //end of else


                }
                return customers;

            }

        }

        //CHARGES DETAILS

        public List<CompanyDetails> GetChargesDetails(string CompanyName, CompanyDetails cd)
        {

            string query = ConfigurationManager.ConnectionStrings["connString"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(query))
            {
                con.Open();

                List<CompanyDetails> customers = new List<CompanyDetails>();
                if (cd.cins == "CIN")
                {
                    MySqlCommand check_User_Name = new MySqlCommand("SELECT COUNT(*) FROM companydetails WHERE cin = @cin", con);
                    check_User_Name.Parameters.AddWithValue("@cin", CompanyName);
                    var CinExist = Convert.ToInt32(check_User_Name.ExecuteScalar());

                    if (CinExist > 0)
                    {
                        //CIN exist

                        MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT c.chargeid,c.creationdate,c.modificationdate,c.closuredate,c.assetsundercharge,c.amount,c.chargeholder from chargesdetails c where c.cin = @cin", con);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@cin", CompanyName);
                        MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        sd.Fill(dt);
                        foreach (DataRow dr in dt.Rows)
                        {
                            customers.Add(
                                new CompanyDetails
                                {
                                    chargeid = dr["chargeid"].ToString(),
                                    creationdate = dr["creationdate"].ToString(),
                                    modificationdate = dr["modificationdate"].ToString(),
                                    closuredate = dr["closuredate"].ToString(),
                                    assetsundercharge = dr["assetsundercharge"].ToString(),
                                    amount = dr["amount"].ToString(),
                                    chargeholder = dr["chargeholder"].ToString()


                                });
                        }
                        return customers;
                    }




                    else
                    {
                        //CIn doesn't exist.
                        MySqlCommand myCommand = new MySqlCommand("Insert into uninsertedcompanies(companyname,cin) values(@companyname, @cin)", con);
                        myCommand.Parameters.AddWithValue("@companyname", "");
                        myCommand.Parameters.AddWithValue("@cin", CompanyName);
                        myCommand.ExecuteNonQuery();

                        //EXECUTE BAT
                        //Scrapper();

                        MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT c.chargeid,c.creationdate,c.modificationdate,c.closuredate,c.assetsundercharge,c.amount,c.chargeholder from chargesdetails c where c.cin = @cin", con);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@cin", CompanyName);
                        MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        sd.Fill(dt);
                        foreach (DataRow dr in dt.Rows)
                        {
                            customers.Add(
                                new CompanyDetails
                                {
                                    chargeid = dr["chargeid"].ToString(),
                                    creationdate = dr["creationdate"].ToString(),
                                    modificationdate = dr["modificationdate"].ToString(),
                                    closuredate = dr["closuredate"].ToString(),
                                    assetsundercharge = dr["assetsundercharge"].ToString(),
                                    amount = dr["amount"].ToString(),
                                    chargeholder = dr["chargeholder"].ToString()



                                });
                        }
                        return customers;





                    }


                }
                else if (cd.companynames == "Company Name") //If Company Name is Given 
                {
                    MySqlCommand check_User_Name = new MySqlCommand("SELECT COUNT(*) FROM companydetails WHERE companyname like @cin", con);
                    check_User_Name.Parameters.AddWithValue("@cin", "%"+CompanyName+"%");
                    var CompanyExist = Convert.ToInt32(check_User_Name.ExecuteScalar());

                    if (CompanyExist > 0)
                    {
                        //Company exist

                        MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT c.chargeid,c.creationdate,c.modificationdate,c.closuredate,c.assetsundercharge,c.amount,c.chargeholder from chargesdetails c where c.companyname like @companyname", con);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@companyname", "%"+CompanyName+"%");
                        MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        sd.Fill(dt);
                        foreach (DataRow dr in dt.Rows)
                        {
                            customers.Add(
                                new CompanyDetails
                                {
                                    chargeid = dr["chargeid"].ToString(),
                                    creationdate = dr["creationdate"].ToString(),
                                    modificationdate = dr["modificationdate"].ToString(),
                                    closuredate = dr["closuredate"].ToString(),
                                    assetsundercharge = dr["assetsundercharge"].ToString(),
                                    amount = dr["amount"].ToString(),
                                    chargeholder = dr["chargeholder"].ToString()

                                });
                        }
                        return customers;
                    }




                    else
                    {
                        //Company doesn't exist.
                        MySqlCommand myCommand = new MySqlCommand("Insert into uninsertedcompanies(companyname,cin) values(@companyname, @cin)", con);
                        myCommand.Parameters.AddWithValue("@companyname", CompanyName);
                        myCommand.Parameters.AddWithValue("@cin", "");
                        myCommand.ExecuteNonQuery();




                        MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT c.chargeid,c.creationdate,c.modificationdate,c.closuredate,c.assetsundercharge,c.amount,c.chargeholder from chargesdetails c where c.companyname like @companyname", con);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@companyname", "%"+CompanyName+"%");
                        MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        sd.Fill(dt);
                        foreach (DataRow dr in dt.Rows)
                        {
                            customers.Add(
                                new CompanyDetails
                                {
                                    chargeid = dr["chargeid"].ToString(),
                                    creationdate = dr["creationdate"].ToString(),
                                    modificationdate = dr["modificationdate"].ToString(),
                                    closuredate = dr["closuredate"].ToString(),
                                    assetsundercharge = dr["assetsundercharge"].ToString(),
                                    amount = dr["amount"].ToString(),
                                    chargeholder = dr["chargeholder"].ToString()



                                });
                        }
                        return customers;





                    } //end of else


                }
                return customers;

            }

        }

        //ESTABLISHMENT DETAILS


        public List<CompanyDetails> GetEstablishmentDetails(string CompanyName, CompanyDetails cd)
        {

            string query = ConfigurationManager.ConnectionStrings["connString"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(query))
            {
                con.Open();

                List<CompanyDetails> customers = new List<CompanyDetails>();
                if (cd.cins == "CIN")
                {
                    MySqlCommand check_User_Name = new MySqlCommand("SELECT COUNT(*) FROM companydetails WHERE cin = @cin", con);
                    check_User_Name.Parameters.AddWithValue("@cin", CompanyName);
                    var CinExist = Convert.ToInt32(check_User_Name.ExecuteScalar());

                    if (CinExist > 0)
                    {
                        //CIN exist

                        MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT  e.establishmentname,e.city, e.pincode,e.address from establishmentdetails e  where e.cin = @cin", con);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@cin", CompanyName);
                        MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        sd.Fill(dt);
                        foreach (DataRow dr in dt.Rows)
                        {
                            customers.Add(
                                new CompanyDetails
                                {
                                    establishmentname = dr["establishmentname"].ToString(),
                                    city = dr["city"].ToString(),
                                    pincode = dr["pincode"].ToString(),
                                    address = dr["address"].ToString()


                                });
                        }
                        return customers;
                    }




                    else
                    {
                        //CIn doesn't exist.
                        MySqlCommand myCommand = new MySqlCommand("Insert into uninsertedcompanies(companyname,cin) values(@companyname, @cin)", con);
                        myCommand.Parameters.AddWithValue("@companyname", "");
                        myCommand.Parameters.AddWithValue("@cin", CompanyName);
                        myCommand.ExecuteNonQuery();

                        //EXECUTE BAT
                        //Scrapper();


                        MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT  e.establishmentname,e.city, e.pincode,e.address from establishmentdetails e  where e.cin = @cin", con);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@cin", CompanyName);
                        MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        sd.Fill(dt);
                        foreach (DataRow dr in dt.Rows)
                        {
                            customers.Add(
                                new CompanyDetails
                                {
                                    establishmentname = dr["establishmentname"].ToString(),
                                    city = dr["city"].ToString(),
                                    pincode = dr["pincode"].ToString(),
                                    address = dr["address"].ToString()


                                });
                        }
                        return customers;





                    }


                }
                else if (cd.companynames == "Company Name") //If Company Name is Given 
                {
                    MySqlCommand check_User_Name = new MySqlCommand("SELECT COUNT(*) FROM companydetails WHERE companyname like @cin", con);
                    check_User_Name.Parameters.AddWithValue("@cin", "%"+CompanyName+"%");
                    var CompanyExist = Convert.ToInt32(check_User_Name.ExecuteScalar());

                    if (CompanyExist > 0)
                    {
                        //Company exist

                        MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT  e.establishmentname,e.city, e.pincode,e.address from establishmentdetails e  where e.companyname like @companyname", con);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@companyname", "%"+CompanyName+"%");
                        MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        sd.Fill(dt);
                        foreach (DataRow dr in dt.Rows)
                        {
                            customers.Add(
                                new CompanyDetails
                                {
                                    establishmentname = dr["establishmentname"].ToString(),
                                    city = dr["city"].ToString(),
                                    pincode = dr["pincode"].ToString(),
                                    address = dr["address"].ToString()


                                });
                        }
                        return customers;
                    }




                    else
                    {
                        //Company doesn't exist.
                        MySqlCommand myCommand = new MySqlCommand("Insert into uninsertedcompanies(companyname,cin) values(@companyname, @cin)", con);
                        myCommand.Parameters.AddWithValue("@companyname", CompanyName);
                        myCommand.Parameters.AddWithValue("@cin", "");
                        myCommand.ExecuteNonQuery();

                        //Execute BAT

                        //string str_Path = HttpContext.Current.Server.MapPath("~/BatchFile/zauba.bat");
                        //ProcessStartInfo processInfo = new ProcessStartInfo(str_Path);
                        //processInfo.UseShellExecute = false;
                        //Process batchProcess = new Process();
                        //batchProcess.StartInfo = processInfo;
                        //batchProcess.Start();

                        //Scrapper();

                        MySqlCommand cmd = new MySqlCommand("SELECT  DISTINCT e.establishmentname,e.city, e.pincode,e.address from establishmentdetails e  where e.companyname like @companyname", con);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@companyname", "%"+CompanyName+"%");
                        MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        sd.Fill(dt);
                        foreach (DataRow dr in dt.Rows)
                        {
                            customers.Add(
                                new CompanyDetails
                                {
                                    establishmentname = dr["establishmentname"].ToString(),
                                    city = dr["city"].ToString(),
                                    pincode = dr["pincode"].ToString(),
                                    address = dr["address"].ToString()


                                });
                        }
                        return customers;





                    } //end of else


                }
                return customers;

            }

        }

        //Based on Date Value given

        public List<CompanyDetails> GetStatistics(string datevalue)
        {
            string query = ConfigurationManager.ConnectionStrings["connString"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(query))
            {
                MySqlCommand cmd = new MySqlCommand("Select DISTINCT c.cin, c.companyname from companydetails c where c.datevalue= @datevalue", con);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@datevalue", datevalue);
                con.Open();
                List<CompanyDetails> customers = new List<CompanyDetails>();
                MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                sd.Fill(dt);
                foreach (DataRow dr in dt.Rows)
                {
                    customers.Add(new CompanyDetails
                    {
                        cin = dr["cin"].ToString(),
                        companyname = dr["companyname"].ToString()

                    });
                }
                return customers;
            }

        }


        //This is the DASHBOARD COMPANY AND CIN COUNT
        public List<string> GetValues()
        {
            List<string> values = new List<string>();
            string query = ConfigurationManager.ConnectionStrings["connString"].ConnectionString;
            //Db com = new Db();
            //string connection = com.CheckForRightConnection();
            using (MySqlConnection con = new MySqlConnection(query))
            {
                List<CompanyDetails> customers = new List<CompanyDetails>();
                //newchange
                MySqlCommand cmd = new MySqlCommand("select (select count(c.cin) from companydetails c) as CountA,(select count(c.companyname) as countB from companydetails c) as CountB", con);
                //select count(a.no) from login a where a.IsActive=1 and a.Roles='user'
                //
                cmd.CommandType = CommandType.Text;
                MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                con.Open();
                sd.Fill(dt);
                values.Add(Convert.ToString(dt.Rows[0].ItemArray[0]));
                values.Add(Convert.ToString(dt.Rows[0].ItemArray[1]));


            }
            return values;

        }

        //THIS IS TO DISPLAY THE TOTAL COMPANY AND CINS THAT EXISTS
        public List<CompanyDetails> GetTotalCompanyCINCount()
        {
            string query = ConfigurationManager.ConnectionStrings["connString"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(query))
            {
                MySqlCommand cmd = new MySqlCommand("Select DISTINCT c.companyid, c.cin, c.companyname from companydetails c", con);
                cmd.CommandType = CommandType.Text;
                List<CompanyDetails> customers = new List<CompanyDetails>();
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                foreach (DataRow dr in dt.Rows)
                {
                    customers.Add(new CompanyDetails
                    {
                        companyid = Convert.ToInt16(dr["companyid"].ToString()),
                        cin = dr["cin"].ToString(),
                        companyname = dr["companyname"].ToString()


                    });
                }
                return customers;
            }

        }


        //GET ANNUAL DETAILS


        public List<CompanyDetails> GetAnnualDetails(string CompanyName, CompanyDetails cd)
        {

            string query = ConfigurationManager.ConnectionStrings["connString"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(query))
            {
                con.Open();

                List<CompanyDetails> customers = new List<CompanyDetails>();
                if (cd.cins == "CIN")
                {
                    MySqlCommand check_User_Name = new MySqlCommand("SELECT COUNT(*) FROM companydetails WHERE cin = @cin", con);
                    check_User_Name.Parameters.AddWithValue("@cin", CompanyName);
                    var CinExist = Convert.ToInt32(check_User_Name.ExecuteScalar());

                    if (CinExist > 0)
                    {
                        //CIN exist

                        MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT a.cin, a.companyname,a.srn,a.eformname,a.eventdate from annualdetails a  where a.cin = @cin", con);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@cin", CompanyName);
                        MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        sd.Fill(dt);
                        foreach (DataRow dr in dt.Rows)
                        {
                            customers.Add(
                                new CompanyDetails
                                {
                                
                                    srn = dr["srn"].ToString(),
                                    eformname = dr["eformname"].ToString(),
                                    eventdate = dr["eventdate"].ToString()
                                   

                                });
                        }
                        return customers;
                    }




                    else
                    {
                        //CIn doesn't exist.
                        DateTime now = DateTime.Now;
                        MySqlCommand myCommand = new MySqlCommand("Insert into uninsertedannualdetails(companyname,cin,runtime) values(@companyname, @cin,@now)", con);
                        myCommand.Parameters.AddWithValue("@companyname", "");
                        myCommand.Parameters.AddWithValue("@cin", CompanyName);
                        myCommand.Parameters.AddWithValue("@now", now);

                        myCommand.ExecuteNonQuery();

                        //Scrapper();
                        //EXECUTE BAT





                        MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT a.cin, a.companyname,a.srn,a.eformname,a.eventdate from annualdetails a  where a.cin = @cin", con);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@cin", CompanyName);
                        MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        sd.Fill(dt);
                        foreach (DataRow dr in dt.Rows)
                        {
                            customers.Add(
                                new CompanyDetails
                                {
                                    cin = dr["cin"].ToString(),
                                    companyname = dr["companyname"].ToString(),
                                    srn = dr["srn"].ToString(),
                                    eformname = dr["eformname"].ToString(),
                                    eventdate = dr["eventdate"].ToString()
                               
                                });
                        }
                        return customers;





                    }


                }
                else if (cd.companynames == "Company Name") //If Company Name is Given 
                {
                    MySqlCommand check_User_Name = new MySqlCommand("SELECT COUNT(*) FROM companydetails WHERE companyname like @cin", con);
                    check_User_Name.Parameters.AddWithValue("@cin", "%" + CompanyName + "%");
                    var CompanyExist = Convert.ToInt32(check_User_Name.ExecuteScalar());

                    if (CompanyExist > 0)
                    {
                        //Company exist

                        MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT a.cin, a.companyname,a.srn,a.eformname,a.eventdate from annualdetails a  where a.companyname like @companyname", con);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@companyname", "%" + CompanyName + "%");
                        MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        sd.Fill(dt);
                        foreach (DataRow dr in dt.Rows)
                        {
                            customers.Add(
                                new CompanyDetails
                                {
                                   
                                    srn = dr["srn"].ToString(),
                                    eformname = dr["eformname"].ToString(),
                                    eventdate = dr["eventdate"].ToString()
                               
                                });
                        }
                        return customers;
                    }




                    else
                    {
                        //Company doesn't exist.
                        DateTime now = DateTime.Now;

                        MySqlCommand myCommand = new MySqlCommand("Insert into uninsertedannualdetails(companyname,cin,runtime) values(@companyname, @cin, @runtime)", con);
                        myCommand.Parameters.AddWithValue("@companyname", CompanyName);
                        myCommand.Parameters.AddWithValue("@cin", "");
                        myCommand.Parameters.AddWithValue("@runtime", now);
                        myCommand.ExecuteNonQuery();

                        //Execute BAT

                        //Scrapper();

                        MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT a.cin, a.companyname,a.srn,a.eformname,a.eventdate from annualdetails a  where a.companyname like @companyname", con);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@companyname", "%" + CompanyName + "%");
                        MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        sd.Fill(dt);
                        foreach (DataRow dr in dt.Rows)
                        {
                            customers.Add(
                                new CompanyDetails
                                {
                                    cin = dr["cin"].ToString(),
                                    companyname = dr["companyname"].ToString(),
                                    srn = dr["srn"].ToString(),
                                    eformname = dr["eformname"].ToString(),
                                    eventdate = dr["eventdate"].ToString()
                                });
                        }
                        return customers;





                    } //end of else


                }
                return customers;

            }

        }


    }
}