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

//        public void GenerateSnapshot(string filePath)
//        {
//            IWebDriver driver = new ChromeDriver();
//            driver.Manage().Window.Maximize(); driver.Navigate().GoToUrl("https://unifiedportal-epfo.epfindia.gov.in/publicPortal/no-auth/misReport/home/loadEstSearchHome");
//            System.Threading.Thread.Sleep(10000);

//            var remElement = driver.FindElement(By.Id("capImg"));
//            Point location = remElement.Location;

//            var screenshot = (driver as ChromeDriver).GetScreenshot();
//            using (MemoryStream stream = new MemoryStream(screenshot.AsByteArray))
//            {
//                using (Bitmap bitmap = new Bitmap(stream))
//                {
//                    RectangleF part = new RectangleF(location.X, location.Y, remElement.Size.Width, remElement.Size.Height);
//                    using (Bitmap bn = bitmap.Clone(part, bitmap.PixelFormat))
//                    {
//                        bn.Save(filePath + "CaptchImage.png", System.Drawing.Imaging.ImageFormat.Png);
//                    }
//                }
//            }
//           //reading text from images
//using (var engine = new TesseractEngine(“”, “eng”,         EngineMode.Default))
//{

//Page ocrPage = engine.Process(Pix.LoadFromFile(filePath + “CaptchImage.png”), PageSegMode.AutoOnly);
//var captchatext = ocrPage.GetText();
//}
//            driver.Close();
//            driver.Dispose();
//        }

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

                        Scrapper();
                        //EXECUTE BAT



                        //string Locat = ConfigurationManager.AppSettings["EXELOG"];

                        //string filepath = Locat + "\\" + "ConsoleBatchFile.exe";
                        //ProcessStartInfo info = new ProcessStartInfo(filepath);
                        //info.UseShellExecute = true;
                        //info.CreateNoWindow = false;
                        //info.Verb = "runas";
                        //bool isRunning = Process.GetProcessesByName("ConsoleBatchFile.exe")
                        //.FirstOrDefault(p => p.MainModule.FileName.StartsWith(@"D:\ConsoleBatchFile\ConsoleBatchFile\bin\Debug")) != default(Process);

                        //if (isRunning == false)
                        //{
                        //    Process.Start(info);


                        //}








                        //System.Diagnostics.Process.Start(@"D:\ScrapingMVC\ScrapingMVC\bin\ConsoleBatchFile.exe");



                        //Process process = new Process();
                        //string Locat=ConfigurationManager.AppSettings["EXELOG"];

                        //string filepath = Locat + "\\"+"ConsoleBatchFile.exe";
                        //process.StartInfo.FileName = filepath;
                        //process.StartInfo.Verb = "runas";
                        //process.StartInfo.CreateNoWindow = false;
                        //process.StartInfo.UseShellExecute = true;

                        //process.Start();
                        //process.Close();


                        //string Locat=ConfigurationManager.AppSettings["EXELOG"];

                        //string filepath = Locat + "\\"+"ConsoleBatchFile.exe";
                        //System.Diagnostics.Process.Start(filepath);

                        //ProcessStartInfo startinfo = new ProcessStartInfo();
                        //startinfo.FileName = filepath;
                        //startinfo.Verb = "runas";
                        //startinfo.CreateNoWindow = false;
                        //startinfo.UseShellExecute = true;


                        //System.Diagnostics.Process.Start(@"D:\ConsoleBatchFile\ConsoleBatchFile\bin\Debug\ConsoleBatchFile.exe");


                        //ProcessStartInfo startinfo = new ProcessStartInfo();
                        //string Locat=ConfigurationManager.AppSettings["EXELOG"];
                        //startinfo.FileName = Locat + "ConsoleBatchFile.exe";

                        //@"D:\ConsoleBatchFile\ConsoleBatchFile\bin\Debug\ConsoleBatchFile.exe";
                        //Locat + "ConsoleBatchFile.exe";
                        //@"D:\ConsoleBatchFile\ConsoleBatchFile\bin\Debug\ConsoleBatchFile.exe";
                        //startinfo.Verb = "runas";
                        //startinfo.CreateNoWindow = false;
                        //startinfo.UseShellExecute = true;
                        //Process myProcess = Process.Start(startinfo);
                        //myProcess.Start();

                        //Process process;
                        //ProcessStartInfo processInfo = new System.Diagnostics.ProcessStartInfo(@"C:\WINDOWS\system32\cmd.exe", @"/C "); ;

                        //processInfo.UseShellExecute = false;
                        //processInfo.CreateNoWindow = false;
                        //processInfo.RedirectStandardError = true;
                        //processInfo.RedirectStandardOutput = true;
                        //processInfo.RedirectStandardInput = true;
                        //processInfo.WorkingDirectory = ConfigurationManager.AppSettings["~/BatchFile/zauba.bat"];
                        //processInfo.UseShellExecute = false;
                        //process = Process.Start(processInfo);
                        //process.WaitForExit();


                        //string exeFileAndLocation = @"D:\ConsoleBatchFile\ConsoleBatchFile\bin\Debug\ConsoleBatchFile.exe";
                        //string arguments = "sampleArgument";
                        //System.Diagnostics.Process.Start(exeFileAndLocation, arguments);


                        //ProcessStartInfo startinfo = new ProcessStartInfo();
                        //startinfo.FileName = "~/References/ConsoleBatchFile";
                        //startinfo.CreateNoWindow = false;
                        //startinfo.UseShellExecute = true;
                        //Process myProcess = Process.Start(startinfo);
                        //myProcess.Start();


                        //ProcessStartInfo psi = new ProcessStartInfo();
                        //psi.FileName = "ConsoleBatchFile.exe";
                        //psi.WorkingDirectory = HttpContext.Current.Server.MapPath("~/bin/");
                        //Process proc = Process.Start(psi);





                        //System.Diagnostics.Process.Start(@"D:\ConsoleBatchFile\ConsoleBatchFile\bin\Debug\ConsoleBatchFile.exe");


                        //Process p = new Process();
                        //p.StartInfo.FileName = @"C:\zauba";
                        //p.Start();

                        //ExecuteBatFile();



                        //ProcessStartInfo info = new ProcessStartInfo("C:\\zauba.bat");
                        //info.UseShellExecute = false;
                        //info.RedirectStandardInput = true;
                        //info.RedirectStandardError = true;
                        //info.RedirectStandardOutput = true;
                        //info.UserName = dialog.User;   
                        //info.UserName = "xyz";
                        //string pass = "xyz";
                        //System.Security.SecureString secret = new System.Security.SecureString();
                        //foreach (char c in pass)
                        //    secret.AppendChar(c);
                        //info.Password = secret;
                        //using (Process install = Process.Start(info))
                        //{
                        //    string output = install.StandardOutput.ReadToEnd();
                        //    install.WaitForExit();
                        //     Do something with you output data          
                        //    Console.WriteLine(output);
                        //}
                        //LaunchCommandLineApp();

                        //var proc1 = new ProcessStartInfo();
                        //string anyCommand="echo";
                        //proc1.UseShellExecute = true;

                        //proc1.WorkingDirectory = @"C:\Windows\System32";

                        //proc1.FileName = @"C:\Windows\System32\cmd.exe";
                        //proc1.Verb = "runas";
                        //proc1.Arguments = "/c " + anyCommand;
                        ////proc1.WindowStyle = ProcessWindowStyle.Hidden;
                        //Process.Start(proc1);



                        //Process p = new Process();
                        //p.StartInfo.FileName = "cmd.exe";
                        //p.StartInfo.RedirectStandardInput = true;
                        //p.StartInfo.RedirectStandardOutput = true;
                        //p.StartInfo.CreateNoWindow = true;
                        //p.StartInfo.UseShellExecute = false;
                        //p.Start();

                        //p.StandardInput.WriteLine("echo Oscar");
                        //p.StandardInput.Flush();
                        //p.StandardInput.Close();
                        //p.WaitForExit();
                        //Console.WriteLine(p.StandardOutput.ReadToEnd());





                        //String command = @"C:\Users\TRN-0270\zauba.bat";
                        //ExecuteCommand(command);




                        //string bat = @"C:\Users\TRN-0270\zauba.bat";
                        //var psi = new ProcessStartInfo();
                        //psi.CreateNoWindow = false; //This hides the dos-style black window that the command prompt usually shows
                        //psi.FileName = @"cmd.exe";
                        //psi.Verb = "runas"; //This is what actually runs the command as administrator
                        //psi.Arguments = "/C start " + bat;
                        //try
                        //{
                        //    var process = new Process();
                        //    process.StartInfo = psi;
                        //    process.Start();
                        //    process.WaitForExit();
                        //}
                        //catch (Exception)
                        //{
                        //    //If you are here the user clicked decline to grant admin privileges (or he's not administrator)
                        //}



                        //System.Diagnostics.Process.Start("cmd.exe", @"/c C:/Users/TRN-0270/zauba.bat");




                        //string str_Path = HttpContext.Current.Server.MapPath("~/BatchFile/zauba.bat");
                        //ProcessStartInfo processInfo = new ProcessStartInfo(str_Path);
                        //processInfo.UseShellExecute = false;
                        //Process batchProcess = new Process();
                        //batchProcess.StartInfo = processInfo;
                        //batchProcess.Start();


                        //Process p = new Process();

                        //p.StartInfo.UseShellExecute = false;
                        //p.StartInfo.FileName = "D:/ScrapingMVC/ScrapingMVC/BatchFile/zauba.bat";
                        //p.StartInfo.CreateNoWindow = false;
                        //p.Start();

                        //p.WaitForExit();



                        //var processInfo = new ProcessStartInfo("cmd.exe", "/c" + "\"C:\\Users\\TRN-0270\\zauba.bat\"");

                        //processInfo.CreateNoWindow = true;

                        //processInfo.UseShellExecute = false;

                        //processInfo.RedirectStandardError = true;
                        //processInfo.RedirectStandardOutput = true;

                        //var process = Process.Start(processInfo);

                        //process.Start();








                        //                  var p = new Process();  
                        //                    p.StartInfo.UseShellExecute = false;  
                        //                    p.StartInfo.RedirectStandardOutput = true;  
                        //                    string eOut = null;
                        //                    p.StartInfo.RedirectStandardError = true;
                        //                    p.ErrorDataReceived += new DataReceivedEventHandler((sender, e) => 
                        //                                               { eOut += e.Data; });
                        //                    p.StartInfo.FileName = "zauba.bat";  
                        //                    p.Start();  

                        //// To avoid deadlocks, use an asynchronous read operation on at least one of the streams.  
                        //p.BeginErrorReadLine();
                        //string output = p.StandardOutput.ReadToEnd();  
                        //p.WaitForExit();
                        //ExecuteBatFile();




                        //ProcessStartInfo info = new ProcessStartInfo("D:/ScrapyMVC/ScrapyMVC/scrapy/zauba.bat"); //exePath must be full path.
                        //info.CreateNoWindow = true;
                        //info.UseShellExecute = false;
                        //var user = System.Security.Principal.WindowsIdentity.GetCurrent().User;
                        //var userName = user.Translate(typeof(System.Security.Principal.NTAccount));
                        //Process.Start(info);




                        //ExecuteBatchFile("D:/ScrapyMVC/ScrapyMVC/scrapy/zauba.bat",1000,false);


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
                    MySqlCommand check_User_Name = new MySqlCommand("SELECT COUNT(*) FROM companydetails WHERE companyname = @cin", con);
                    check_User_Name.Parameters.AddWithValue("@cin", CompanyName);
                    var CompanyExist = Convert.ToInt32(check_User_Name.ExecuteScalar());

                    if (CompanyExist > 0)
                    {
                        //Company exist

                        MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT c.cin, c.companyname,c.companystatus,c.roc,c.registrationnumber,c.companycategory,c.companysubcategory,c.companyclass,c.dateofincorporation,c.ageofcompany,c.activity,c.numberofmembers from companydetails c  where c.companyname = @companyname", con);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@companyname", CompanyName);
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

                        //Execute BAT

                        Scrapper();

                        MySqlCommand cmd = new MySqlCommand("SELECT  DISTINCT c.cin, c.companyname,c.companystatus,c.roc,c.registrationnumber,c.companycategory,c.companysubcategory,c.companyclass,c.dateofincorporation,c.ageofcompany,c.activity,c.numberofmembers from companydetails c  where c.companyname = @companyname", con);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@companyname", CompanyName);
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

                        //EXECUTE BAT


                        //string str_Path = HttpContext.Current.Server.MapPath("~/BatchFile/zauba.bat");
                        //ProcessStartInfo processInfo = new ProcessStartInfo(str_Path);
                        //processInfo.UseShellExecute = false;
                        //Process batchProcess = new Process();
                        //batchProcess.StartInfo = processInfo;
                        //batchProcess.Start();


                        //Process p = new Process();

                        //p.StartInfo.UseShellExecute = false;
                        //p.StartInfo.FileName = "D:/ScrapingMVC/ScrapingMVC/BatchFile/zauba.bat";
                        //p.StartInfo.CreateNoWindow = false;
                        //p.Start();

                        //p.WaitForExit();




                        //Scrapper();



                        //                  var p = new Process();  
                        //                    p.StartInfo.UseShellExecute = false;  
                        //                    p.StartInfo.RedirectStandardOutput = true;  
                        //                    string eOut = null;
                        //                    p.StartInfo.RedirectStandardError = true;
                        //                    p.ErrorDataReceived += new DataReceivedEventHandler((sender, e) => 
                        //                                               { eOut += e.Data; });
                        //                    p.StartInfo.FileName = "zauba.bat";  
                        //                    p.Start();  

                        //// To avoid deadlocks, use an asynchronous read operation on at least one of the streams.  
                        //p.BeginErrorReadLine();
                        //string output = p.StandardOutput.ReadToEnd();  
                        //p.WaitForExit();
                        //ExecuteBatFile();




                        //ProcessStartInfo info = new ProcessStartInfo("D:/ScrapyMVC/ScrapyMVC/scrapy/zauba.bat"); //exePath must be full path.
                        //info.CreateNoWindow = true;
                        //info.UseShellExecute = false;
                        //var user = System.Security.Principal.WindowsIdentity.GetCurrent().User;
                        //var userName = user.Translate(typeof(System.Security.Principal.NTAccount));
                        //Process.Start(info);




                        //ExecuteBatchFile("D:/ScrapyMVC/ScrapyMVC/scrapy/zauba.bat",1000,false);


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
                    MySqlCommand check_User_Name = new MySqlCommand("SELECT COUNT(*) FROM sharecapital WHERE companyname = @cin", con);
                    check_User_Name.Parameters.AddWithValue("@cin", CompanyName);
                    var CompanyExist = Convert.ToInt32(check_User_Name.ExecuteScalar());

                    if (CompanyExist > 0)
                    {
                        //Company exist

                        MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT  s.authorizedcapital,s.paidupcapital from sharecapital s  where s.companyname = @companyname", con);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@companyname", CompanyName);
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

                        MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT  s.authorizedcapital,s.paidupcapital from sharecapital s  where s.companyname = @companyname", con);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@companyname", CompanyName);
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

                        //EXECUTE BAT

                        //Scrapper();
                        //string str_Path = HttpContext.Current.Server.MapPath("~/BatchFile/zauba.bat");
                        //ProcessStartInfo processInfo = new ProcessStartInfo(str_Path);
                        //processInfo.UseShellExecute = false;
                        //Process batchProcess = new Process();
                        //batchProcess.StartInfo = processInfo;
                        //batchProcess.Start();


                        //Process p = new Process();

                        //p.StartInfo.UseShellExecute = false;
                        //p.StartInfo.FileName = "D:/ScrapingMVC/ScrapingMVC/BatchFile/zauba.bat";
                        //p.StartInfo.CreateNoWindow = false;
                        //p.Start();

                        //p.WaitForExit();








                        //                  var p = new Process();  
                        //                    p.StartInfo.UseShellExecute = false;  
                        //                    p.StartInfo.RedirectStandardOutput = true;  
                        //                    string eOut = null;
                        //                    p.StartInfo.RedirectStandardError = true;
                        //                    p.ErrorDataReceived += new DataReceivedEventHandler((sender, e) => 
                        //                                               { eOut += e.Data; });
                        //                    p.StartInfo.FileName = "zauba.bat";  
                        //                    p.Start();  

                        //// To avoid deadlocks, use an asynchronous read operation on at least one of the streams.  
                        //p.BeginErrorReadLine();
                        //string output = p.StandardOutput.ReadToEnd();  
                        //p.WaitForExit();
                        //ExecuteBatFile();




                        //ProcessStartInfo info = new ProcessStartInfo("D:/ScrapyMVC/ScrapyMVC/scrapy/zauba.bat"); //exePath must be full path.
                        //info.CreateNoWindow = true;
                        //info.UseShellExecute = false;
                        //var user = System.Security.Principal.WindowsIdentity.GetCurrent().User;
                        //var userName = user.Translate(typeof(System.Security.Principal.NTAccount));
                        //Process.Start(info);




                        //ExecuteBatchFile("D:/ScrapyMVC/ScrapyMVC/scrapy/zauba.bat",1000,false);


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
                    MySqlCommand check_User_Name = new MySqlCommand("SELECT COUNT(*) FROM annualcompliance WHERE companyname = @cin", con);
                    check_User_Name.Parameters.AddWithValue("@cin", CompanyName);
                    var CompanyExist = Convert.ToInt32(check_User_Name.ExecuteScalar());

                    if (CompanyExist > 0)
                    {
                        //Company exist

                        MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT  a.listingstatus,a.dateoflastgeneralmeeting,a.dateoflastestbalancesheet from annualcompliance a  where a.companyname = @companyname", con);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@companyname", CompanyName);
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

                        //Scrapper();
                        //Execute BAT

                        //string str_Path = HttpContext.Current.Server.MapPath("~/BatchFile/zauba.bat");
                        //ProcessStartInfo processInfo = new ProcessStartInfo(str_Path);
                        //processInfo.UseShellExecute = false;
                        //Process batchProcess = new Process();
                        //batchProcess.StartInfo = processInfo;
                        //batchProcess.Start();



                        MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT  a.listingstatus,a.dateoflastgeneralmeeting,a.dateoflastestbalancesheet from annualcompliance a  where a.companyname = @companyname", con);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@companyname", CompanyName);
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
                    MySqlCommand check_User_Name = new MySqlCommand("SELECT COUNT(*) FROM companydetails WHERE companyname = @cin", con);
                    check_User_Name.Parameters.AddWithValue("@cin", CompanyName);
                    var CompanyExist = Convert.ToInt32(check_User_Name.ExecuteScalar());

                    if (CompanyExist > 0)
                    {
                        //Company exist

                        MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT  d.din,d.directorname,d.designation,d.appointmentdate from directordetails d  where d.companyname = @companyname", con);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@companyname", CompanyName);
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

                        //Execute BAT
                        //Scrapper();
                        //string str_Path = HttpContext.Current.Server.MapPath("~/BatchFile/zauba.bat");
                        //ProcessStartInfo processInfo = new ProcessStartInfo(str_Path);
                        //processInfo.UseShellExecute = false;
                        //Process batchProcess = new Process();
                        //batchProcess.StartInfo = processInfo;
                        //batchProcess.Start();



                        MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT  d.din,d.directorname,d.designation,d.appointmentdate from directordetails d  where d.companyname = @companyname", con);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@companyname", CompanyName);
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
                    MySqlCommand check_User_Name = new MySqlCommand("SELECT COUNT(*) FROM companydetails WHERE companyname = @cin", con);
                    check_User_Name.Parameters.AddWithValue("@cin", CompanyName);
                    var CompanyExist = Convert.ToInt32(check_User_Name.ExecuteScalar());

                    if (CompanyExist > 0)
                    {
                        //Company exist

                        MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT c.chargeid,c.creationdate,c.modificationdate,c.closuredate,c.assetsundercharge,c.amount,c.chargeholder from chargesdetails c where c.companyname = @companyname", con);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@companyname", CompanyName);
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

                        //Scrapper();
                        //Execute BAT

                        //string str_Path = HttpContext.Current.Server.MapPath("~/BatchFile/zauba.bat");
                        //ProcessStartInfo processInfo = new ProcessStartInfo(str_Path);
                        //processInfo.UseShellExecute = false;
                        //Process batchProcess = new Process();
                        //batchProcess.StartInfo = processInfo;
                        //batchProcess.Start();



                        MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT c.chargeid,c.creationdate,c.modificationdate,c.closuredate,c.assetsundercharge,c.amount,c.chargeholder from chargesdetails c where c.companyname = @companyname", con);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@companyname", CompanyName);
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
                    MySqlCommand check_User_Name = new MySqlCommand("SELECT COUNT(*) FROM companydetails WHERE companyname = @cin", con);
                    check_User_Name.Parameters.AddWithValue("@cin", CompanyName);
                    var CompanyExist = Convert.ToInt32(check_User_Name.ExecuteScalar());

                    if (CompanyExist > 0)
                    {
                        //Company exist

                        MySqlCommand cmd = new MySqlCommand("SELECT DISTINCT  e.establishmentname,e.city, e.pincode,e.address from establishmentdetails e  where e.companyname = @companyname", con);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@companyname", CompanyName);
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

                        MySqlCommand cmd = new MySqlCommand("SELECT  DISTINCT e.establishmentname,e.city, e.pincode,e.address from establishmentdetails e  where e.companyname = @companyname", con);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@companyname", CompanyName);
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
    }
}