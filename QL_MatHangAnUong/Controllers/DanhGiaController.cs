using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web.Mvc;
using QL_MatHangAnUong.Filters;
using QL_MatHangAnUong.Models;

namespace QL_MatHangAnUong.Controllers
{
    // ───── ViewModel riêng của chức năng đánh giá (đặt tại đây để không đụng vào thư mục Models) ─────

    /// <summary>Số sao trung bình + tổng số lượt đánh giá (hiện cạnh tên món).</summary>
    public class DanhGiaTomTat
    {
        public int Tong { get; set; }
        public double TrungBinh { get; set; }
    }

    /// <summary>Một dòng đánh giá hiển thị trong danh sách.</summary>
    public class DanhGiaDong
    {
        public string HoTen { get; set; }
        public int SoSao { get; set; }
        public string NoiDung { get; set; }
        public DateTime NgayDanh { get; set; }
        public bool LaCuaToi { get; set; }
    }

    /// <summary>Toàn bộ khung "Đánh giá sản phẩm" ở trang chi tiết.</summary>
    public class DanhGiaKhungViewModel
    {
        public int MaSP { get; set; }
        public int Tong { get; set; }
        public double TrungBinh { get; set; }
        /// <summary>PhanBo[s] = số lượt đánh giá s sao (s = 1..5; phần tử 0 không dùng).</summary>
        public int[] PhanBo { get; set; }
        public List<DanhGiaDong> Dong { get; set; }
        public bool DuocDanhGia { get; set; }
        /// <summary>Lý do chưa được đánh giá (khi DuocDanhGia = false).</summary>
        public string LyDo { get; set; }
    }

    /// <summary>
    /// Đánh giá sản phẩm.
    /// Quy tắc: chỉ khách ĐÃ MUA món đó và có đơn ở trạng thái "Hoàn thành" mới được đánh giá,
    /// mỗi khách chỉ đánh giá mỗi món MỘT lần (kiểm tra ở Controller + ràng buộc UNIQUE trong CSDL).
    /// </summary>
    public class DanhGiaController : BaseController
    {
        private const int SoDanhGiaHienToiDa = 20;
        private const int DoDaiNoiDungToiDa = 500;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Child action (nhúng trong trang chi tiết) không cần nạp lại menu / giỏ hàng — trang cha đã nạp rồi.
            if (filterContext.IsChildAction) return;
            base.OnActionExecuting(filterContext);
        }

        // ───── Child action: dòng sao cạnh tên món ─────
        [ChildActionOnly]
        public ActionResult Sao(int maSP)
        {
            var phanBo = LayPhanBo(maSP);
            return PartialView("_Sao", new DanhGiaTomTat
            {
                Tong = phanBo.Sum(),
                TrungBinh = TinhTrungBinh(phanBo)
            });
        }

        // ───── Child action: khung đánh giá (thống kê + form + danh sách) ─────
        [ChildActionOnly]
        public ActionResult Khung(int maSP)
        {
            var phanBo = LayPhanBo(maSP);
            var vm = new DanhGiaKhungViewModel
            {
                MaSP = maSP,
                PhanBo = phanBo,
                Tong = phanBo.Sum(),
                TrungBinh = TinhTrungBinh(phanBo)
            };

            var nd = NguoiDungHienTai;
            int maND = nd != null ? nd.MaND : 0;

            vm.Dong = (from d in db.DanhGias
                       join n in db.NguoiDungs on d.MaND equals n.MaND
                       where d.MaSP == maSP
                       orderby d.NgayDanh descending
                       select new DanhGiaDong
                       {
                           HoTen = n.HoTen,
                           SoSao = d.SoSao,
                           NoiDung = d.NoiDung,
                           NgayDanh = d.NgayDanh,
                           LaCuaToi = d.MaND == maND
                       })
                      .Take(SoDanhGiaHienToiDa)
                      .ToList();

            if (nd == null)
                vm.LyDo = "Đăng nhập để đánh giá món này.";
            else if (db.DanhGias.Any(d => d.MaSP == maSP && d.MaND == maND))
                vm.LyDo = "Bạn đã đánh giá món này rồi. Cảm ơn bạn!";
            else if (!DaMuaVaNhanHang(maSP, maND))
                vm.LyDo = "Chỉ khách đã mua món này và có đơn ở trạng thái \"Hoàn thành\" mới được đánh giá.";
            else
                vm.DuocDanhGia = true;

            return PartialView("_Khung", vm);
        }

