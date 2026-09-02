using System;
using System.Collections.Generic;
using System.Linq;

namespace QL_MatHangAnUong.Models
{
    /// <summary>
    /// Một dòng trong giỏ hàng. Cùng một sản phẩm nhưng khác tùy chọn (size, đường, đá, topping)
    /// sẽ được tính là 2 dòng khác nhau, giống các web trà sữa thật.
    /// </summary>
    public class GioHangItem
    {
        public int MaSP { get; set; }
        public string TenSP { get; set; }
        public string HinhAnh { get; set; }

        /// <summary>Giá gốc của sản phẩm (chưa cộng tùy chọn).</summary>
        public decimal GiaGoc { get; set; }

        /// <summary>Đơn giá cuối cùng = GiaGoc + phụ thu size + phụ thu topping.</summary>
        public decimal DonGia { get; set; }

        public int SoLuong { get; set; }

        public string Size { get; set; }
        public string Duong { get; set; }
        public string Da { get; set; }
        public List<string> Toppings { get; set; }

        public GioHangItem()
        {
            Toppings = new List<string>();
            SoLuong = 1;
            Size = "M";
            Duong = "100%";
            Da = "100%";
        }

        public decimal ThanhTien
        {
            get { return DonGia * SoLuong; }
        }

        /// <summary>Chuỗi mô tả tùy chọn để hiển thị và để lưu vào ChiTietDonHang.</summary>
        public string MoTaTuyChon
        {
            get
            {
                var parts = new List<string>
                {
                    "Size " + Size,
                    "Đường " + Duong,
                    "Đá " + Da
                };
                if (Toppings != null && Toppings.Count > 0)
                    parts.Add("Topping: " + string.Join(", ", Toppings));
                return string.Join(" | ", parts);
            }
        }

        /// <summary>Khóa nhận dạng một dòng giỏ hàng (sản phẩm + tùy chọn).</summary>
        public string Khoa
        {
            get
            {
                var tp = (Toppings == null || Toppings.Count == 0)
                    ? "" : string.Join("-", Toppings.OrderBy(x => x));
                return string.Format("{0}_{1}_{2}_{3}_{4}", MaSP, Size, Duong, Da, tp);
            }
        }
    }

    /// <summary>
    /// Giỏ hàng lưu trong Session["GioHang"].
    /// </summary>
    public class GioHang
    {
        public List<GioHangItem> Items { get; set; }

        /// <summary>Mã giảm giá khách đã áp dụng (nếu có).</summary>
        public string MaGiamGia { get; set; }

        public GioHang()
        {
            Items = new List<GioHangItem>();
        }

        public int TongSoLuong
        {
            get { return Items.Sum(i => i.SoLuong); }
        }

        public decimal TamTinh
        {
            get { return Items.Sum(i => i.ThanhTien); }
        }

        public void Them(GioHangItem item)
        {
            var cu = Items.FirstOrDefault(i => i.Khoa == item.Khoa);
            if (cu != null)
                cu.SoLuong += item.SoLuong;
            else
                Items.Add(item);
        }

        public void CapNhatSoLuong(string khoa, int soLuong)
        {
            var item = Items.FirstOrDefault(i => i.Khoa == khoa);
            if (item == null) return;

            if (soLuong <= 0)
                Items.Remove(item);
            else
                item.SoLuong = Math.Min(soLuong, 100);
        }

        public void Xoa(string khoa)
        {
            Items.RemoveAll(i => i.Khoa == khoa);
        }

        public void XoaTatCa()
        {
            Items.Clear();
            MaGiamGia = null;
        }
    }

    /// <summary>
    /// Bảng phụ thu tùy chọn — tách riêng để Controller và View dùng chung, tránh hard-code nhiều nơi.
    /// </summary>
    public static class TuyChonSanPham
    {
        public static readonly string[] CacSize = { "S", "M", "L" };
        public static readonly string[] CacMucDuong = { "0%", "30%", "50%", "70%", "100%" };
        public static readonly string[] CacMucDa = { "0%", "30%", "50%", "70%", "100%" };

        /// <summary>Danh sách topping và giá tương ứng.</summary>
        public static readonly Dictionary<string, decimal> CacTopping = new Dictionary<string, decimal>
        {
            { "Trân châu đen", 8000 },
            { "Trân châu trắng", 8000 },
            { "Thạch phô mai", 10000 },
            { "Kem cheese", 10000 },
            { "Pudding trứng", 10000 },
            { "Thạch dừa", 7000 }
        };

        public static decimal PhuThuSize(string size)
        {
            switch (size)
            {
                case "L": return 12000;
                case "M": return 6000;
                default: return 0;
            }
        }

        public static decimal PhuThuTopping(IEnumerable<string> toppings)
        {
            if (toppings == null) return 0;
            decimal tong = 0;
            foreach (var t in toppings)
            {
                if (t != null && CacTopping.ContainsKey(t)) tong += CacTopping[t];
            }
            return tong;
        }
    }
}
