using System;
using System.Collections.Generic;
using System.Linq;
using QL_MatHangAnUong.Helpers;

namespace QL_MatHangAnUong.Models
{
    /// <summary>
    /// KHO DỮ LIỆU TẠM (in-memory).
    /// </summary>
    public static class KhoDuLieu
    {
        private static readonly object _khoa = new object();
        private static bool _daKhoiTao;

        private static List<LoaiSanPham> _loais;
        private static List<SanPham> _sanPhams;
        private static List<NguoiDung> _nguoiDungs;
        private static List<KhuyenMai> _khuyenMais;
        private static List<DonHang> _donHangs;
        private static List<CuaHang> _cuaHangs;

        /// <summary>
        /// Danh sách đầy đủ, CHÍNH THỨC 34 Tỉnh/Thành phố của Việt Nam theo Nghị quyết sắp xếp
        /// đơn vị hành chính, có hiệu lực từ 1/7/2025 (cả nước từ 63 tỉnh/thành rút gọn còn 34).
        /// Dùng để đổ đủ vào dropdown lọc — không phụ thuộc việc tỉnh đó đã có chi nhánh hay chưa.
        /// </summary>
        private static readonly List<string> _tatCaTinhThanhVN = new List<string>
        {
            "An Giang", "Bắc Ninh", "Cà Mau", "Cao Bằng", "Cần Thơ", "Đà Nẵng", "Đắk Lắk",
            "Điện Biên", "Đồng Nai", "Đồng Tháp", "Gia Lai", "Hà Nội", "Hà Tĩnh", "Hải Phòng",
            "Huế", "Hưng Yên", "Khánh Hòa", "Lai Châu", "Lạng Sơn", "Lào Cai", "Lâm Đồng",
            "Nghệ An", "Ninh Bình", "Phú Thọ", "Quảng Ngãi", "Quảng Ninh", "Quảng Trị",
            "Sơn La", "Tây Ninh", "Thái Nguyên", "Thanh Hóa", "TP. Hồ Chí Minh",
            "Tuyên Quang", "Vĩnh Long"
        };

        private static int _idSanPham = 1;
        private static int _idLoai = 1;
        private static int _idNguoiDung = 1;
        private static int _idKhuyenMai = 1;
        private static int _idDonHang = 1000;
        private static int _idChiTiet = 1;
        private static int _idCuaHang = 1;

        public const string ThuMucAnhLoai = "/Content/Images/Loai/";
        public const string ThuMucAnhSanPham = "/Content/Images/SanPham/";

        #region Khởi tạo dữ liệu mẫu

        private static void BaoDamKhoiTao()
        {
            if (_daKhoiTao) return;
            lock (_khoa)
            {
                if (_daKhoiTao) return;
                TaoDuLieuMau();
                _daKhoiTao = true;
            }
        }

