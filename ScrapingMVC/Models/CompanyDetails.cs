using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace ScrapingMVC.Models
{
    public class CompanyDetails
    {
        public int? companyid { get; set; }

        [DisplayName("CIN")]
        public string cin { get; set; }
        [DisplayName("Company Name")]

        public string companyname { get; set; }

        [DisplayName("Company Status")]

        public string companystatus { get; set; }
        [DisplayName("ROC")]

        public string roc { get; set; }
        [DisplayName("Registration Number")]

        public string registrationnumber { get; set; }
        [DisplayName("Company Category")]

        public string companycategory { get; set; }
        [DisplayName("Company SubCategory")]

        public string companysubcategory { get; set; }
        [DisplayName("Company Class")]


        public string companyclass { get; set; }
        [DisplayName("Date of Incorporation")]


        public string dateofincorporation { get; set; }
        [DisplayName("Age of Company")]


        public string ageofcompany { get; set; }
        [DisplayName("Activity")]


        public string activity { get; set; }
        [DisplayName("Number of Members")]

        public string numberofmembers { get; set; }

        public string cins { get; set; }

        public string companynames { get; set; }

    
        // share capital details
        [DisplayName("Authorized Capital")]
        public string authorizedcapital { get; set; }

        [DisplayName("Paid Up Capital")]
         public string paidupcapital { get; set; }


        //annual details

        [DisplayName("Listing Status")]
         public string listingstatus { get; set; }

         [DisplayName("Date of Last General Meeting")]
         public string dateoflastgeneralmeeting { get; set; }

         [DisplayName("Date of Lastest Balance Sheet")]
         public string dateoflastestbalancesheet { get; set; }
         

        public List<CompanyDetails> customers { get; set; }

        public List<CompanyDetails> customers1 { get; set; }

        public List<CompanyDetails> customers2 { get; set; }

        public List<CompanyDetails> customers3 { get; set; }

        public List<CompanyDetails> customers4 { get; set; }

        public List<CompanyDetails> customers5 { get; set; }

        public List<CompanyDetails> customers6 { get; set; }




        //establishment details

        [DisplayName("Establishment Name")]

        public string establishmentname { get; set; }

        [DisplayName("City")]

        public string city { get; set; }
        [DisplayName("Pincode")]


        public string pincode { get; set; }
        [DisplayName("Address")]


       public string address { get; set; }
       [DisplayName("DIN")]


        //director details

       public string din { get; set; }
       [DisplayName("Director Name")]


       public string directorname { get; set; }
       [DisplayName("Designation")]


       public string designation { get; set; }
       [DisplayName("Appointment Date")]


       public string appointmentdate { get; set; }



        //Charges Details
       [DisplayName("Charge Id")]

       public string chargeid { get; set; }
       [DisplayName("Creation Date")]
       public string creationdate { get; set; }
       [DisplayName("Modification Date")]

       public string modificationdate { get; set; }
       [DisplayName("Closure Date")]

       public string closuredate { get; set; }
       [DisplayName("Assets under Charge")]

       public string assetsundercharge { get; set; }
       [DisplayName("Amount")]
       public string amount { get; set; }
       [DisplayName("Charge Holder")]
       public string chargeholder { get; set; }

        [DisplayName("SRN")]
       public string srn { get; set; }
       [DisplayName("EForm Name")]

        public string eformname { get; set; }
       [DisplayName("Event Date")]


       public string eventdate { get; set; }
       [DisplayName("ChallanCopy")]


       public string challancopy { get; set; }


       public int primarykey { get; set; }

       public string cinvaluefromdb { get; set; }

       public string companynamefromdb { get; set; }
    }


}