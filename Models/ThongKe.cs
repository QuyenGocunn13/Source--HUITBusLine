using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace QLBVXK.Models
{
    public class ThongKe
    {
        public int Thang { get; set; }
        public int Nam { get; set; }
        public string MaChuyenXe { get; set; }
        public int SoLuongVe { get; set; }
        public double TongDoanhThuTheoThang { get; set; } 
        public int Rank { get; set; } 
        public string LoaiThongKe { get; set; } 
    }
}