        private static void TaoDuLieuMau()
        {
            _loais = new List<LoaiSanPham>();
            _sanPhams = new List<SanPham>();
            _nguoiDungs = new List<NguoiDung>();
            _khuyenMais = new List<KhuyenMai>();
            _donHangs = new List<DonHang>();
            _cuaHangs = new List<CuaHang>();

            // ---------- LOẠI SẢN PHẨM ----------
            var traSua = ThemLoaiMau("Trà sữa", "Trà sữa pha từ trà ủ nóng mỗi ngày", ThuMucAnhLoai + "trasua.jpg", 1);
            var traTraiCay = ThemLoaiMau("Trà trái cây", "Trà thanh mát cùng trái cây tươi", "https://images.unsplash.com/photo-1497534446932-c925b458314e?auto=format&fit=crop&w=600&q=80", 2);
            var caPhe = ThemLoaiMau("Cà phê", "Cà phê rang xay nguyên chất", "https://images.unsplash.com/photo-1495474472287-4d71bcdd2085?auto=format&fit=crop&w=600&q=80", 3);
            var kem = ThemLoaiMau("Kem & Đá xay", "Đá xay và kem tươi mát lạnh", "https://images.unsplash.com/photo-1497034825429-c343d7c6a68f?auto=format&fit=crop&w=600&q=80", 4);
            var banh = ThemLoaiMau("Bánh ngọt", "Bánh ngọt nhà làm ăn kèm thức uống", "https://images.unsplash.com/photo-1578985545062-69928b1d9587?auto=format&fit=crop&w=600&q=80", 5);
            var monThem = ThemLoaiMau("Món thêm", "Topping và món ăn vặt", "https://images.unsplash.com/photo-1541599468348-e96984315921?auto=format&fit=crop&w=600&q=80", 6);

            // ---------- SẢN PHẨM ----------
            ThemSanPhamMau("Trà sữa trân châu đường đen", traSua.MaLoai, 45000, 39000,
                "https://images.unsplash.com/photo-1558857563-b371033873b8?auto=format&fit=crop&w=800&q=80",
                "Trà sữa béo nhẹ kết hợp trân châu đường đen dai mềm, nấu mới mỗi 2 tiếng.", true, 320);
            ThemSanPhamMau("Trà sữa hồng trà kem cheese", traSua.MaLoai, 49000, null,
                ThuMucAnhSanPham + "hongtrakemcheese.jpg",
                "Hồng trà đậm vị phủ lớp kem cheese mặn ngọt hài hòa.", true, 245);
            ThemSanPhamMau("Trà sữa khoai môn", traSua.MaLoai, 45000, null,
                ThuMucAnhSanPham +"tskhoaimon.jpg",
                "Khoai môn nghiền nguyên chất, thơm bùi, thêm trân châu trắng.", false, 180);
            ThemSanPhamMau("Trà sữa matcha Nhật Bản", traSua.MaLoai, 52000, 45000,
                ThuMucAnhSanPham + "matchanb.jpg",
                "Matcha Uji nguyên chất, vị chát nhẹ đặc trưng, ít ngọt.", true, 210);
            ThemSanPhamMau("Trà sữa socola", traSua.MaLoai, 47000, null,
                ThuMucAnhSanPham + "socola.jpg",
                "Socola Bỉ đậm đà, hợp cho bạn thích vị ngọt sâu.", false, 95);
            ThemSanPhamMau("Trà sữa oolong nướng", traSua.MaLoai, 49000, null,
                ThuMucAnhSanPham + "olongnuong.jpg",
                "Oolong nướng thơm mùi khói nhẹ, hậu vị ngọt thanh.", false, 130);

            ThemSanPhamMau("Trà đào cam sả", traTraiCay.MaLoai, 45000, 39000,
                ThuMucAnhSanPham + "tradaocamsa.jpg",
                "Đào ngâm giòn, cam tươi và sả thơm — món bán chạy nhất mùa hè.", true, 410);
            ThemSanPhamMau("Trà vải hoa hồng", traTraiCay.MaLoai, 45000, null,
                ThuMucAnhSanPham + "travaihh.jpg",
                "Vải ngọt mọng cùng hương hoa hồng dịu nhẹ.", true, 260);
            ThemSanPhamMau("Trà dâu tây tuyết", traTraiCay.MaLoai, 52000, null,
                ThuMucAnhSanPham + "tradautuyet.png",
                "Dâu Đà Lạt xay cùng trà xanh, phủ tuyết sữa.", false, 155);
            ThemSanPhamMau("Trà chanh giã tay", traTraiCay.MaLoai, 35000, 29000,
                ThuMucAnhSanPham + "trachanhgiatay.jpg",
                "Chanh tươi giã tay cùng trà xanh, giải nhiệt tức thì.", false, 300);
            ThemSanPhamMau("Trà ổi hồng bạc hà", traTraiCay.MaLoai, 45000, null,
                ThuMucAnhSanPham + "oihongbacha.jpg",
                "Ổi hồng ép tươi thêm lá bạc hà the mát.", false, 120);

            ThemSanPhamMau("Cà phê sữa đá", caPhe.MaLoai, 32000, null,
                ThuMucAnhSanPham + "cfsuada.jpg",
                "Robusta rang đậm pha phin, thêm sữa đặc và đá.", true, 380);
            ThemSanPhamMau("Bạc xỉu", caPhe.MaLoai, 35000, null,
                ThuMucAnhSanPham + "bacxiu.jpg",
                "Nhiều sữa, ít cà phê, hợp bạn mới tập uống cà phê.", false, 190);
            ThemSanPhamMau("Cà phê muối", caPhe.MaLoai, 39000, 35000,
                ThuMucAnhSanPham + "cfmuoi.jpg",
                "Lớp kem muối béo mặn trên nền cà phê đậm.", true, 275);
            ThemSanPhamMau("Latte hạnh nhân", caPhe.MaLoai, 49000, null,
                ThuMucAnhSanPham + "lattehn.jpg",
                "Espresso cùng sữa hạnh nhân, ít đường.", false, 88);
            ThemSanPhamMau("Cold brew cam quế", caPhe.MaLoai, 55000, null,
                ThuMucAnhSanPham + "coldbrew.jpg",
                "Cà phê ủ lạnh 18 tiếng, thêm cam và quế.", false, 70);

            ThemSanPhamMau("Đá xay socola chip", kem.MaLoai, 59000, 49000,
                ThuMucAnhSanPham + "sclchip.jpg",
                "Socola chip đá xay phủ kem tươi và vụn bánh.", true, 205);
            ThemSanPhamMau("Đá xay matcha đậu đỏ", kem.MaLoai, 59000, null,
                ThuMucAnhSanPham + "matchadaudo.jpg",
                "Matcha đá xay cùng đậu đỏ ninh mềm.", false, 140);
            ThemSanPhamMau("Kem tươi vani", kem.MaLoai, 29000, null,
                ThuMucAnhSanPham + "kemvani.jpg",
                "Kem tươi vani Madagascar, ngọt dịu.", false, 175);
            ThemSanPhamMau("Kem dâu bạc hà", kem.MaLoai, 32000, null,
                ThuMucAnhSanPham + "daubacha.jpg",
                "Hai vị kem dâu và bạc hà trong một ly.", false, 96);

            ThemSanPhamMau("Bánh su kem trứng muối", banh.MaLoai, 25000, null,
                ThuMucAnhSanPham + "sukemtrungmuoi.jpg",
                "Vỏ su giòn, nhân kem trứng muối béo mặn.", true, 230);
            ThemSanPhamMau("Bánh tiramisu", banh.MaLoai, 45000, 39000,
                "https://images.unsplash.com/photo-1571877227200-a0d98ea607e9?auto=format&fit=crop&w=800&q=80",
                "Tiramisu chuẩn Ý với mascarpone và cacao.", true, 165);
            ThemSanPhamMau("Bánh mousse dâu", banh.MaLoai, 42000, null,
                ThuMucAnhSanPham + "moussedau.jpg",
                "Mousse dâu mềm mịn, chua ngọt cân bằng.", false, 110);
            ThemSanPhamMau("Bánh croissant bơ", banh.MaLoai, 32000, null,
                "https://images.unsplash.com/photo-1555507036-ab1f4038808a?auto=format&fit=crop&w=800&q=80",
                "Croissant nướng mỗi sáng, thơm mùi bơ Pháp.", false, 145);

            ThemSanPhamMau("Trân châu đường đen (phần)", monThem.MaLoai, 10000, null,
                "https://images.unsplash.com/photo-1558857563-b371033873b8?auto=format&fit=crop&w=800&q=80",
                "Một phần trân châu đường đen nấu mới.", false, 420);
            ThemSanPhamMau("Khoai tây lắc phô mai", monThem.MaLoai, 35000, null,
                "https://images.unsplash.com/photo-1573080496219-bb080dd4f877?auto=format&fit=crop&w=800&q=80",
                "Khoai tây chiên giòn lắc bột phô mai.", false, 150);

            // ---------- NGƯỜI DÙNG ----------
            ThemNguoiDungMau("Chủ shop SugarChills", "seller@sugarchills.vn", "123456", "0901234567",
                "12 Nguyễn Văn Bảo, Gò Vấp, TP.HCM", NguoiDung.RoleSeller);
            ThemNguoiDungMau("Nguyễn Thị Ngọc", "khach@gmail.com", "123456", "0912345678",
                "45 Lê Văn Việt, TP. Thủ Đức, TP.HCM", NguoiDung.RoleCustomer);
            ThemNguoiDungMau("Trần Minh Khoa", "khoa.tran@gmail.com", "123456", "0987654321",
                "203 Quang Trung, Gò Vấp, TP.HCM", NguoiDung.RoleCustomer);

            // ---------- KHUYẾN MÃI ----------
            ThemKhuyenMaiMau("Chào bạn mới", "SUGAR10", 10, 20000, 0,
                DateTime.Today.AddDays(-10), DateTime.Today.AddDays(60),
                "Giảm 10% (tối đa 20.000đ) cho mọi đơn hàng.", KhuyenMai.GiamTongTien);
            ThemKhuyenMaiMau("Freeship đơn từ 100k", "CHILL15", 100, 15000, 100000,
                DateTime.Today.AddDays(-5), DateTime.Today.AddDays(30),
                "Miễn phí giao hàng cho đơn từ 100.000đ.", KhuyenMai.GiamPhiGiaoHang);
            ThemKhuyenMaiMau("Happy Hour 14h-17h", "HAPPY20", 20, 50000, 150000,
                DateTime.Today.AddDays(-2), DateTime.Today.AddDays(20),
                "Giảm 20% (tối đa 50.000đ) cho đơn từ 150.000đ.", KhuyenMai.GiamTongTien);
            ThemKhuyenMaiMau("Tết Trung Thu 2025", "TRUNGTHU", 25, 60000, 200000,
                DateTime.Today.AddDays(-120), DateTime.Today.AddDays(-90),
                "Chương trình đã kết thúc — dùng để minh họa khuyến mãi hết hạn.", KhuyenMai.GiamTongTien);

            TaoDonHangMau();

            // ---------- CỬA HÀNG (chi nhánh trên cả nước) ----------
            // Tên Tỉnh/Thành phố theo đúng 34 đơn vị hành chính sau sáp nhập (hiệu lực 1/7/2025).
            // Cấp Quận/Huyện đã bị bãi bỏ trên cả nước — mô hình hiện tại chỉ còn 2 cấp: Tỉnh/Thành → Phường/Xã.
            ThemCuaHangMau("SugarChills Gò Vấp", "12 Nguyễn Văn Bảo", "TP. Hồ Chí Minh", "Gò Vấp", "1900 6789", "07:00 - 22:00");
            ThemCuaHangMau("SugarChills Sài Gòn", "45 Nguyễn Huệ", "TP. Hồ Chí Minh", "Sài Gòn", "1900 6789", "07:00 - 23:00");
            ThemCuaHangMau("SugarChills Tân Hưng", "116 Nguyễn Thị Thập", "TP. Hồ Chí Minh", "Tân Hưng", "1900 6789", "07:00 - 22:00");
            ThemCuaHangMau("SugarChills Tây Thạnh", "140 Lê Trọng Tấn", "TP. Hồ Chí Minh", "Tây Thạnh", "1900 6789", "06:30 - 22:00");
            ThemCuaHangMau("SugarChills Vũng Tàu", "15 Thùy Vân", "TP. Hồ Chí Minh", "Vũng Tàu", "1900 6789", "07:00 - 22:30");
            ThemCuaHangMau("SugarChills Cầu Giấy", "25 Trần Duy Hưng", "Hà Nội", "Cầu Giấy", "1900 6789", "07:30 - 22:00");
            ThemCuaHangMau("SugarChills Hai Bà Trưng", "88 Bà Triệu", "Hà Nội", "Hai Bà Trưng", "1900 6789", "07:30 - 22:00");
            ThemCuaHangMau("SugarChills Hải Châu", "20 Bạch Đằng", "Đà Nẵng", "Hải Châu", "1900 6789", "07:00 - 22:30");
            ThemCuaHangMau("SugarChills Ngô Quyền", "10 Điện Biên Phủ", "Hải Phòng", "Ngô Quyền", "1900 6789", "07:30 - 22:00");
            ThemCuaHangMau("SugarChills Ninh Kiều", "5 Hòa Bình", "Cần Thơ", "Ninh Kiều", "1900 6789", "07:00 - 22:00");
            ThemCuaHangMau("SugarChills Nha Trang", "68 Trần Phú", "Khánh Hòa", "Nha Trang", "1900 6789", "07:00 - 23:00");
            ThemCuaHangMau("SugarChills Huế", "30 Lê Lợi", "Huế", "Phú Xuân", "1900 6789", "07:30 - 22:00");
            ThemCuaHangMau("SugarChills Biên Hòa", "88 Đồng Khởi", "Đồng Nai", "Biên Hòa", "1900 6789", "07:00 - 22:00");
        }

