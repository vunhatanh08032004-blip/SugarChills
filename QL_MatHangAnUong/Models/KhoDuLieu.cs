using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace QL_MatHangAnUong.Models
{
    /// <summary>
    /// Lớp truy cập dữ liệu tập trung.
    /// Mỗi phương thức tạo và đóng DbContext riêng — đảm bảo kết nối luôn được giải phóng.
    /// </summary>
    public static class KhoDuLieu
    {
        // ── NguoiDung ──────────────────────────────────────────────────────────

        public static bool EmailDaTonTai(string email)
        {
            using (var db = new QL_MatHangAnUongEntities())
                return db.NguoiDungs.Any(n => n.Email == email);
        }

        public static void ThemNguoiDung(NguoiDung nd)
        {
            nd.NgayDangKy = DateTime.Now;
            using (var db = new QL_MatHangAnUongEntities())
            {
                db.NguoiDungs.Add(nd);
                db.SaveChanges();
            }
        }

        public static NguoiDung KiemTraDangNhap(string email, string matKhau)
        {
            using (var db = new QL_MatHangAnUongEntities())
                return db.NguoiDungs.FirstOrDefault(n => n.Email == email && n.MatKhau == matKhau);
        }

        public static NguoiDung LayNguoiDung(int maND)
        {
            using (var db = new QL_MatHangAnUongEntities())
                return db.NguoiDungs.Find(maND);
        }

        public static void CapNhatThongTinNguoiDung(int maND, string hoTen, string dienThoai, string diaChi)
        {
            using (var db = new QL_MatHangAnUongEntities())
            {
                var nd = db.NguoiDungs.Find(maND);
                if (nd == null) return;
                nd.HoTen     = hoTen;
                nd.DienThoai = dienThoai;
                nd.DiaChi    = diaChi;
                db.SaveChanges();
            }
        }

        public static bool DoiMatKhau(int maND, string matKhauHienTai, string matKhauMoi)
        {
            using (var db = new QL_MatHangAnUongEntities())
            {
                var nd = db.NguoiDungs.FirstOrDefault(
                    n => n.MaND == maND && n.MatKhau == matKhauHienTai);
                if (nd == null) return false;
                nd.MatKhau = matKhauMoi;
                db.SaveChanges();
                return true;
            }
        }

        // ── LoaiSanPham ────────────────────────────────────────────────────────

        public static List<LoaiSanPham> LayLoais()
        {
            using (var db = new QL_MatHangAnUongEntities())
                return db.LoaiSanPhams.OrderBy(l => l.ThuTu).ToList();
        }

        public static List<LoaiSanPham> LayLoaiHienThi()
        {
            using (var db = new QL_MatHangAnUongEntities())
                return db.LoaiSanPhams.Where(l => l.HienThi).OrderBy(l => l.ThuTu).ToList();
        }

        public static LoaiSanPham LayLoai(int maLoai)
        {
            using (var db = new QL_MatHangAnUongEntities())
                return db.LoaiSanPhams.Find(maLoai);
        }

        public static int DemSanPhamTheoLoai(int maLoai)
        {
            using (var db = new QL_MatHangAnUongEntities())
                return db.SanPhams.Count(s => s.MaLoai == maLoai);
        }

        public static void ThemLoai(LoaiSanPham loai)
        {
            using (var db = new QL_MatHangAnUongEntities())
            {
                db.LoaiSanPhams.Add(loai);
                db.SaveChanges();
            }
        }

        public static bool CapNhatLoai(LoaiSanPham loai)
        {
            using (var db = new QL_MatHangAnUongEntities())
            {
                var existing = db.LoaiSanPhams.Find(loai.MaLoai);
                if (existing == null) return false;
                existing.TenLoai = loai.TenLoai;
                existing.MoTa    = loai.MoTa;
                existing.HinhAnh = loai.HinhAnh;
                existing.HienThi = loai.HienThi;
                existing.ThuTu   = loai.ThuTu;
                db.SaveChanges();
                return true;
            }
        }

        public static bool XoaLoai(int id, out string loi)
        {
            using (var db = new QL_MatHangAnUongEntities())
            {
                var loai = db.LoaiSanPhams.Find(id);
                if (loai == null) { loi = "Không tìm thấy loại."; return false; }
                if (db.SanPhams.Any(s => s.MaLoai == id))
                {
                    loi = string.Format("Không thể xóa loại \"{0}\" vì còn sản phẩm thuộc loại này.", loai.TenLoai);
                    return false;
                }
                db.LoaiSanPhams.Remove(loai);
                db.SaveChanges();
                loi = null;
                return true;
            }
        }

        // ── SanPham ────────────────────────────────────────────────────────────

        public static List<SanPham> LaySanPhams()
        {
            using (var db = new QL_MatHangAnUongEntities())
                return db.SanPhams.Include("LoaiSanPham")
                         .OrderByDescending(s => s.NgayTao).ToList();
        }

        public static SanPham LaySanPham(int maSP)
        {
            using (var db = new QL_MatHangAnUongEntities())
                return db.SanPhams.Include("LoaiSanPham")
                         .FirstOrDefault(s => s.MaSP == maSP);
        }

        /// <summary>Tìm kiếm + lọc sản phẩm đang bán, trả về danh sách đã sắp xếp.</summary>
        public static List<SanPham> LocSanPham(string tuKhoa, int? maLoai,
                                                decimal? giaTu, decimal? giaDen, string sapXep)
        {
            using (var db = new QL_MatHangAnUongEntities())
            {
                // Lọc phía DB trước (chuỗi so sánh)
                var q = db.SanPhams.Where(s => s.DangBan).AsQueryable();

                if (!string.IsNullOrWhiteSpace(tuKhoa))
                {
                    string tk = tuKhoa.Trim().ToLower();
                    q = q.Where(s => s.TenSP.ToLower().Contains(tk)
                                  || (s.MoTa != null && s.MoTa.ToLower().Contains(tk)));
                }
                if (maLoai.HasValue && maLoai.Value > 0)
                    q = q.Where(s => s.MaLoai == maLoai.Value);

                // Load về memory để dùng computed property GiaBanThucTe
                var list = q.ToList();

                if (giaTu.HasValue)
                    list = list.Where(s => s.GiaBanThucTe >= giaTu.Value).ToList();
                if (giaDen.HasValue)
                    list = list.Where(s => s.GiaBanThucTe <= giaDen.Value).ToList();

                switch (sapXep)
                {
                    case "gia-tang": return list.OrderBy(s => s.GiaBanThucTe).ToList();
                    case "gia-giam": return list.OrderByDescending(s => s.GiaBanThucTe).ToList();
                    case "ban-chay": return list.OrderByDescending(s => s.LuotBan).ToList();
                    case "giam-gia": return list
                        .Where(s => s.GiaKhuyenMai.HasValue && s.GiaKhuyenMai.Value > 0)
                        .OrderByDescending(s => s.Gia - s.GiaBanThucTe).ToList();
                    case "moi":     return list.OrderByDescending(s => s.NgayTao).ToList();
                    default:        return list.OrderByDescending(s => s.NoiBat)
                                               .ThenByDescending(s => s.LuotBan).ToList();
                }
            }
        }

        public static List<SanPham> LaySanPhamLienQuan(int maSP, int maLoai, int soLuong)
        {
            using (var db = new QL_MatHangAnUongEntities())
                return db.SanPhams
                    .Where(s => s.MaSP != maSP && s.MaLoai == maLoai && s.DangBan)
                    .OrderByDescending(s => s.LuotBan)
                    .Take(soLuong)
                    .ToList();
        }

        public static void ThemSanPham(SanPham sp)
        {
            sp.NgayTao = DateTime.Now;
            using (var db = new QL_MatHangAnUongEntities())
            {
                db.SanPhams.Add(sp);
                db.SaveChanges();
            }
        }

        public static bool CapNhatSanPham(SanPham sp)
        {
            using (var db = new QL_MatHangAnUongEntities())
            {
                var existing = db.SanPhams.Find(sp.MaSP);
                if (existing == null) return false;
                existing.TenSP       = sp.TenSP;
                existing.MaLoai      = sp.MaLoai;
                existing.Gia         = sp.Gia;
                existing.GiaKhuyenMai= sp.GiaKhuyenMai;
                existing.HinhAnh     = sp.HinhAnh;
                existing.MoTa        = sp.MoTa;
                existing.SoLuongTon  = sp.SoLuongTon;
                existing.DangBan     = sp.DangBan;
                existing.NoiBat      = sp.NoiBat;
                db.SaveChanges();
                return true;
            }
        }

        public static bool XoaSanPham(int id, out string loi)
        {
            using (var db = new QL_MatHangAnUongEntities())
            {
                var sp = db.SanPhams.Find(id);
                if (sp == null) { loi = "Không tìm thấy sản phẩm."; return false; }
                if (db.ChiTietDonHangs.Any(ct => ct.MaSP == id))
                {
                    loi = string.Format(
                        "Không thể xóa \"{0}\" vì đã có trong đơn hàng. Hãy tắt trạng thái bán thay vì xóa.",
                        sp.TenSP);
                    return false;
                }
                db.SanPhams.Remove(sp);
                db.SaveChanges();
                loi = null;
                return true;
            }
        }

        public static bool DoiTrangThaiNoiBat(int id)
        {
            using (var db = new QL_MatHangAnUongEntities())
            {
                var sp = db.SanPhams.Find(id);
                if (sp == null) return false;
                sp.NoiBat = !sp.NoiBat;
                db.SaveChanges();
                return true;
            }
        }

        // ── DonHang ────────────────────────────────────────────────────────────

        public static List<DonHang> LayDonHangs()
        {
            using (var db = new QL_MatHangAnUongEntities())
                return db.DonHangs.Include("NguoiDung")
                         .OrderByDescending(d => d.NgayDat).ToList();
        }

        public static DonHang LayDonHang(int maDH)
        {
            using (var db = new QL_MatHangAnUongEntities())
                return db.DonHangs
                    .Include("NguoiDung")
                    .Include("ChiTietDonHangs")
                    .FirstOrDefault(d => d.MaDH == maDH);
        }

        public static List<DonHang> LayDonHangCuaKhach(int maND)
        {
            using (var db = new QL_MatHangAnUongEntities())
                return db.DonHangs
                    .Where(d => d.MaND == maND)
                    .OrderByDescending(d => d.NgayDat)
                    .ToList();
        }

        public static void ThemDonHang(DonHang dh)
        {
            using (var db = new QL_MatHangAnUongEntities())
            {
                db.DonHangs.Add(dh);
                db.SaveChanges();
            }
        }

        public static void CapNhatTrangThaiDonHang(int maDH, string trangThai)
        {
            using (var db = new QL_MatHangAnUongEntities())
            {
                var dh = db.DonHangs.Find(maDH);
                if (dh == null) return;
                dh.TrangThai = trangThai;
                db.SaveChanges();
            }
        }

        // ── KhuyenMai ──────────────────────────────────────────────────────────

        public static List<KhuyenMai> LayKhuyenMais()
        {
            using (var db = new QL_MatHangAnUongEntities())
                return db.KhuyenMais.OrderByDescending(k => k.NgayBatDau).ToList();
        }

        public static KhuyenMai LayKhuyenMai(int maKM)
        {
            using (var db = new QL_MatHangAnUongEntities())
                return db.KhuyenMais.Find(maKM);
        }

        public static List<KhuyenMai> LayKhuyenMaiConHieuLuc()
        {
            using (var db = new QL_MatHangAnUongEntities())
                return db.KhuyenMais
                    .Where(k => k.KichHoat
                             && k.NgayBatDau <= DateTime.Now
                             && k.NgayKetThuc >= DateTime.Now)
                    .ToList();
        }

        public static KhuyenMai TimTheoMaGiamGia(string maGiamGia)
        {
            if (string.IsNullOrWhiteSpace(maGiamGia)) return null;
            string ma = maGiamGia.Trim().ToUpper();
            using (var db = new QL_MatHangAnUongEntities())
                return db.KhuyenMais.FirstOrDefault(k => k.MaGiamGia == ma);
        }

        public static void ThemKhuyenMai(KhuyenMai km)
        {
            using (var db = new QL_MatHangAnUongEntities())
            {
                db.KhuyenMais.Add(km);
                db.SaveChanges();
            }
        }

        public static bool CapNhatKhuyenMai(KhuyenMai km)
        {
            using (var db = new QL_MatHangAnUongEntities())
            {
                var existing = db.KhuyenMais.Find(km.MaKM);
                if (existing == null) return false;
                existing.TenKM       = km.TenKM;
                existing.MaGiamGia   = km.MaGiamGia;
                existing.PhanTramGiam= km.PhanTramGiam;
                existing.GiamToiDa   = km.GiamToiDa;
                existing.DonToiThieu = km.DonToiThieu;
                existing.NgayBatDau  = km.NgayBatDau;
                existing.NgayKetThuc = km.NgayKetThuc;
                existing.MoTa        = km.MoTa;
                existing.KichHoat    = km.KichHoat;
                existing.LoaiGiam    = km.LoaiGiam;
                db.SaveChanges();
                return true;
            }
        }

        public static bool XoaKhuyenMai(int id)
        {
            using (var db = new QL_MatHangAnUongEntities())
            {
                var km = db.KhuyenMais.Find(id);
                if (km == null) return false;
                db.KhuyenMais.Remove(km);
                db.SaveChanges();
                return true;
            }
        }

        public static bool MaGiamGiaDaTonTai(string maGiamGia, int? boQuaMaKM)
        {
            string ma = maGiamGia.Trim().ToUpper();
            using (var db = new QL_MatHangAnUongEntities())
                return db.KhuyenMais.Any(k =>
                    k.MaGiamGia == ma &&
                    (!boQuaMaKM.HasValue || k.MaKM != boQuaMaKM.Value));
        }
    }
}
