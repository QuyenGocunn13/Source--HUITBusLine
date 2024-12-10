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
    public class QLKhachHangController : Controller
    {
        QLBanVeXeKhachEntities db = new QLBanVeXeKhachEntities();

        // GET: AdminHome/QLKhachHang
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult DSKhachHang()
        {
            List<HANHKHACH> ds = db.HANHKHACHes.Select(t => t).ToList<HANHKHACH>();
            return View(ds);
        }

        public ActionResult ThemKH()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ThemKH(HANHKHACH model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Thêm khách hàng
                    db.Database.ExecuteSqlCommand(
                        "EXEC sp_ThemHanhKhach @SoDienThoai, @TenHanhKhach, @GioiTinh, @DiaChi, @Email",
                        new System.Data.SqlClient.SqlParameter("@SoDienThoai", model.SoDienThoai ?? (object)DBNull.Value),
                        new System.Data.SqlClient.SqlParameter("@TenHanhKhach", SqlDbType.NVarChar) { Value = model.TenHanhKhach ?? (object)DBNull.Value },
                        new System.Data.SqlClient.SqlParameter("@GioiTinh", model.GioiTinh ?? (object)DBNull.Value),
                        new System.Data.SqlClient.SqlParameter("@DiaChi", model.DiaChi ?? (object)DBNull.Value),
                        new System.Data.SqlClient.SqlParameter("@Email", model.Email ?? (object)DBNull.Value)
                    );

                    TempData["SuccessMessage"] = "Thêm hành khách thành công!";
                    return RedirectToAction("DSKhachHang");
                }
                catch (Exception ex)
                {
                    // Bắt lỗi do trigger (RAISEERROR từ SQL)
                    if (ex.Message.Contains("Số điện thoại đã tồn tại"))
                    {
                        TempData["ErrorMessages"] = "Số điện thoại đã tồn tại trong hệ thống.";
                    }
                    else if (ex.Message.Contains("Email đã tồn tại"))
                    {
                        TempData["ErrorMessages"] = "Email đã tồn tại trong hệ thống.";
                    }
                    else
                    {
                        TempData["ErrorMessages"] = "Có lỗi xảy ra khi thêm hành khách. Chi tiết: " + ex.Message;
                    }
                }
            }
            return View(model);
        }

        // GET: Sửa thông tin hành khách
        public ActionResult SuaKH(string id)
        {
            id = id?.Trim();

            var hk = db.HANHKHACHes.FirstOrDefault(h => h.MaHanhKhach == id);
            if (hk == null)
            {
                return HttpNotFound();
            }
            return View(hk);
        }

        [HttpPost]
        public ActionResult SuaKH(HANHKHACH h)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra xem số điện thoại có bị trùng lặp không
                    var isPhoneNumberExist = db.HANHKHACHes.Any(x => x.SoDienThoai == h.SoDienThoai && x.MaHanhKhach != h.MaHanhKhach);
                    if (isPhoneNumberExist)
                    {
                        TempData["ErrorMessages"] = "Số điện thoại đã tồn tại trong hệ thống.";
                        return View(h);
                    }

                    // Kiểm tra email có bị trùng lặp không
                    var isEmailExist = db.HANHKHACHes.Any(x => x.Email == h.Email && x.MaHanhKhach != h.MaHanhKhach);
                    if (isEmailExist)
                    {
                        TempData["ErrorMessages"] = "Email đã tồn tại trong hệ thống.";
                        return View(h);
                    }

                    // Cập nhật thông tin hành khách
                    var timhk = db.HANHKHACHes.Find(h.MaHanhKhach);
                    if (timhk != null)
                    {
                        timhk.TenHanhKhach = h.TenHanhKhach;
                        timhk.UserID = h.UserID;
                        timhk.GioiTinh = h.GioiTinh;
                        timhk.DiaChi = h.DiaChi;
                        timhk.SoDienThoai = h.SoDienThoai;
                        timhk.Email = h.Email;

                        // Lưu thay đổi vào cơ sở dữ liệu
                        db.SaveChanges();
                        TempData["SuccessMessage"] = "Cập nhật thông tin hành khách thành công!";
                        return RedirectToAction("DSKhachHang");
                    }
                    else
                    {
                        TempData["ErrorMessages"] = "Hành khách không tồn tại.";
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessages"] = "Có lỗi xảy ra khi cập nhật dữ liệu: " + ex.Message;
                }
            }

            return View(h);
        }

        public ActionResult XoaKH(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["ErrorMessages"] = "ID không hợp lệ.";
                return RedirectToAction("DSKhachHang");
            }

            id = id?.Trim();

            try
            {
                db.Database.ExecuteSqlCommand(
                    "EXEC sp_XoaHanhKhach @MaHanhKhach",
                    new System.Data.SqlClient.SqlParameter("@MaHanhKhach", id)
                );

                TempData["SuccessMessage"] = "Xóa hành khách thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessages"] = "Có lỗi xảy ra khi xóa dữ liệu: " + ex.Message;
            }

            return RedirectToAction("DSKhachHang");
        }
    }
}