        private static LoaiSanPham ThemLoaiMau(string ten, string moTa, string hinh, int thuTu)
        {
            var l = new LoaiSanPham
            {
                MaLoai = _idLoai++,
                TenLoai = ten,
                MoTa = moTa,
                HinhAnh = hinh,
                ThuTu = thuTu,
                HienThi = true
            };
            _loais.Add(l);
            return l;
        }

        private static CuaHang ThemCuaHangMau(string ten, string diaChi, string tinhThanh, string quanHuyen,
            string sdt, string gioMoCua)
        {
            var ch = new CuaHang
            {
                MaCH = _idCuaHang++,
                TenCH = ten,
                DiaChi = diaChi,
                TinhThanh = tinhThanh,
                QuanHuyen = quanHuyen,
                SoDienThoai = sdt,
                GioMoCua = gioMoCua
            };
            _cuaHangs.Add(ch);
            return ch;
        }

        private static void ThemSanPhamMau(string ten, int maLoai, decimal gia, decimal? giaKM,
            string hinh, string moTa, bool noiBat, int luotBan)
        {
            _sanPhams.Add(new SanPham
            {
                MaSP = _idSanPham++,
                TenSP = ten,
                MaLoai = maLoai,
                Gia = gia,
                GiaKhuyenMai = giaKM,
                HinhAnh = hinh,
                MoTa = moTa,
                NoiBat = noiBat,
                DangBan = true,
                SoLuongTon = 100,
                LuotBan = luotBan,
                NgayTao = DateTime.Now.AddDays(-_idSanPham)
            });
        }

