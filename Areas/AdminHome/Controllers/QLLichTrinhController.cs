using QLBVXK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QLBVXK.Areas.AdminHome.Controllers
{
    public class QLLichTrinhController : Controller
    {
        QLBanVeXeKhachEntities db = new QLBanVeXeKhachEntities();

        // GET: AdminHome/QLLichTrinh
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult DSLichTrinh(int page = 1, int pageSize = 5)
        {
            var allLichTrinh = db.CHUYENXEs.ToList(); 

            int totalItems = allLichTrinh.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var lichTrinhPage = allLichTrinh.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;

            return View(lichTrinhPage);
        }

        public ActionResult ThemLT()
        {
            // Lấy danh sách mã tuyến xe từ bảng TUYENXE và kết hợp với tên tỉnh thành
            var maTuyenXeList = db.TUYENXEs
                .Join(db.TINHTHANHs, tx => tx.DiemDi, tt => tt.MaTinhThanh, (tx, tt) => new { tx, tt.TenTinhThanh })
                .Join(db.TINHTHANHs, combined => combined.tx.DiemDen, tt => tt.MaTinhThanh, (combined, tt2) => new
                {
                    MaTuyenXe = combined.tx.MaTuyenXe,
                    TenTuyenXe = combined.tx.TenTuyenXe,
                    DiemDi = combined.TenTinhThanh,  // Tên điểm đi
                    DiemDen = tt2.TenTinhThanh      // Tên điểm đến
                })
                .AsEnumerable() 
                .Select(t => new
                {
                    MaTuyenXe = t.MaTuyenXe,
                    TenTuyenXe = $"{t.MaTuyenXe}: {t.DiemDi} - {t.DiemDen}" 
                })
                .ToList();

            ViewBag.MaTuyenXeList = new SelectList(maTuyenXeList, "MaTuyenXe", "TenTuyenXe");

            // Lấy danh sách mã xe từ bảng XE
            var maXeList = db.XEs.Select(xe => new { xe.MaXe, xe.BienSoXe }).ToList();
            ViewBag.MaXeList = new SelectList(maXeList, "MaXe", "BienSoXe");

            return View();
        }

        [HttpPost]
        public ActionResult ThemLT(CHUYENXE model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra ngày khởi hành không được trong quá khứ
                if (model.NgayKhoiHanh < DateTime.Now.Date)
                {
                    ModelState.AddModelError("NgayKhoiHanh", "Ngày khởi hành không được trong quá khứ.");
                }

                // Kiểm tra logic ngày giờ
                if (model.NgayDenDuKien < model.NgayKhoiHanh ||
                   (model.NgayDenDuKien == model.NgayKhoiHanh && model.GioDenDuKien <= model.GioKhoiHanh))
                {
                    ModelState.AddModelError("NgayDenDuKien", "Ngày giờ đến dự kiến phải sau ngày giờ khởi hành.");
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        // Gọi stored procedure
                        db.Database.ExecuteSqlCommand(
                            "EXEC sp_InsertChuyenXe @MaTuyenXe, @MaXe, @GiaTien, @GioKhoiHanh, @NgayKhoiHanh, @GioDenDuKien, @NgayDenDuKien",
                            new System.Data.SqlClient.SqlParameter("@MaTuyenXe", model.MaTuyenXe),
                            new System.Data.SqlClient.SqlParameter("@MaXe", model.MaXe),
                            new System.Data.SqlClient.SqlParameter("@GiaTien", model.GiaTien),
                            new System.Data.SqlClient.SqlParameter("@GioKhoiHanh", model.GioKhoiHanh),
                            new System.Data.SqlClient.SqlParameter("@NgayKhoiHanh", model.NgayKhoiHanh),
                            new System.Data.SqlClient.SqlParameter("@GioDenDuKien", model.GioDenDuKien),
                            new System.Data.SqlClient.SqlParameter("@NgayDenDuKien", model.NgayDenDuKien)
                        );

                        TempData["SuccessMessage"] = "Thêm lịch trình thành công!";

                        return RedirectToAction("DSLichTrinh");
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                    }
                }
            }

            // Truyền lại danh sách cho View nếu có lỗi
            var maTuyenXeList = db.TUYENXEs.Select(tx => new { tx.MaTuyenXe, tx.TenTuyenXe }).ToList();
            ViewBag.MaTuyenXeList = new SelectList(maTuyenXeList, "MaTuyenXe", "TenTuyenXe");

            var maXeList = db.XEs.Select(xe => new { xe.MaXe, xe.BienSoXe }).ToList();
            ViewBag.MaXeList = new SelectList(maXeList, "MaXe", "BienSoXe");

            return View(model);
        }

        // GET: Sửa thông tin lịch trình
        public ActionResult SuaLT(string id)
        {
            id = id?.Trim();

            var lt = db.CHUYENXEs.FirstOrDefault(c => c.MaChuyenXe == id);
            if (lt == null)
            {
                return HttpNotFound("Không tìm thấy lịch trình với mã này.");
            }

            // Cập nhật lại phần ngày theo định dạng yyyy-MM-dd
            ViewBag.NgayKhoiHanh = lt.NgayKhoiHanh?.ToString("yyyy-MM-dd");
            ViewBag.NgayDenDuKien = lt.NgayDenDuKien?.ToString("yyyy-MM-dd");

            // Lấy danh sách mã tuyến xe và xe như hiện tại
            var maTuyenXeList = db.TUYENXEs.Select(tx => new SelectListItem
            {
                Value = tx.MaTuyenXe.ToString(),
                Text = tx.TenTuyenXe
            }).ToList();

            var maXeList = db.XEs.Select(x => new SelectListItem
            {
                Value = x.MaXe.ToString(),
                Text = x.BienSoXe
            }).ToList();

            ViewBag.MaTuyenXeList = new SelectList(maTuyenXeList, "Value", "Text", lt.MaTuyenXe);
            ViewBag.MaXeList = new SelectList(maXeList, "Value", "Text", lt.MaXe);

            return View(lt);
        }

        // POST: Sửa thông tin lịch trình
        [HttpPost]
        public ActionResult SuaLT(CHUYENXE lt)
        {
            // Truyền lại danh sách mã tuyến và mã xe nếu có lỗi
            var maTuyenXeList = db.TUYENXEs.Select(tx => new SelectListItem
            {
                Value = tx.MaTuyenXe.ToString(),
                Text = tx.TenTuyenXe
            }).ToList();

            var maXeList = db.XEs.Select(x => new SelectListItem
            {
                Value = x.MaXe.ToString(),
                Text = x.BienSoXe
            }).ToList();

            ViewBag.MaTuyenXeList = new SelectList(maTuyenXeList, "Value", "Text", lt.MaTuyenXe);
            ViewBag.MaXeList = new SelectList(maXeList, "Value", "Text", lt.MaXe);

            // Kiểm tra tính hợp lệ của thông tin nhập vào
            if (ModelState.IsValid)
            {
                // Kiểm tra ngày khởi hành không được trong quá khứ
                if (lt.NgayKhoiHanh < DateTime.Now.Date)
                {
                    ModelState.AddModelError("NgayKhoiHanh", "Ngày khởi hành không được trong quá khứ.");
                }

                // Kiểm tra logic ngày giờ
                if (lt.NgayDenDuKien < lt.NgayKhoiHanh ||
                    (lt.NgayDenDuKien == lt.NgayKhoiHanh && lt.GioDenDuKien <= lt.GioKhoiHanh))
                {
                    ModelState.AddModelError("NgayDenDuKien", "Ngày giờ đến dự kiến phải sau ngày giờ khởi hành.");
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        var existingChuyenXe = db.CHUYENXEs.FirstOrDefault(c => c.MaChuyenXe == lt.MaChuyenXe);
                        if (existingChuyenXe != null)
                        {
                            // Cập nhật thông tin lịch trình
                            existingChuyenXe.MaTuyenXe = lt.MaTuyenXe;
                            existingChuyenXe.MaXe = lt.MaXe;
                            existingChuyenXe.GiaTien = lt.GiaTien;
                            existingChuyenXe.SoGheTrong = lt.SoGheTrong;
                            existingChuyenXe.GioKhoiHanh = lt.GioKhoiHanh;
                            existingChuyenXe.NgayKhoiHanh = lt.NgayKhoiHanh;
                            existingChuyenXe.GioDenDuKien = lt.GioDenDuKien;
                            existingChuyenXe.NgayDenDuKien = lt.NgayDenDuKien;

                            // Lưu thay đổi vào cơ sở dữ liệu
                            db.SaveChanges();
                            TempData["SuccessMessage"] = "Cập nhật lịch trình thành công!";
                            return RedirectToAction("DSLichTrinh");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Không tìm thấy lịch trình với mã này.");
                        }
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật dữ liệu: " + ex.Message);
                    }
                }
            }

            return View(lt);
        }

        public ActionResult XoaLT(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["ErrorMessages"] = "ID không hợp lệ.";
                return RedirectToAction("DSLichTrinh");
            }

            id = id?.Trim();

            try
            {
                // Gọi thủ tục xóa chuyến xe
                db.Database.ExecuteSqlCommand(
                    "EXEC sp_XoaChuyenXe @MaChuyenXe",
                    new System.Data.SqlClient.SqlParameter("@MaChuyenXe", id)
                );

                TempData["SuccessMessage"] = "Xóa lịch trình thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessages"] = "Có lỗi xảy ra khi xóa dữ liệu: " + ex.Message;
            }

            return RedirectToAction("DSLichTrinh");  // Chuyển hướng lại danh sách lịch trình
        }
    }
}