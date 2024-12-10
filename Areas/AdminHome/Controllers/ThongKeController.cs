using QLBVXK.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QLBVXK.Areas.AdminHome.Controllers
{
    public class ThongKeController : Controller
    {
        QLBanVeXeKhachEntities db = new QLBanVeXeKhachEntities();

        // GET: AdminHome/ThongKE
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ThongKeDoanhThu()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ThongKeDoanhThu(int nam, string loaiThongKe)
        {
            List<ThongKe> thongKeList = new List<ThongKe>();

            if (loaiThongKe == "DoanhThu")
            {
                var dslt = db.sp_ThongKeDoanhThuTheoThang(nam).ToList();

                thongKeList = dslt.Select(x => new ThongKe
                {
                    Thang = (int)x.Thang,
                    TongDoanhThuTheoThang = (double)x.TongDoanhThuTheo_Thang,
                    LoaiThongKe = "DoanhThu"
                }).ToList();
            }
            else if (loaiThongKe == "ChuyenXe")
            {
                var dslt = db.Database.SqlQuery<ThongKe>("EXEC sp_ThongKeTop5ChuyenXeTheoThang").ToList();
                thongKeList = dslt.Select(x => new ThongKe
                {
                    Thang = x.Thang,
                    Nam = x.Nam,
                    MaChuyenXe = x.MaChuyenXe,
                    SoLuongVe = x.SoLuongVe,
                    Rank = x.Rank,
                    LoaiThongKe = "ChuyenXe"
                }).ToList();
            }

            return View(thongKeList);
        }
    }
}