        private static void ThemNguoiDungMau(string hoTen, string email, string matKhau,
            string dienThoai, string diaChi, string vaiTro)
        {
            _nguoiDungs.Add(new NguoiDung
            {
                MaND = _idNguoiDung++,
                HoTen = hoTen,
                Email = email,
                MatKhau = matKhau,
                DienThoai = dienThoai,
                DiaChi = diaChi,
                VaiTro = vaiTro,
                NgayDangKy = DateTime.Now.AddDays(-30)
            });
        }

        private static void ThemKhuyenMaiMau(string ten, string ma, int phanTram, decimal toiDa,
            decimal donToiThieu, DateTime batDau, DateTime ketThuc, string moTa, string loaiGiam)
        {
            _khuyenMais.Add(new KhuyenMai
            {
                MaKM = _idKhuyenMai++,
                TenKM = ten,
                MaGiamGia = ma,
                PhanTramGiam = phanTram,
                GiamToiDa = toiDa,
                DonToiThieu = donToiThieu,
                NgayBatDau = batDau,
                NgayKetThuc = ketThuc,
                MoTa = moTa,
                LoaiGiam = loaiGiam,
                KichHoat = true
            });
        }

        /// <summary>Sinh một ít đơn hàng trong 30 ngày gần nhất để trang Thống kê có số liệu.</summary>
        private static void TaoDonHangMau()
        {
            var rnd = new Random(2025);          // seed cố định => dữ liệu ổn định mỗi lần chạy
            var trangThais = new[]
            {
                DonHang.HoanThanh, DonHang.HoanThanh, DonHang.HoanThanh,
                DonHang.DangGiao, DonHang.DaXacNhan, DonHang.ChoXacNhan, DonHang.DaHuy
            };
            var khachs = _nguoiDungs.Where(n => n.VaiTro == NguoiDung.RoleCustomer).ToList();

            for (int i = 0; i < 40; i++)
            {
                var khach = khachs[rnd.Next(khachs.Count)];
                var dh = new DonHang
                {
                    MaDH = ++_idDonHang,
                    MaND = khach.MaND,
                    TenNguoiNhan = khach.HoTen,
                    DienThoai = khach.DienThoai,
                    DiaChi = khach.DiaChi,
                    NgayDat = DateTime.Now.AddDays(-rnd.Next(0, 30)).AddHours(-rnd.Next(0, 12)),
                    TrangThai = trangThais[rnd.Next(trangThais.Length)],
                    HinhThucThanhToan = rnd.Next(2) == 0 ? "COD" : "Chuyển khoản",
                    PhiGiaoHang = 15000
                };

                int soDong = rnd.Next(1, 4);
                for (int j = 0; j < soDong; j++)
                {
                    var sp = _sanPhams[rnd.Next(_sanPhams.Count)];
                    int sl = rnd.Next(1, 4);
                    dh.ChiTietDonHangs.Add(new ChiTietDonHang
                    {
                        MaCT = _idChiTiet++,
                        MaDH = dh.MaDH,
                        MaSP = sp.MaSP,
                        TenSP = sp.TenSP,
                        SoLuong = sl,
                        DonGia = sp.GiaBanThucTe,
                        TuyChon = "Size M | Đường 70% | Đá 100%"
                    });
                }

                dh.TamTinh = dh.ChiTietDonHangs.Sum(c => c.ThanhTien);
                dh.TienGiam = 0;
                dh.TongTien = dh.TamTinh + dh.PhiGiaoHang - dh.TienGiam;
                _donHangs.Add(dh);
            }
        }

