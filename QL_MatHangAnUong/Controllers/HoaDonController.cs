using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using QL_MatHangAnUong.Filters;
using QL_MatHangAnUong.Models;

namespace QL_MatHangAnUong.Controllers
{
    /// <summary>
    /// In hóa đơn đơn hàng. Khách chỉ in được đơn của chính mình; người bán in được mọi đơn.
    /// </summary>
    [KiemTraDangNhap]
    public class HoaDonController : BaseController
    {
        // GET: /HoaDon/In/5            -> xem hóa đơn
        // GET: /HoaDon/In/5?tuDongIn=true -> mở xong tự bật hộp thoại in
        public ActionResult In(int id, bool tuDongIn = false)
        {
            var dh = db.DonHangs
                .Include(d => d.ChiTietDonHangs)
                .FirstOrDefault(d => d.MaDH == id);

            if (dh == null)
                return HttpNotFound("Không tìm thấy đơn hàng.");

            var nd = NguoiDungHienTai;
            if (dh.MaND != nd.MaND && nd.VaiTro != NguoiDung.RoleSeller)
                return new HttpUnauthorizedResult("Bạn không có quyền xem hóa đơn này.");

            ViewBag.TuDongIn = tuDongIn;
            return View(dh);
        }
    }
}
