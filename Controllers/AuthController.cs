using QLBVXK.Models;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Security;

namespace QLBVXK.Controllers
{
    public class AuthController : Controller
    {
        QLBanVeXeKhachEntities db = new QLBanVeXeKhachEntities();

        // GET: Auth
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Res()
        {
            return View();
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Res(string HoTen, string SoDienThoai, string Email, string Pass)
        {
            // Kiểm tra số điện thoại hợp lệ
            if (!Regex.IsMatch(SoDienThoai, @"^\d{10}$"))
            {
                ViewBag.Message = "Số điện thoại phải có 10 chữ số.";
                return View();
            }

            // Kiểm tra mật khẩu tối thiểu 8 ký tự
            if (string.IsNullOrEmpty(Pass) || Pass.Length < 8)
            {
                ViewBag.Message = "Mật khẩu phải có tối thiểu 8 ký tự.";
                return View();
            }

            // Kiểm tra họ tên
            if (string.IsNullOrEmpty(HoTen))
            {
                ViewBag.Message = "Họ và tên không được để trống.";
                return View();
            }

            // Kiểm tra email hợp lệ
            if (string.IsNullOrEmpty(Email) || !Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ViewBag.Message = "Email không hợp lệ.";
                return View();
            }

            // Kiểm tra nếu số điện thoại đã tồn tại
            var existingUser = db.TAIKHOANs.FirstOrDefault(u => u.UserName == SoDienThoai);
            if (existingUser != null)
            {
                ViewBag.Message = "Số điện thoại này đã được đăng ký.";
                return View();
            }

            // Gọi thủ tục sp_TaoTaiKhoan để tạo tài khoản và hành khách
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@HoTen", HoTen),
                new SqlParameter("@SoDienThoai", SoDienThoai),
                new SqlParameter("@Email", Email),
                new SqlParameter("@Pass", Pass)
            };

            try
            {
                db.Database.ExecuteSqlCommand("EXEC sp_TaoTaiKhoan @HoTen, @SoDienThoai, @Email, @Pass", parameters);

                // Nếu thành công, điều hướng đến trang đăng nhập
                TempData["SuccessMessage"] = "Tạo tài khoản và hành khách thành công!";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Đã xảy ra lỗi khi tạo tài khoản: " + ex.Message;
                return View();
            }
        }

        [HttpPost]
        public ActionResult Login(string UserName, string Pass)
        {
            // Kiểm tra tài khoản trong cơ sở dữ liệu
            var user = db.TAIKHOANs.FirstOrDefault(u => u.UserName == UserName && u.Pass == Pass);

            if (user != null)
            {
                var userDetails = db.HANHKHACHes.FirstOrDefault(h => h.UserID == user.UserID);
                // Thiết lập cookie xác thực người dùng
                FormsAuthentication.SetAuthCookie(user.UserName, false);

                // Lưu thông tin người dùng và vai trò vào session
                Session["UserRole"] = user.UserRole;
                Session["Username"] = user.UserName; // Lưu tên người dùng vào session
                Session["UserID"] = user.UserID;

                if (user.UserRole == "Admin")
                {
                    return RedirectToAction("TrangChu", "AdminHome", new { area = "AdminHome" });
                }
                else if (user.UserRole == "KhachHang")
                {
                    // Lưu thông tin từ bảng HANHKHACH vào session
                    Session["MaHanhKhach"] = userDetails.MaHanhKhach;
                    Session["TenHanhKhach"] = userDetails.TenHanhKhach;
                    Session["GioiTinh"] = userDetails.GioiTinh;
                    Session["DiaChi"] = userDetails.DiaChi;
                    Session["SoDienThoai"] = userDetails.SoDienThoai;
                    Session["Email"] = userDetails.Email;
                    return RedirectToAction("Index", "KhachHang");
                }
            }
            else
            {
                // Hiển thị thông báo lỗi nếu đăng nhập thất bại
                ViewBag.Message = "Tên đăng nhập hoặc mật khẩu không đúng.";
                return View();
            }

            return RedirectToAction("Login");
        }

        // Đăng xuất người dùng
        public ActionResult Logout()
        {
            // Hủy cookie xác thực và session
            FormsAuthentication.SignOut();
            Session.Clear();

            // Điều hướng về trang đăng nhập
            return RedirectToAction("Login");
        }
    }
}