        #endregion

        #region Loại sản phẩm

        public static List<LoaiSanPham> LayLoais()
        {
            BaoDamKhoiTao();
            return _loais.OrderBy(l => l.ThuTu).ThenBy(l => l.TenLoai).ToList();
        }

        public static List<LoaiSanPham> LayLoaiHienThi()
        {
            return LayLoais().Where(l => l.HienThi).ToList();
        }

        public static LoaiSanPham LayLoai(int maLoai)
        {
            BaoDamKhoiTao();
            return _loais.FirstOrDefault(l => l.MaLoai == maLoai);
        }

        public static void ThemLoai(LoaiSanPham loai)
        {
            BaoDamKhoiTao();
            lock (_khoa)
            {
                loai.MaLoai = _idLoai++;
                _loais.Add(loai);
            }
        }

        public static bool CapNhatLoai(LoaiSanPham loai)
        {
            BaoDamKhoiTao();
            var cu = LayLoai(loai.MaLoai);
            if (cu == null) return false;

            cu.TenLoai = loai.TenLoai;
            cu.MoTa = loai.MoTa;
            cu.HinhAnh = loai.HinhAnh;
            cu.HienThi = loai.HienThi;
            cu.ThuTu = loai.ThuTu;
            return true;
        }

        /// <summary>Xóa loại. Trả về false nếu loại đang có sản phẩm (ràng buộc khóa ngoại).</summary>
        public static bool XoaLoai(int maLoai, out string thongBaoLoi)
        {
            BaoDamKhoiTao();
            thongBaoLoi = null;

            var loai = LayLoai(maLoai);
            if (loai == null)
            {
                thongBaoLoi = Ngu.S("SellerLoai_KhongTimThay");
                return false;
            }

            int soSP = _sanPhams.Count(s => s.MaLoai == maLoai);
            if (soSP > 0)
            {
                thongBaoLoi = string.Format(
                    Ngu.S("KhoDuLieu_KhongTheXoaLoaiFormat"),
                    loai.TenLoai, soSP);
                return false;
            }

            lock (_khoa) { _loais.Remove(loai); }
            return true;
        }

        public static int DemSanPhamTheoLoai(int maLoai)
        {
            BaoDamKhoiTao();
            return _sanPhams.Count(s => s.MaLoai == maLoai);
        }

        #endregion

        #region Sản phẩm

        /// <summary>Lấy toàn bộ sản phẩm (dành cho trang quản trị).</summary>
        public static List<SanPham> LaySanPhams()
        {
            BaoDamKhoiTao();
            GanLoaiChoSanPham();
            return _sanPhams.OrderByDescending(s => s.MaSP).ToList();
        }

