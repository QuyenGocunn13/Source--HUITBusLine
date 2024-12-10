using QLBVXK.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QLBVXK.Controllers
{
    public class HomeController : Controller
    {
        QLBanVeXeKhachEntities1 db = new QLBanVeXeKhachEntities1();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        public ActionResult LichTrinh(string DiemDi, string DiemDen, DateTime? NgayDi, int page = 1, int pageSize = 5)  // Đổi pageSize = 5
        {
            // Lấy danh sách tỉnh thành
            ViewBag.TinhThanhList = db.TINHTHANHs.Select(t => new SelectListItem
            {
                Value = t.MaTinhThanh.ToString(),
                Text = t.TenTinhThanh
            }).ToList();

            List<ChuyenXeView> dslt;

            // Lấy dữ liệu tùy theo tham số
            if (string.IsNullOrEmpty(DiemDi) || string.IsNullOrEmpty(DiemDen) || !NgayDi.HasValue)
            {
                dslt = db.fn_HienThiTatCaChuyenXe()
                         .Select(x => new ChuyenXeView
                         {
                             MaChuyenXe = x.MaChuyenXe,
                             TenTuyenXe = x.TenTuyenXe,
                             BienSoXe = x.BienSoXe,
                             NgayKhoiHanh = x.NgayKhoiHanh ?? DateTime.MinValue,
                             GioKhoiHanh = (TimeSpan)x.GioKhoiHanh,
                             GioDenDuKien = (TimeSpan)x.GioDenDuKien,
                             DiemDi = x.DiemDi,
                             DiemDen = x.DiemDen,
                             TenLoaiXe = x.TenLoaiXe,
                             SoGheTrong = (int)x.SoGheTrong,
                             GiaTien = (int)x.GiaTien
                         })
                         .ToList();
            }
            else
            {
                dslt = db.fn_HienThiChuyenXe(DiemDi, DiemDen, NgayDi)
                         .Select(x => new ChuyenXeView
                         {
                             MaChuyenXe = x.MaChuyenXe,
                             TenTuyenXe = x.TenTuyenXe,
                             BienSoXe = x.BienSoXe,
                             NgayKhoiHanh = x.NgayKhoiHanh ?? DateTime.MinValue,
                             GioKhoiHanh = (TimeSpan)x.GioKhoiHanh,
                             GioDenDuKien = (TimeSpan)x.GioDenDuKien,
                             DiemDi = x.DiemDi,
                             DiemDen = x.DiemDen,
                             TenLoaiXe = x.TenLoaiXe,
                             SoGheTrong = (int)x.SoGheTrong,
                             GiaTien = (int)x.GiaTien
                         })
                         .ToList();
            }

            // Áp dụng phân trang
            var paginatedList = dslt.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // Tính tổng số trang
            int totalItems = dslt.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // Gửi các dữ liệu cần thiết cho view
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.diemdi = DiemDi;
            ViewBag.diemden = DiemDen;
            ViewBag.ngaydi = NgayDi;

            return View(paginatedList);
        }
    }
}