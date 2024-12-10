using QLBVXK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QLBVXK.Controllers
{
    public class KhachHangController : Controller
    {
        QLBanVeXeKhachEntities1 db = new QLBanVeXeKhachEntities1();

        public ActionResult Index(string MaVeXe = null, DateTime? NgayKhoiHanh = null, string DiemDi = null, string DiemDen = null, string TrangThai = null)
        {
            string UserID = Session["Userid"]?.ToString().Trim();
            if (string.IsNullOrEmpty(UserID))
            {
                ViewBag.Message = "Phiên làm việc đã hết hạn. Vui lòng đăng nhập lại.";
                return View(new List<ChiTietVeView>());
            }

            var chiTietVe = CaNhan(UserID, MaVeXe, NgayKhoiHanh, DiemDi, DiemDen, TrangThai);
            ViewBag.TinhThanhList = db.TINHTHANHs.Select(t => new SelectListItem
            {
                Value = t.MaTinhThanh.ToString(),
                Text = t.TenTinhThanh
            }).ToList();

            ViewBag.MaVeXe = MaVeXe;
            ViewBag.NgayKhoiHanh = NgayKhoiHanh?.ToString("yyyy-MM-dd");
            ViewBag.DiemDi = DiemDi;
            ViewBag.DiemDen = DiemDen;
            ViewBag.TrangThai = TrangThai;

            return View(chiTietVe);
        }

        public List<ChiTietVeView> CaNhan(string UserID, string MaVeXe, DateTime? NgayKhoiHanh, string DiemDi, string DiemDen, string TrangThai)
        {
            List<ChiTietVeView> chiTietVe = new List<ChiTietVeView>();

            // Kiểm tra và xử lý các tham số ngoài UserID
            MaVeXe = string.IsNullOrEmpty(MaVeXe) ? null : MaVeXe;
            DiemDi = string.IsNullOrEmpty(DiemDi) ? null : DiemDi;
            DiemDen = string.IsNullOrEmpty(DiemDen) ? null : DiemDen;
            TrangThai = string.IsNullOrEmpty(TrangThai) ? null : TrangThai;
            NgayKhoiHanh = NgayKhoiHanh.HasValue ? NgayKhoiHanh : null;

            if (string.IsNullOrEmpty(MaVeXe) && string.IsNullOrEmpty(DiemDi) && string.IsNullOrEmpty(DiemDen) && !NgayKhoiHanh.HasValue && string.IsNullOrEmpty(TrangThai))
            {

                chiTietVe = db.fn_HienThiChiTietVe(UserID)
                    .AsEnumerable()
                    .Select(x => new ChiTietVeView
                    {
                        MaVeXe = x.MaVeXe,
                        ViTriGhe = x.ViTriGhe,
                        TenTuyenXe = x.TenTuyenXe,
                        DiemDi = x.DiemDi,
                        DiemDen = x.DiemDen,
                        NgayKhoiHanh = (DateTime)x.NgayKhoiHanh,
                        GioKhoiHanh = (TimeSpan)x.GioKhoiHanh,
                        NgayDenDuKien = (DateTime)x.NgayDenDuKien,
                        GioDenDuKien = (TimeSpan)x.GioDenDuKien,
                        GiaTien = Convert.ToDecimal(x.GiaTien),
                        TrangThai = x.TrangThai
                    }).ToList();
            }
            else
            {
                chiTietVe = db.fn_HienThiChiTietVeLoc(UserID, MaVeXe, NgayKhoiHanh, DiemDi, DiemDen, TrangThai)
                    .AsEnumerable()
                    .Select(x => new ChiTietVeView
                    {
                        MaVeXe = x.MaVeXe,
                        ViTriGhe = x.ViTriGhe,
                        TenTuyenXe = x.TenTuyenXe,
                        DiemDi = x.DiemDi,
                        DiemDen = x.DiemDen,
                        NgayKhoiHanh = (DateTime)x.NgayKhoiHanh,
                        GioKhoiHanh = (TimeSpan)x.GioKhoiHanh,
                        NgayDenDuKien = (DateTime)x.NgayDenDuKien,
                        GioDenDuKien = (TimeSpan)x.GioDenDuKien,
                        GiaTien = Convert.ToDecimal(x.GiaTien),
                        TrangThai = x.TrangThai
                    }).ToList();
            }

            ViewBag.MaVeXe = MaVeXe;
            ViewBag.NgayKhoiHanh = NgayKhoiHanh?.ToString("yyyy-MM-dd");
            ViewBag.DiemDi = DiemDi;
            ViewBag.DiemDen = DiemDen;
            ViewBag.TrangThai = TrangThai;

            return chiTietVe;
        }

        [HttpPost]
        public ActionResult DoiMatKhau(string matKhauCu, string matKhauMoi, string xacNhanMatKhauMoi)
        {
            try
            {
                var userId = (string)Session["Userid"]?.ToString().Trim();
                if (userId == null)
                {
                    TempData["Message"] = "Phiên làm việc đã hết hạn. Vui lòng đăng nhập lại.";
                    return RedirectToAction("Login", "Account");
                }

                // Kiểm tra mật khẩu mới và xác nhận mật khẩu
                if (matKhauMoi != xacNhanMatKhauMoi)
                {
                    TempData["Message"] = "Mật khẩu mới và xác nhận mật khẩu không khớp.";
                    return RedirectToAction("Index");
                }

                using (var db = new QLBanVeXeKhachEntities())
                {
                    var taiKhoan = db.TAIKHOANs.FirstOrDefault(x => x.UserID == userId.ToString());
                    if (taiKhoan == null)
                    {
                        TempData["Message"] = "Tài khoản không tồn tại.";
                        return RedirectToAction("Index");
                    }

                    if (taiKhoan.Pass.Trim() != matKhauCu)
                    {
                        TempData["Message"] = "Mật khẩu cũ không chính xác.";
                        return RedirectToAction("Index");
                    }

                    taiKhoan.Pass = matKhauMoi;
                    db.SaveChanges();

                    TempData["Message"] = "Đổi mật khẩu thành công!";
                    return RedirectToAction("Index", "KhachHang"); 
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Đã xảy ra lỗi: " + ex.Message;
                return View("Index"); 
            }
        }

        [HttpPost]
        public ActionResult UpdateProfile(string TenHanhKhach, string GioiTinh, string Email, string DiaChi)
        {
            try
            {
                // Kiểm tra UserID trong Session
                var userId = (string)Session["Userid"]?.ToString().Trim();
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Message"] = "Phiên làm việc đã hết hạn. Vui lòng đăng nhập lại.";
                    return RedirectToAction("Login", "Account");
                }

                // Lấy thông tin người dùng từ cơ sở dữ liệu
                var userDetails = db.HANHKHACHes.FirstOrDefault(h => h.UserID == userId);

                if (userDetails != null)
                {
                    // Cập nhật thông tin người dùng
                    userDetails.TenHanhKhach = TenHanhKhach;
                    userDetails.GioiTinh = GioiTinh;
                    userDetails.Email = Email;
                    userDetails.DiaChi = DiaChi;

                    // Lưu thay đổi vào cơ sở dữ liệu
                    db.SaveChanges();

                    // Cập nhật thông tin vào Session
                    Session["TenHanhKhach"] = userDetails.TenHanhKhach;
                    Session["GioiTinh"] = userDetails.GioiTinh;
                    Session["Email"] = userDetails.Email;
                    Session["DiaChi"] = userDetails.DiaChi;

                    // Thông báo thành công
                    TempData["Message"] = "Cập nhật thông tin thành công!";
                    
                }
                else
                {
                    TempData["Message"] = "Không tìm thấy thông tin người dùng.";
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi và thông báo
                TempData["Message"] = "Đã xảy ra lỗi: " + ex.Message;
            }
            return RedirectToAction("Index");
        }
    }
}