        /// <summary>Sản phẩm đang bán (dành cho trang khách hàng).</summary>
        public static List<SanPham> LaySanPhamDangBan()
        {
            return LaySanPhams().Where(s => s.DangBan).ToList();
        }

        public static SanPham LaySanPham(int maSP)
        {
            BaoDamKhoiTao();
            GanLoaiChoSanPham();
            return _sanPhams.FirstOrDefault(s => s.MaSP == maSP);
        }

        /// <summary>Gán navigation property LoaiSanPham </summary>
        private static void GanLoaiChoSanPham()
        {
            foreach (var sp in _sanPhams)
                sp.LoaiSanPham = _loais.FirstOrDefault(l => l.MaLoai == sp.MaLoai);
        }

        public static void ThemSanPham(SanPham sp)
        {
            BaoDamKhoiTao();
            lock (_khoa)
            {
                sp.MaSP = _idSanPham++;
                sp.NgayTao = DateTime.Now;
                _sanPhams.Add(sp);
            }
        }

        public static bool CapNhatSanPham(SanPham sp)
        {
            BaoDamKhoiTao();
            var cu = _sanPhams.FirstOrDefault(s => s.MaSP == sp.MaSP);
            if (cu == null) return false;

            cu.TenSP = sp.TenSP;
            cu.MaLoai = sp.MaLoai;
            cu.Gia = sp.Gia;
            cu.GiaKhuyenMai = sp.GiaKhuyenMai;
            cu.HinhAnh = sp.HinhAnh;
            cu.MoTa = sp.MoTa;
            cu.SoLuongTon = sp.SoLuongTon;
            cu.DangBan = sp.DangBan;
            cu.NoiBat = sp.NoiBat;
            return true;
        }

        /// <summary>Bật/tắt trạng thái "nổi bật" của 1 sản phẩm (nút sao trong Quản lý sản phẩm).</summary>
        public static bool DoiTrangThaiNoiBat(int maSP)
        {
            BaoDamKhoiTao();
            var sp = _sanPhams.FirstOrDefault(s => s.MaSP == maSP);
            if (sp == null) return false;

            sp.NoiBat = !sp.NoiBat;
            return true;
        }

        public static bool XoaSanPham(int maSP, out string thongBaoLoi)
        {
            BaoDamKhoiTao();
            thongBaoLoi = null;

            var sp = _sanPhams.FirstOrDefault(s => s.MaSP == maSP);
            if (sp == null)
            {
                thongBaoLoi = Ngu.S("SellerProd_KhongTimThaySP");
                return false;
            }

            bool dangCoTrongDon = _donHangs.Any(d => d.ChiTietDonHangs.Any(c => c.MaSP == maSP)
                                                     && d.TrangThai != DonHang.HoanThanh
                                                     && d.TrangThai != DonHang.DaHuy);
            if (dangCoTrongDon)
            {
                thongBaoLoi = Ngu.S("KhoDuLieu_SPDangTrongDonChuaXuLy");
                return false;
            }

            lock (_khoa) { _sanPhams.Remove(sp); }
            return true;
        }

        /// <summary>
        /// Tìm kiếm + lọc + sắp xếp sản phẩm cho trang khách hàng.
        /// </summary>
        public static List<SanPham> LocSanPham(string tuKhoa, int? maLoai, decimal? giaTu, decimal? giaDen, string sapXep)
        {
            var ds = LaySanPhamDangBan().AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                string tk = tuKhoa.Trim().ToLower();
                ds = ds.Where(s => s.TenSP.ToLower().Contains(tk)
                                   || (s.MoTa != null && s.MoTa.ToLower().Contains(tk)));
            }

            if (maLoai.HasValue && maLoai.Value > 0)
                ds = ds.Where(s => s.MaLoai == maLoai.Value);

            if (giaTu.HasValue)
                ds = ds.Where(s => s.GiaBanThucTe >= giaTu.Value);

            if (giaDen.HasValue)
                ds = ds.Where(s => s.GiaBanThucTe <= giaDen.Value);

            switch (sapXep)
            {
                case "gia-tang": ds = ds.OrderBy(s => s.GiaBanThucTe); break;
                case "gia-giam": ds = ds.OrderByDescending(s => s.GiaBanThucTe); break;
                case "ten": ds = ds.OrderBy(s => s.TenSP); break;
                case "ban-chay": ds = ds.OrderByDescending(s => s.LuotBan); break;
                case "moi": ds = ds.OrderByDescending(s => s.NgayTao); break;
                case "giam-gia":
                    ds = ds.Where(s => s.GiaKhuyenMai.HasValue && s.GiaKhuyenMai > 0)
                           .OrderByDescending(s => s.PhanTramGiam);
                    break;
                default: ds = ds.OrderByDescending(s => s.NoiBat).ThenByDescending(s => s.LuotBan); break;
            }

