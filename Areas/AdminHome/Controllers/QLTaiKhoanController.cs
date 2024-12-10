using QLBVXK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QLBVXK.Areas.AdminHome.Controllers
{
    public class QLTaiKhoanController : Controller
    {
        QLBanVeXeKhachEntities db = new QLBanVeXeKhachEntities();

        // GET: AdminHome/QLTaiKhoan
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult DSTaiKhoan(int page = 1, int pageSize = 10)
        {
            var allTaiKhoan = db.TAIKHOANs.ToList();

            int totalItems = allTaiKhoan.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var taiKhoanPage = allTaiKhoan.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;

            return View(taiKhoanPage);
        }

        public ActionResult SuaTK(string id)
        {
            id = id?.Trim();

            // Tìm tài khoản theo UserID
            var taiKhoan = db.TAIKHOANs.FirstOrDefault(t => t.UserID == id);

            if (taiKhoan == null)
            {
                TempData["ErrorMessages"] = "Tài khoản không tồn tại.";
                return RedirectToAction("DSTaiKhoan");
            }

            // Lấy danh sách vai trò người dùng
            ViewBag.UserRoles = new List<SelectListItem>
    {
        new SelectListItem { Text = "Admin", Value = "Admin" },
        new SelectListItem { Text = "KhachHang", Value = "KhachHang" }
    };

            return View(taiKhoan);
        }

        [HttpPost]
        public ActionResult SuaTK(TAIKHOAN taiKhoan)
        {
            // Đảm bảo các lựa chọn vai trò luôn được nạp
            ViewBag.UserRoles = new List<SelectListItem>
    {
        new SelectListItem { Text = "Admin", Value = "Admin" },
        new SelectListItem { Text = "KhachHang", Value = "KhachHang" }
    };

            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra số điện thoại
                    if (taiKhoan.UserName.Length != 10 || !taiKhoan.UserName.All(char.IsDigit))
                    {
                        TempData["ErrorMessages"] = "Số điện thoại phải có đúng 10 chữ số.";
                        return View(taiKhoan);
                    }

                    // Kiểm tra trùng số điện thoại
                    var isUserNameExist = db.TAIKHOANs.Any(x => x.UserName == taiKhoan.UserName && x.UserID != taiKhoan.UserID);
                    if (isUserNameExist)
                    {
                        TempData["ErrorMessages"] = "Số điện thoại đã tồn tại trong hệ thống.";
                        return View(taiKhoan);
                    }

                    // Cập nhật mật khẩu nếu có
                    if (string.IsNullOrEmpty(taiKhoan.Pass))
                    {
                        var existingTaiKhoan = db.TAIKHOANs.FirstOrDefault(x => x.UserID == taiKhoan.UserID);
                        if (existingTaiKhoan != null)
                        {
                            taiKhoan.Pass = existingTaiKhoan.Pass;  // Giữ lại mật khẩu cũ
                        }
                    }
                    else if (taiKhoan.Pass.Length < 8)
                    {
                        TempData["ErrorMessages"] = "Mật khẩu phải có ít nhất 8 ký tự.";
                        return View(taiKhoan);
                    }

                    var existingTaiKhoanDb = db.TAIKHOANs.FirstOrDefault(x => x.UserID == taiKhoan.UserID);
                    if (existingTaiKhoanDb != null)
                    {
                        existingTaiKhoanDb.UserName = taiKhoan.UserName;
                        existingTaiKhoanDb.Pass = taiKhoan.Pass;
                        existingTaiKhoanDb.UserRole = taiKhoan.UserRole;

                        db.SaveChanges();
                        TempData["SuccessMessage"] = "Cập nhật tài khoản thành công!";
                        return RedirectToAction("DSTaiKhoan");
                    }
                    else
                    {
                        TempData["ErrorMessages"] = "Tài khoản không tồn tại.";
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessages"] = "Có lỗi xảy ra khi cập nhật dữ liệu: " + ex.Message;
                }
            }

            return View(taiKhoan);
        }

        public ActionResult XoaTK(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["ErrorMessages"] = "ID không hợp lệ.";
                return RedirectToAction("DSTaiKhoan");
            }

            id = id?.Trim();
            try
            {
                db.Database.ExecuteSqlCommand(
                    "EXEC XoaTaiKhoan @UserID",
                    new System.Data.SqlClient.SqlParameter("@UserID", id)
                );

                TempData["SuccessMessage"] = "Xóa tài khoản thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessages"] = "Có lỗi xảy ra khi xóa dữ liệu: " + ex.Message;
            }

            return RedirectToAction("DSTaiKhoan");
        }
    }
}