        // ───── POST: gửi đánh giá ─────
        [HttpPost]
        [ValidateAntiForgeryToken]
        [KiemTraDangNhap]
        public ActionResult Gui(int maSP, int soSao, string noiDung)
        {
            string quayVe = Url.Action("ChiTiet", "SanPham", new { id = maSP }) + "#khungDanhGia";

            if (soSao < 1 || soSao > 5)
            {
                ThongBao("Vui lòng chọn số sao từ 1 đến 5.", "warning");
                return Redirect(quayVe);
            }

            noiDung = (noiDung ?? "").Trim();
            if (noiDung.Length > DoDaiNoiDungToiDa)
            {
                ThongBao("Nội dung đánh giá tối đa " + DoDaiNoiDungToiDa + " ký tự.", "warning");
                return Redirect(quayVe);
            }

            if (!db.SanPhams.Any(s => s.MaSP == maSP))
            {
                ThongBao("Món bạn muốn đánh giá không tồn tại.", "danger");
                return RedirectToAction("Index", "SanPham");
            }

            int maND = NguoiDungHienTai.MaND;

            if (db.DanhGias.Any(d => d.MaSP == maSP && d.MaND == maND))
            {
                ThongBao("Bạn đã đánh giá món này rồi.", "warning");
                return Redirect(quayVe);
            }

            if (!DaMuaVaNhanHang(maSP, maND))
            {
                ThongBao("Chỉ khách đã mua món này và có đơn \"Hoàn thành\" mới được đánh giá.", "warning");
                return Redirect(quayVe);
            }

            db.DanhGias.Add(new DanhGia
            {
                MaSP = maSP,
                MaND = maND,
                SoSao = soSao,
                NoiDung = noiDung.Length == 0 ? null : noiDung,
                NgayDanh = DateTime.Now
            });

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                // Gửi 2 lần liên tiếp -> vi phạm UNIQUE (MaND, MaSP) trong CSDL
                ThongBao("Bạn đã đánh giá món này rồi.", "warning");
                return Redirect(quayVe);
            }

            ThongBao("Cảm ơn bạn đã đánh giá!");
            return Redirect(quayVe);
        }

        // ───── Hàm dùng chung ─────

        /// <summary>Khách đã mua món này và có ít nhất 1 đơn chứa món đó ở trạng thái "Hoàn thành".</summary>
        private bool DaMuaVaNhanHang(int maSP, int maND)
        {
            string hoanThanh = DonHang.HoanThanh;
            return db.ChiTietDonHangs.Any(ct => ct.MaSP == maSP
                                             && ct.DonHang.MaND == maND
                                             && ct.DonHang.TrangThai == hoanThanh);
        }

        /// <summary>Đếm số lượt theo từng mức sao bằng 1 truy vấn. Kết quả: mảng 6 phần tử, chỉ số 1..5 là số lượt.</summary>
        private int[] LayPhanBo(int maSP)
        {
            var phanBo = new int[6];
            var theoSao = db.DanhGias
                .Where(d => d.MaSP == maSP)
                .GroupBy(d => d.SoSao)
                .Select(g => new { Sao = g.Key, SoLuong = g.Count() })
                .ToList();

            foreach (var s in theoSao)
                if (s.Sao >= 1 && s.Sao <= 5) phanBo[s.Sao] = s.SoLuong;

            return phanBo;
        }

        private static double TinhTrungBinh(int[] phanBo)
        {
            int tong = phanBo.Sum();
            if (tong == 0) return 0;
            int tongSao = 0;
            for (int i = 1; i <= 5; i++) tongSao += i * phanBo[i];
            return Math.Round(tongSao / (double)tong, 1);
        }
    }
}
