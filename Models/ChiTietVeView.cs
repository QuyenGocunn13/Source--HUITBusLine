using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QLBVXK.Models
{
    public class ChiTietVeView
    {
        public List<VeViewModel> DanhSachVe { get; set; }
        public string MaVeXe { get; set; }
        public string ViTriGhe { get; set; }
        public string TenTuyenXe { get; set; }
        public string DiemDi { get; set; }
        public string DiemDen { get; set; }
        public DateTime NgayKhoiHanh { get; set; }
        public TimeSpan GioKhoiHanh { get; set; }
        public DateTime NgayDenDuKien { get; set; }
        public TimeSpan GioDenDuKien { get; set; }
        public decimal GiaTien { get; set; }
        public string TrangThai { get; set; }
    }
}