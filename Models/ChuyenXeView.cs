using System;

namespace QLBVXK.Models
{
    public class ChuyenXeView
    {
        public string MaChuyenXe { get; set; }
        public string TenTuyenXe { get; set; }
        public string BienSoXe { get; set; }
        public TimeSpan GioKhoiHanh { get; set; }
        public TimeSpan GioDenDuKien { get; set; }
        public DateTime NgayKhoiHanh { get; set; }
        public string DiemDi { get; set; }
        public string DiemDen { get; set; }
        public string TenLoaiXe { get; set; }
        public int SoGheTrong { get; set; }
        public int GiaTien { get; set; }
    }
}