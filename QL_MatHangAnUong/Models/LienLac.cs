using System;

namespace QL_MatHangAnUong.Models
{
    /// <summary>
    /// Tin nhắn liên hệ khách gửi từ trang /Home/LienHe.
    /// Class thường (POCO), KHÔNG nằm trong EDMX: đọc/ghi bằng
    /// db.Database.SqlQuery / ExecuteSqlCommand nên không cần cập nhật model .edmx.
    /// Tên thuộc tính phải trùng tên cột trong bảng dbo.LienLacs.
    /// </summary>
    public class LienLac
    {
        public int MaLienLac { get; set; }
        public string HoTen { get; set; }
        public string SoDienThoai { get; set; }
        public string Email { get; set; }
        public string NoiDung { get; set; }
        public DateTime NgayGui { get; set; }
        public bool DaDoc { get; set; }
    }
}
