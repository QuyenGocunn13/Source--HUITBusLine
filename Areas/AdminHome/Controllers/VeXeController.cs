using QLBVXK.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QLBVXK.Areas.AdminHome.Controllers
{
    public class VeXeController : Controller
    {
        QLBanVeXeKhachEntities db = new QLBanVeXeKhachEntities();

        // GET: AdminHome/VeXe
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult DSVeXe(int page = 1, int pageSize = 5)
        {
            var dsvexe = db.VEXEs.ToList();

            int totalItems = dsvexe.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var lichTrinhPage = dsvexe.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;

            return View(lichTrinhPage);
        }
        public ActionResult Details(string id)
        {
            var chiTietVeXe = db.CHITIETVEXEs.Where(ct => ct.MaVeXe == id).ToList();

            if (chiTietVeXe == null || !chiTietVeXe.Any())
            {
                TempData["Error"] = $"Không tìm thấy chi tiết vé với mã: {id}";
                return RedirectToAction("DSVeXe");
            }

            return View(chiTietVeXe);
        }

        public ActionResult HuyVe(string id)
        {
            try
            {
                var veXe = db.VEXEs.SingleOrDefault(v => v.MaVeXe == id);

                if (veXe == null)
                {
                    TempData["Error"] = $"Không tìm thấy vé xe với mã: {id}";
                    return RedirectToAction("DSVeXe");
                }

                veXe.TrangThai = "Hủy";
                db.SaveChanges();

                TempData["Success"] = "Đã hủy vé thành công.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Có lỗi xảy ra: {ex.Message}";
            }

            return RedirectToAction("DSVeXe");
        }
    }
}
