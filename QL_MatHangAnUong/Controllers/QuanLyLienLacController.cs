using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using QL_MatHangAnUong.Filters;
using QL_MatHangAnUong.Models;

namespace QL_MatHangAnUong.Controllers
{
    /// <summary>
    /// [NGƯỜI BÁN] Xem và xử lý tin nhắn liên hệ của khách hàng.
    /// Bảng LienLacs không có trong EDMX nên dùng SQL có tham số (chống SQL Injection).
    /// </summary>
    [KiemTraNguoiBan]
    public class QuanLyLienLacController : BaseController
    {
        // SqlException.Number = 208: "Invalid object name" → chưa chạy script tạo bảng
        private const int LoiChuaCoBang = 208;
        private const string ThongBaoChuaCoBang =
            "Chưa có bảng LienLacs trong database. Hãy chạy file TaoBang_LienLacs.sql trong SSMS.";

        // GET: /QuanLyLienLac?loc=chuadoc
        public ActionResult Index(string loc = "tatca", int trang = 1)
        {
            ViewBag.Title = "Tin nhắn khách hàng";
            ViewBag.Loc = loc;

            List<LienLac> ds;
            try
            {
                ds = db.Database.SqlQuery<LienLac>(
                    "SELECT MaLienLac, HoTen, SoDienThoai, Email, NoiDung, NgayGui, DaDoc " +
                    "FROM LienLacs ORDER BY DaDoc ASC, NgayGui DESC").ToList();
            }
            catch (SqlException ex)
            {
                if (ex.Number != LoiChuaCoBang) throw;
                ThongBao(ThongBaoChuaCoBang, "danger");
                ds = new List<LienLac>();
            }

            ViewBag.TongSo = ds.Count;
            ViewBag.SoChuaDoc = ds.Count(x => !x.DaDoc);

            if (loc == "chuadoc") ds = ds.Where(x => !x.DaDoc).ToList();
            else if (loc == "dadoc") ds = ds.Where(x => x.DaDoc).ToList();

            return View(PhanTrang(ds, trang, 10));   // 10 tin / trang
        }

        // GET: /QuanLyLienLac/ChiTiet/5  — mở xem thì tự đánh dấu đã đọc
        public ActionResult ChiTiet(int id)
        {
            var tin = TimTheoMa(id);
            if (tin == null) return HttpNotFound("Không tìm thấy tin nhắn.");

            if (!tin.DaDoc)
            {
                CapNhatDaDoc(id, true);
                tin.DaDoc = true;
            }

            ViewBag.Title = "Chi tiết tin nhắn #" + id;
            return View(tin);
        }

        // POST: /QuanLyLienLac/DanhDau  — đổi trạng thái đã đọc / chưa đọc
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DanhDau(int id, bool daDoc, string loc = "tatca")
        {
            if (CapNhatDaDoc(id, daDoc) == 0)
                ThongBao("Không tìm thấy tin nhắn.", "danger");
            else
                ThongBao(daDoc ? "Đã đánh dấu là đã đọc." : "Đã đánh dấu là chưa đọc.");

            return RedirectToAction("Index", new { loc });
        }

        // POST: /QuanLyLienLac/Xoa  — View có confirm() trước khi gửi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Xoa(int id, string loc = "tatca")
        {
            int soDong = db.Database.ExecuteSqlCommand(
                "DELETE FROM LienLacs WHERE MaLienLac = @id",
                new SqlParameter("@id", id));

            if (soDong == 0) ThongBao("Không tìm thấy tin nhắn cần xóa.", "danger");
            else ThongBao("Đã xóa tin nhắn #" + id + ".");

            return RedirectToAction("Index", new { loc });
        }

        // ---------- Hàm dùng chung ----------
        private LienLac TimTheoMa(int id)
        {
            return db.Database.SqlQuery<LienLac>(
                "SELECT MaLienLac, HoTen, SoDienThoai, Email, NoiDung, NgayGui, DaDoc " +
                "FROM LienLacs WHERE MaLienLac = @id",
                new SqlParameter("@id", id)).FirstOrDefault();
        }

        private int CapNhatDaDoc(int id, bool daDoc)
        {
            return db.Database.ExecuteSqlCommand(
                "UPDATE LienLacs SET DaDoc = @daDoc WHERE MaLienLac = @id",
                new SqlParameter("@daDoc", daDoc),
                new SqlParameter("@id", id));
        }
    }
}
