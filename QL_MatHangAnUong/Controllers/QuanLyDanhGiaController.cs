using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using QL_MatHangAnUong.Filters;

namespace QL_MatHangAnUong.Controllers
{
    /// <summary>Một dòng trong danh sách đánh giá ở khu quản trị.</summary>
    public class DanhGiaQuanTriDong
    {
        public int MaDG { get; set; }
        public int MaSP { get; set; }
        public string TenSP { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
        public int SoSao { get; set; }
        public string NoiDung { get; set; }
        public DateTime NgayDanh { get; set; }
    }

    /// <summary>
    /// [NGƯỜI BÁN] Xem, lọc và xóa đánh giá sản phẩm của khách (đánh giá xấu / spam).
    /// </summary>
    [KiemTraNguoiBan]
    public class QuanLyDanhGiaController : BaseController
    {
        private const int SoDongMoiTrang = 10;

        // GET: /QuanLyDanhGia?tuKhoa=tra&soSao=1&trang=1
        public ActionResult Index(string tuKhoa, int? soSao, int trang = 1)
        {
            ViewBag.Title = "Đánh giá sản phẩm";

            var q = from d in db.DanhGias
                    join s in db.SanPhams on d.MaSP equals s.MaSP
                    join n in db.NguoiDungs on d.MaND equals n.MaND
                    select new { d, s, n };

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                string tk = tuKhoa.Trim();
                q = q.Where(x => x.s.TenSP.Contains(tk) || x.n.HoTen.Contains(tk));
            }

            if (soSao.HasValue && soSao.Value >= 1 && soSao.Value <= 5)
            {
                int ss = soSao.Value;
                q = q.Where(x => x.d.SoSao == ss);
            }

            var truyVan = q.OrderByDescending(x => x.d.NgayDanh)
                           .ThenByDescending(x => x.d.MaDG)
                           .Select(x => new DanhGiaQuanTriDong
                           {
                               MaDG = x.d.MaDG,
                               MaSP = x.d.MaSP,
                               TenSP = x.s.TenSP,
                               HoTen = x.n.HoTen,
                               Email = x.n.Email,
                               SoSao = x.d.SoSao,
                               NoiDung = x.d.NoiDung,
                               NgayDanh = x.d.NgayDanh
                           });

            // Thống kê toàn bộ (không phụ thuộc bộ lọc): số lượt theo từng mức sao + điểm trung bình chung
            var soTheoSao = new int[6];
            foreach (var g in db.DanhGias.GroupBy(d => d.SoSao).Select(g => new { Sao = g.Key, SoLuong = g.Count() }).ToList())
                if (g.Sao >= 1 && g.Sao <= 5) soTheoSao[g.Sao] = g.SoLuong;

            int tong = soTheoSao.Sum();
            int tongSao = 0;
            for (int i = 1; i <= 5; i++) tongSao += i * soTheoSao[i];

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.SoSao = soSao;
            ViewBag.SoTheoSao = soTheoSao;
            ViewBag.TongSo = tong;
            ViewBag.TrungBinh = tong == 0 ? 0 : Math.Round(tongSao / (double)tong, 1);

            return View(PhanTrang(truyVan, trang, SoDongMoiTrang));
        }

        // POST: /QuanLyDanhGia/Xoa — View có confirm() trước khi gửi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Xoa(int id, string tuKhoa, int? soSao, int trang = 1)
        {
            var dg = db.DanhGias.Find(id);
            if (dg == null)
            {
                ThongBao("Không tìm thấy đánh giá cần xóa.", "danger");
            }
            else
            {
                db.DanhGias.Remove(dg);
                db.SaveChanges();
                ThongBao("Đã xóa đánh giá #" + id + ".", "info");
            }

            return RedirectToAction("Index", new { tuKhoa, soSao, trang });
        }
    }
}
