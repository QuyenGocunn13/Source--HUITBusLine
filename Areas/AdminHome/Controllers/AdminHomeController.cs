using QLBVXK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QLBVXK.Areas.AdminHome.Controllers
{
    public class AdminHomeController : Controller
    {

        // GET: AdminHome/AdminHome
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult TrangChu()
        {
            return View();
        }
    }
}