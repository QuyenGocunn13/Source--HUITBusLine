using QLBVXK.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QLBVXK.Areas.AdminHome.Controllers
{
    public class QLTaiXeController : Controller
    {
        QLBanVeXeKhachEntities db = new QLBanVeXeKhachEntities();

        // GET: AdminHome/QLTaiXe
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult DSTaiXe(int page = 1, int pageSize = 5)
        {
            var allTaiXe = db.TAIXEs.ToList();

            // Tính tổng số trang
            int totalItems = allTaiXe.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // Lấy danh sách tài xế cho trang hiện tại
            var taiXePage = allTaiXe.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;

            return View(taiXePage);
        }

        public ActionResult ThemTX()
        {
            // Lấy danh sách mã xe từ bảng XE
            var maXeList = db.XEs.Select(x => new { x.MaXe, x.BienSoXe }).ToList();

            // Truyền danh sách mã xe vào ViewBag dưới dạng SelectList
            ViewBag.MaXeList = new SelectList(maXeList, "MaXe", "BienSoXe");

            return View();
        }

        [HttpPost]
        public ActionResult ThemTX(TAIXE model)
        {
            if (ModelState.IsValid)
            {
                var existingCCCD = db.TAIXEs.FirstOrDefault(t => t.CCCD == model.CCCD);
                if (existingCCCD != null)
                {
                    ModelState.AddModelError("CCCD", "CCCD đã tồn tại trong hệ thống.");
                }

                var existingPhone = db.TAIXEs.FirstOrDefault(t => t.SoDienThoai == model.SoDienThoai);
                if (existingPhone != null)
                {
                    ModelState.AddModelError("SoDienThoai", "Số điện thoại đã tồn tại trong hệ thống.");
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        // Gọi stored procedure để thêm tài xế
                        db.Database.ExecuteSqlCommand(
                            "EXEC them_taixe @MaXe, @TenTaiXe, @SoDienThoai, @CCCD, @DiaChi",
                            new System.Data.SqlClient.SqlParameter("@MaXe", model.MaXe ?? (object)DBNull.Value),
                            new System.Data.SqlClient.SqlParameter("@TenTaiXe", SqlDbType.NVarChar) { Value = model.TenTaiXe ?? (object)DBNull.Value },
                            new System.Data.SqlClient.SqlParameter("@SoDienThoai", model.SoDienThoai ?? (object)DBNull.Value),
                            new System.Data.SqlClient.SqlParameter("@CCCD", model.CCCD ?? (object)DBNull.Value),
                            new System.Data.SqlClient.SqlParameter("@DiaChi", model.DiaChi ?? (object)DBNull.Value)
                        );

                        TempData["SuccessMessage"] = "Thêm tài xế thành công!";
                        return RedirectToAction("DSTaiXe");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Lỗi khi thêm tài xế: " + ex.ToString());
                        TempData["ErrorMessages"] = "Có lỗi xảy ra khi thêm tài xế. Chi tiết: " + ex.Message;
                    }
                }
                else
                {
                    TempData["ErrorMessages"] = "Thông tin nhập vào không hợp lệ. Vui lòng kiểm tra lại.";
                }
            }

            var maXeList = db.XEs.Select(x => new { x.MaXe, x.BienSoXe }).ToList();
            ViewBag.MaXeList = new SelectList(maXeList, "MaXe", "BienSoXe");

            return View(model);
        }

        // GET: Sửa thông tin tài xế
        public ActionResult SuaTX(string id)
        {
            id = id?.Trim();

            // Tìm tài xế theo mã tài xế
            var tx = db.TAIXEs.FirstOrDefault(t => t.MaTaiXe == id);
            if (tx == null)
            {
                return HttpNotFound();
            }

            // Lấy danh sách mã xe từ bảng XE và chuyển đổi thành SelectListItem
            var maXeList = db.XEs.Select(x => new SelectListItem
            {
                Value = x.MaXe.ToString(),
                Text = x.BienSoXe
            }).ToList();

            // Truyền danh sách mã xe vào ViewBag dưới dạng SelectList
            ViewBag.MaXeList = new SelectList(maXeList, "Value", "Text", tx.MaXe);

            return View(tx);
        }

        [HttpPost]
        public ActionResult SuaTX(TAIXE t)
        {
            // Trước khi xử lý lỗi, đảm bảo luôn truyền lại danh sách mã xe
            var maXeList = db.XEs.Select(x => new SelectListItem
            {
                Value = x.MaXe.ToString(),
                Text = x.BienSoXe
            }).ToList();
            ViewBag.MaXeList = new SelectList(maXeList, "Value", "Text", t.MaXe);

            if (ModelState.IsValid)
            {
                // Kiểm tra trùng số điện thoại và CCCD
                var existingCCCD = db.TAIXEs.FirstOrDefault(tx => tx.CCCD == t.CCCD);
                if (existingCCCD != null && existingCCCD.MaTaiXe != t.MaTaiXe)
                {
                    ModelState.AddModelError("CCCD", "CCCD đã tồn tại trong hệ thống.");
                }

                var existingPhone = db.TAIXEs.FirstOrDefault(tx => tx.SoDienThoai == t.SoDienThoai);
                if (existingPhone != null && existingPhone.MaTaiXe != t.MaTaiXe)
                {
                    ModelState.AddModelError("SoDienThoai", "Số điện thoại đã tồn tại trong hệ thống.");
                }

                // Nếu không có lỗi, cập nhật thông tin tài xế
                if (ModelState.IsValid)
                {
                    var timtx = db.TAIXEs.Find(t.MaTaiXe);
                    if (timtx != null)
                    {
                        timtx.MaXe = t.MaXe;
                        timtx.TenTaiXe = t.TenTaiXe;
                        timtx.SoDienThoai = t.SoDienThoai;
                        timtx.CCCD = t.CCCD;
                        timtx.DiaChi = t.DiaChi;

                        try
                        {
                            db.SaveChanges();
                            TempData["SuccessMessage"] = "Cập nhật thông tin tài xế thành công!";
                            return RedirectToAction("DSTaiXe");
                        }
                        catch (Exception ex)
                        {
                            TempData["ErrorMessages"] = "Có lỗi xảy ra khi cập nhật dữ liệu: " + ex.Message;
                        }
                    }
                    else
                    {
                        TempData["ErrorMessages"] = "Không tìm thấy tài xế với ID này.";
                    }
                }
            }

            return View(t);
        }

        public ActionResult XoaKH(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["ErrorMessages"] = "ID không hợp lệ.";
                return RedirectToAction("DSTaiXe");
            }

            id = id?.Trim();

            try
            {
                db.Database.ExecuteSqlCommand(
                    "EXEC XoaTaiXeVaPhanCong @MaTaiXe",
                    new System.Data.SqlClient.SqlParameter("@MaTaiXe", id)
                );

                TempData["SuccessMessage"] = "Xóa tài xế thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessages"] = "Có lỗi xảy ra khi xóa dữ liệu: " + ex.Message;
            }

            return RedirectToAction("DSTaiXe");
        }
    }
}