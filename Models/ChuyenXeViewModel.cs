using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QLBVXK.Models
{
    public class ChuyenXeViewModel
    {
        public string MaChuyenXe { get; set; }
        public string MaTuyenXe { get; set; }
        public string MaXe { get; set; }
        public double GiaTien { get; set; }
        public int? SoGheTrong { get; set; }
        public string GioKhoiHanh { get; set; }
        public string NgayKhoiHanh { get; set; }
        public string GioDenDuKien { get; set; }
        public string NgayDenDuKien { get; set; }
    }

}