            return ds.ToList();
        }

        /// <summary>Sản phẩm liên quan: cùng loại, khác chính nó.</summary>
        public static List<SanPham> LaySanPhamLienQuan(int maSP, int maLoai, int soLuong = 4)
        {
            return LaySanPhamDangBan()
                .Where(s => s.MaLoai == maLoai && s.MaSP != maSP)
                .OrderByDescending(s => s.LuotBan)
                .Take(soLuong)
                .ToList();
        }

        #endregion

        #region Người dùng

        public static List<NguoiDung> LayNguoiDungs()
        {
            BaoDamKhoiTao();
            return _nguoiDungs.ToList();
        }

        public static NguoiDung LayNguoiDung(int maND)
        {
            BaoDamKhoiTao();
            return _nguoiDungs.FirstOrDefault(n => n.MaND == maND);
        }

        public static bool EmailDaTonTai(string email)
        {
            BaoDamKhoiTao();
            if (string.IsNullOrWhiteSpace(email)) return false;
            return _nguoiDungs.Any(n => n.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        public static NguoiDung KiemTraDangNhap(string email, string matKhau)
        {
            BaoDamKhoiTao();
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(matKhau)) return null;

            return _nguoiDungs.FirstOrDefault(n =>
                n.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase)
                && n.MatKhau == matKhau);
        }

        public static NguoiDung ThemNguoiDung(NguoiDung nd)
        {
            BaoDamKhoiTao();
            lock (_khoa)
            {
                nd.MaND = _idNguoiDung++;
                nd.NgayDangKy = DateTime.Now;
                _nguoiDungs.Add(nd);
            }
            return nd;
        }
        public static bool CapNhatThongTinNguoiDung(int maND, string hoTen, string dienThoai, string diaChi)
        {
            BaoDamKhoiTao();
            lock (_khoa)
            {
                var nd = _nguoiDungs.FirstOrDefault(n => n.MaND == maND);
                if (nd == null) return false;

                nd.HoTen = hoTen.Trim();
                nd.DienThoai = dienThoai.Trim();
                nd.DiaChi = string.IsNullOrWhiteSpace(diaChi) ? null : diaChi.Trim();
                return true;
            }
        }

        /// <summary>
        /// Đổi mật khẩu: bắt buộc phải cung cấp đúng mật khẩu hiện tại mới cho đổi.
        /// Trả về false nếu không tìm thấy người dùng hoặc mật khẩu hiện tại sai.
        /// Lưu ý: đồ án demo nên lưu mật khẩu thẳng (plain text), thực tế cần băm
        /// (SHA256 + salt hoặc BCrypt) trước khi so sánh / lưu.
        /// </summary>
        public static bool DoiMatKhau(int maND, string matKhauHienTai, string matKhauMoi)
        {
            BaoDamKhoiTao();
            lock (_khoa)
            {
                var nd = _nguoiDungs.FirstOrDefault(n => n.MaND == maND);
                if (nd == null) return false;
                if (nd.MatKhau != matKhauHienTai) return false;

                nd.MatKhau = matKhauMoi;
                return true;
            }
        }

        #endregion

        #region Khuyến mãi

        public static List<KhuyenMai> LayKhuyenMais()
        {
            BaoDamKhoiTao();
            return _khuyenMais.OrderByDescending(k => k.MaKM).ToList();
        }

        public static List<KhuyenMai> LayKhuyenMaiConHieuLuc()
        {
            return LayKhuyenMais().Where(k => k.ConHieuLuc).ToList();
        }

        public static KhuyenMai LayKhuyenMai(int maKM)
        {
            BaoDamKhoiTao();
            return _khuyenMais.FirstOrDefault(k => k.MaKM == maKM);
        }

