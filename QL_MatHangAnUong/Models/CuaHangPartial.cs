namespace QL_MatHangAnUong.Models
{
    public partial class CuaHang
    {
        /// <summary>Địa chỉ đầy đủ: DiaChi, QuanHuyen, TinhThanh.</summary>
        public string DiaChiDayDu
        {
            get
            {
                var parts = new System.Collections.Generic.List<string>();
                if (!string.IsNullOrWhiteSpace(DiaChi))    parts.Add(DiaChi.Trim());
                if (!string.IsNullOrWhiteSpace(QuanHuyen)) parts.Add(QuanHuyen.Trim());
                if (!string.IsNullOrWhiteSpace(TinhThanh)) parts.Add(TinhThanh.Trim());
                return parts.Count > 0 ? string.Join(", ", parts) : TenCH;
            }
        }
    }
}
