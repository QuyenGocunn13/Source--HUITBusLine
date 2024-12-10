using Newtonsoft.Json;
using QLBVXK.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace QLBVXK.Controllers
{
    public class VeController : Controller
    {
        QLBanVeXeKhachEntities1 db = new QLBanVeXeKhachEntities1();
        // GET: Ve
        public ActionResult Index(string MaChuyenXe)
        {
            var userId = Session["Userid"].ToString().Trim();

            var giaTien = db.CHUYENXEs.Select(c => c.GiaTien).FirstOrDefault(); 
                                                                                
            var hanhKhach = db.HANHKHACHes.FirstOrDefault(h => h.UserID == userId);
            if (hanhKhach == null)
            {
                ViewBag.ErrorMessage = "Không tìm thấy thông tin hành khách.";
                return View();
            }

            // Lấy các thông tin cần thiết của hành khách
            ViewBag.MaHanhKhach = hanhKhach.MaHanhKhach;
            ViewBag.TenHanhKhach = hanhKhach.TenHanhKhach;
            ViewBag.SoDienThoai = hanhKhach.SoDienThoai;
            ViewBag.Email = hanhKhach.Email;

            var gheDaDat = db.VEXEs
            .Where(v => v.MaChuyenXe == MaChuyenXe && v.TrangThai == "Đã đặt")  // Lọc theo chuyến xe và trạng thái "Đã đặt"
            .Select(v => v.ViTriGhe)  // Chỉ lấy vị trí ghế
            .ToList();

            ViewBag.GheDaDat = gheDaDat;  // Truyền danh sách ghế đã đặt vào ViewBag
            ViewBag.GheDaDatxg = string.Join(", ", gheDaDat);
            if (MaChuyenXe.ToString() != null)
            {
                ViewBag.MaChuyenXe = MaChuyenXe.ToString();
            }
            // Gán giá tiền vào ViewBag
            var loaiXe = (from cx in db.CHUYENXEs
                          join xe in db.XEs on cx.MaXe equals xe.MaXe
                          join lx in db.LOAIXEs on xe.MaLoaiXe equals lx.MaLoaiXe
                          where cx.MaChuyenXe == MaChuyenXe
                          select lx.LoaiGhe).FirstOrDefault();

            if (loaiXe == null)
            {
                ViewBag.thongbao = "Không tìm thấy loại xe phù hợp!";
                return View(ViewBag.thongbao);
            }

            ViewBag.GiaTien = giaTien;
            ViewBag.loaixe = loaiXe;

            return View();
        }

        public ActionResult DatVe(string maChuyenXe, List<string> viTriGhe, string hoVaTen, string soDienThoai, string email, decimal giaTien = 0)
        {
            Session.Remove("BookingInfo");

            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrEmpty(maChuyenXe) || viTriGhe == null || viTriGhe.Count == 0 || string.IsNullOrEmpty(hoVaTen) || string.IsNullOrEmpty(soDienThoai) || string.IsNullOrEmpty(email))
            {
                ViewBag.thongbao = "Dữ liệu không hợp lệ, vui lòng kiểm tra lại!";
                return View(ViewBag.thongbao);
            }

            maChuyenXe = maChuyenXe?.Trim();

            // Tính tổng số lượng ghế và tổng tiền
            var soLuongGhe = viTriGhe.Count;
            var tongTien = soLuongGhe * giaTien;

            // Tạo một đối tượng BookingInfo để lưu thông tin
            var bookingInfo = new BookingInfo
            {
                MaChuyenXe = maChuyenXe,
                ViTriGhe = viTriGhe,
                SoLuongGhe = soLuongGhe,
                TongTien = tongTien,
                HoVaTen = hoVaTen,
                SoDienThoai = soDienThoai,
                Email = email
            };

            Session["BookingInfo"] = bookingInfo;

            return RedirectToAction("DatVeThanhToan", "Ve");
        }

        public ActionResult DatVeThanhToan()
        {
            var bookingInfo = Session["BookingInfo"] as BookingInfo;
            if (bookingInfo == null)
            {
                ViewBag.thongbao = "Không tìm thấy thông tin đặt vé.";
                return View();
            }

            return View(bookingInfo);
        }

        public ActionResult ThanhToan()
        {
            var bookingInfo = Session["BookingInfo"] as BookingInfo;
            var userId = Session["Userid"].ToString().Trim();

            if (bookingInfo == null)
            {
                ViewBag.thongbao = "Không có thông tin đặt vé để thanh toán.";
                return RedirectToAction("Index", "Ve");
            }

            try
            {
                // Lấy thông tin hành khách
                string userName = Session["Username"] as string;
                var maHanhKhach = db.HANHKHACHes
                                     .Where(hk => hk.UserID == db.TAIKHOANs
                                     .Where(tk => tk.UserName == userName).Select(tk => tk.UserID)
                                     .FirstOrDefault())
                                     .Select(hk => hk.MaHanhKhach)
                                     .FirstOrDefault();

                if (string.IsNullOrEmpty(maHanhKhach))
                {
                    ViewBag.thongbao = "Không tìm thấy mã hành khách.";
                    return RedirectToAction("ThanhToan", "Ve");
                }

                bookingInfo.MaHanhKhach = maHanhKhach.Trim();
                string viTriGheChuoi = string.Join(",", bookingInfo.ViTriGhe);

                // Gọi thủ tục sp_DatVe để tạo vé và lấy mã vé xe
                var danhSachVe = db.Database.SqlQuery<VEXE>(
                    "EXEC sp_DatVe @MaHanhKhach, @MaChuyenXe, @ViTriGhe",
                    new SqlParameter("@MaHanhKhach", bookingInfo.MaHanhKhach),
                    new SqlParameter("@MaChuyenXe", bookingInfo.MaChuyenXe),
                    new SqlParameter("@ViTriGhe", viTriGheChuoi)
                ).ToList();

                if (danhSachVe == null || !danhSachVe.Any())
                {
                    ViewBag.thongbao = "Đặt vé thất bại hoặc không có vé nào được tạo.";
                    return RedirectToAction("ThanhToan", "Ve");
                }

                // Lấy mã vé từ danh sách vé vừa tạo (ví dụ, vé đầu tiên trong danh sách)
                var maVeXe = danhSachVe.FirstOrDefault()?.MaVeXe;
                var danhSachVeHienThi = db.VEXEs
                    .Where(v => v.MaChuyenXe == bookingInfo.MaChuyenXe
                                && bookingInfo.ViTriGhe.Contains(v.ViTriGhe)
                                && v.MaHanhKhach == bookingInfo.MaHanhKhach)
                    .Join(db.CHUYENXEs,
                          v => v.MaChuyenXe,
                          cx => cx.MaChuyenXe,
                          (v, cx) => new VeViewModel
                          {
                              MaVeXe = v.MaVeXe,
                              ViTriGhe = v.ViTriGhe,
                              GiaTien = cx.GiaTien  // Lấy giá tiền từ bảng CHUYENXE
                          })
                    .ToList();

                ViewBag.DanhSachVe = null;
                ViewBag.DanhSachVe = danhSachVeHienThi;
                double tongTien = danhSachVeHienThi.Sum(ve => ve.GiaTien);

                ViewBag.TongTien = tongTien;
                if (string.IsNullOrEmpty(maVeXe))
                {
                    ViewBag.thongbao = "Không tìm thấy mã vé xe.";
                    return RedirectToAction("ThanhToan", "Ve");
                }
                db.sp_CapNhatSoGheTrong();
                // Gọi thủ tục fn_HienThiChiTietVeLoc để lấy thông tin chi tiết vé
                var chiTietVe = db.fn_HienThiChiTietVeLoc(userId, maVeXe, null, null, null, null)
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

                if (chiTietVe == null || !chiTietVe.Any())
                {
                    ViewBag.thongbao = "Không tìm thấy thông tin vé.";
                    return View();
                }

                // Lưu thông tin chuyến đi vào ViewBag
                var firstChiTietVe = chiTietVe.FirstOrDefault();
                if (firstChiTietVe != null)
                {
                    ViewBag.TenTuyenXe = firstChiTietVe.TenTuyenXe;
                    ViewBag.DiemDi = firstChiTietVe.DiemDi;
                    ViewBag.DiemDen = firstChiTietVe.DiemDen;
                    ViewBag.NgayKhoiHanh = firstChiTietVe.NgayKhoiHanh;
                    ViewBag.GioKhoiHanh = firstChiTietVe.GioKhoiHanh;
                    ViewBag.NgayDenDuKien = firstChiTietVe.NgayDenDuKien;
                    ViewBag.GioDenDuKien = firstChiTietVe.GioDenDuKien;
                    ViewBag.GiaTien = firstChiTietVe.GiaTien;
                }

                ViewBag.thongbao = "Đặt vé thành công.";
                return View(chiTietVe);
            }
            catch (Exception ex)
            {
                ViewBag.thongbao = "Đã có lỗi xảy ra: " + ex.Message;
                return View();
            }
        }
    }
}