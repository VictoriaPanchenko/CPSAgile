using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CpsBoostAgile.DAO;
using CpsBoostAgile.DbContext;
using CpsBoostAgile.Enumeration;

namespace CpsBoostAgile.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        
        public ActionResult Contact()
        {
            return View();
        }
    }
}