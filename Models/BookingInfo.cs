using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QLBVXK.Models
{
    public class BookingInfo
    {
        public string MaHanhKhach { get; set; }
        public string MaChuyenXe { get; set; }
        public List<string> ViTriGhe { get; set; }
        public int SoLuongGhe { get; set; }
        public decimal TongTien { get; set; }
        public string HoVaTen { get; set; }
        public string SoDienThoai { get; set; }
        public string Email { get; set; }
    }
}