        public static KhuyenMai TimTheoMaGiamGia(string ma)
        {
            BaoDamKhoiTao();
            if (string.IsNullOrWhiteSpace(ma)) return null;
            return _khuyenMais.FirstOrDefault(k =>
                k.MaGiamGia.Equals(ma.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        public static bool MaGiamGiaDaTonTai(string ma, int? boQuaMaKM = null)
        {
            BaoDamKhoiTao();
            if (string.IsNullOrWhiteSpace(ma)) return false;
            return _khuyenMais.Any(k => k.MaGiamGia.Equals(ma.Trim(), StringComparison.OrdinalIgnoreCase)
                                        && (!boQuaMaKM.HasValue || k.MaKM != boQuaMaKM.Value));
        }

        public static void ThemKhuyenMai(KhuyenMai km)
        {
            BaoDamKhoiTao();
            lock (_khoa)
            {
                km.MaKM = _idKhuyenMai++;
                _khuyenMais.Add(km);
            }
        }

        public static bool CapNhatKhuyenMai(KhuyenMai km)
        {
            BaoDamKhoiTao();
            var cu = LayKhuyenMai(km.MaKM);
            if (cu == null) return false;

            cu.TenKM = km.TenKM;
            cu.MaGiamGia = km.MaGiamGia;
            cu.PhanTramGiam = km.PhanTramGiam;
            cu.GiamToiDa = km.GiamToiDa;
            cu.DonToiThieu = km.DonToiThieu;
            cu.NgayBatDau = km.NgayBatDau;
            cu.NgayKetThuc = km.NgayKetThuc;
            cu.MoTa = km.MoTa;
            cu.KichHoat = km.KichHoat;
            return true;
        }

        public static bool XoaKhuyenMai(int maKM)
        {
            BaoDamKhoiTao();
            var km = LayKhuyenMai(maKM);
            if (km == null) return false;
            lock (_khoa) { _khuyenMais.Remove(km); }
            return true;
        }

        #endregion

        #region Đơn hàng

        public static List<DonHang> LayDonHangs()
        {
            BaoDamKhoiTao();
            return _donHangs.OrderByDescending(d => d.NgayDat).ToList();
        }

        public static List<DonHang> LayDonHangCuaKhach(int maND)
        {
            BaoDamKhoiTao();
            return _donHangs.Where(d => d.MaND == maND)
                            .OrderByDescending(d => d.NgayDat)
                            .ToList();
        }

        public static DonHang LayDonHang(int maDH)
        {
            BaoDamKhoiTao();
            var dh = _donHangs.FirstOrDefault(d => d.MaDH == maDH);
            if (dh != null && dh.MaND.HasValue)
                dh.NguoiDung = LayNguoiDung(dh.MaND.Value);
            return dh;
        }

        public static DonHang ThemDonHang(DonHang dh)
        {
            BaoDamKhoiTao();
            lock (_khoa)
            {
                dh.MaDH = ++_idDonHang;
                foreach (var ct in dh.ChiTietDonHangs)
                {
                    ct.MaCT = _idChiTiet++;
                    ct.MaDH = dh.MaDH;

                    // Trừ tồn kho và cộng lượt bán
                    var sp = _sanPhams.FirstOrDefault(s => s.MaSP == ct.MaSP);
                    if (sp != null)
                    {
                        sp.SoLuongTon = Math.Max(0, sp.SoLuongTon - ct.SoLuong);
                        sp.LuotBan += ct.SoLuong;
                    }
                }
                _donHangs.Add(dh);
            }
            return dh;
        }

        public static bool CapNhatTrangThaiDonHang(int maDH, string trangThai)
        {
            BaoDamKhoiTao();
            var dh = _donHangs.FirstOrDefault(d => d.MaDH == maDH);
            if (dh == null) return false;
            if (!DonHang.CacTrangThai.Contains(trangThai)) return false;

            dh.TrangThai = trangThai;

            // Đơn hoàn thành nghĩa là đã giao và đã thu đủ tiền (kể cả COD).
            if (trangThai == DonHang.HoanThanh)
            {
                dh.DaThanhToan = true;
                dh.SoTienConLai = 0;
            }

            return true;
        }

        #endregion

        #region Cửa hàng

        /// <summary>Toàn bộ chi nhánh, có thể lọc theo Tỉnh/Thành và/hoặc Phường/Xã.</summary>
        public static List<CuaHang> LayCuaHangs(string tinhThanh = null, string quanHuyen = null)
        {
            BaoDamKhoiTao();
            var ds = _cuaHangs.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(tinhThanh))
                ds = ds.Where(c => c.TinhThanh == tinhThanh);

            if (!string.IsNullOrWhiteSpace(quanHuyen))
                ds = ds.Where(c => c.QuanHuyen == quanHuyen);

            return ds.OrderBy(c => c.TinhThanh).ThenBy(c => c.QuanHuyen).ToList();
        }

        /// <summary>
        /// Danh sách đầy đủ 34 Tỉnh/Thành phố hiện hành của Việt Nam (dùng để đổ vào dropdown lọc) —
        /// hiện luôn TẤT CẢ tỉnh/thành, kể cả nơi chưa có chi nhánh. Nếu chọn 1 tỉnh chưa có chi nhánh,
        /// danh sách Phường/Xã và kết quả bên dưới sẽ tự động rỗng (không hiện chi nhánh nào).
        /// </summary>
        public static List<string> LayDanhSachTinhThanh()
        {
            return _tatCaTinhThanhVN;
        }

        /// <summary>Danh sách Phường/Xã thuộc 1 Tỉnh/Thành (dropdown con, phụ thuộc dropdown Tỉnh/Thành).</summary>
        public static List<string> LayQuanHuyenTheoTinh(string tinhThanh)
        {
            BaoDamKhoiTao();
            if (string.IsNullOrWhiteSpace(tinhThanh)) return new List<string>();

            return _cuaHangs.Where(c => c.TinhThanh == tinhThanh)
                             .Select(c => c.QuanHuyen)
                             .Distinct()
                             .OrderBy(q => q)
                             .ToList();
        }

        #endregion
    }
}
