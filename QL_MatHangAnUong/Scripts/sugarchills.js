/* ============================================================
   SugarChills - JavaScript dùng chung
   ============================================================ */
(function ($) {
    "use strict";

    /* ---------- Toast thông báo nhỏ ---------- */
    function hienToast(noiDung) {
        var $t = $("#scToast");
        if ($t.length === 0) {
            $t = $('<div class="sc-toast" id="scToast"></div>').appendTo("body");
        }
        $t.html('<i class="bi bi-check-circle-fill"></i> ' + noiDung).fadeIn(180);
        clearTimeout($t.data("timer"));
        $t.data("timer", setTimeout(function () { $t.fadeOut(250); }, 2200));
    }

    $(function () {

        /* ---------- 1. Thêm nhanh vào giỏ bằng AJAX ---------- */
        $(document).on("submit", ".form-them-nhanh", function (e) {
            e.preventDefault();
            var $form = $(this);

            $.ajax({
                url: $form.attr("action"),
                type: "POST",
                data: $form.serialize(),
                headers: { "X-Requested-With": "XMLHttpRequest" }
            }).done(function (kq) {
                if (kq && kq.canDangNhap) {
                    window.location.href = "/TaiKhoan/DangNhap";
                    return;
                }
                if (kq && kq.thanhCong) {
                    $("#soLuongGioHang").text(kq.soLuongGio);
                    hienToast(kq.thongBao);
                } else {
                    hienToast((kq && kq.thongBao) || "Không thêm được sản phẩm.");
                }
            }).fail(function () {
                // Nếu AJAX lỗi thì submit form bình thường để không chặn người dùng
                $form.off("submit").submit();
            });
        });

        /* ---------- 2. Gợi ý tìm kiếm ---------- */
        var timerGoiY = null;
        $("#oTimKiem").on("keyup", function () {
            var tuKhoa = $(this).val();
            var $box = $("#goiYTimKiem");

            clearTimeout(timerGoiY);
            if (tuKhoa.length < 2) { $box.hide().empty(); return; }

            timerGoiY = setTimeout(function () {
                $.getJSON("/SanPham/GoiY", { tuKhoa: tuKhoa }, function (ds) {
                    if (!ds || ds.length === 0) { $box.hide().empty(); return; }

                    var html = "";
                    $.each(ds, function (i, sp) {
                        html += '<a href="/SanPham/ChiTiet/' + sp.maSP + '">' +
                            '<img src="' + sp.hinhAnh + '" onerror="this.src=\'https://placehold.co/60x60/FFE6EE/E8517F?text=SC\'" />' +
                            '<span>' + sp.tenSP + '</span>' +
                            '<span class="ms-auto sc-text-primary fw-bold">' +
                            Number(sp.gia).toLocaleString("vi-VN") + 'đ</span></a>';
                    });
                    $box.html(html).show();
                });
            }, 250);
        });

        $(document).on("click", function (e) {
            if (!$(e.target).closest(".sc-search").length) $("#goiYTimKiem").hide();
        });

        /* ---------- 3. Nút tăng/giảm số lượng ---------- */
        $(document).on("click", ".sc-qty button", function (e) {
            e.preventDefault();
            var $input = $(this).siblings("input");
            var buoc = $(this).data("buoc") === "tang" ? 1 : -1;
            var giaTri = parseInt($input.val(), 10) || 1;

            giaTri += buoc;
            if (giaTri < 1) giaTri = 1;
            if (giaTri > 100) giaTri = 100;

            $input.val(giaTri).trigger("change");
        });

        /* ---------- 4. Giỏ hàng: đổi số lượng thì tự submit form dòng đó ---------- */
        $(document).on("change", ".o-so-luong-gio", function () {
            $(this).closest("form").submit();
        });

        /* ---------- 5. Xác nhận trước khi xóa ---------- */
        $(document).on("submit", "form.can-xac-nhan", function () {
            var noiDung = $(this).data("hoi") || "Bạn chắc chắn muốn xóa?";
            return window.confirm(noiDung);
        });

        /* ---------- 6. Xem trước ảnh khi nhập URL ở form quản trị ---------- */
        $(document).on("input", "#HinhAnh", function () {
            var url = $(this).val();
            $("#xemTruocAnh").attr("src", url || "https://placehold.co/300x300/FFE6EE/E8517F?text=SugarChills");
        });

        /* ---------- 6b. Form sản phẩm: chọn nguồn ảnh (dán link / tải từ máy) ---------- */
        $(document).on("change", "#chonAnhLink, #chonAnhUpload", function () {
            var dungUpload = $("#chonAnhUpload").is(":checked");
            $("#khungLinkAnh").toggle(!dungUpload);
            $("#khungUploadAnh").toggle(dungUpload);
        });

        $(document).on("change", "#AnhTaiLen", function () {
            var file = this.files && this.files[0];
            if (!file) return;

            var doc = new FileReader();
            doc.onload = function (e) {
                $("#xemTruocAnh").attr("src", e.target.result);
            };
            doc.readAsDataURL(file);
        });

        /* ---------- 6c. Trang đặt hàng: chọn "Thanh toán bằng QR" -> hiện mã QR, đợi 10s rồi
           mô phỏng thanh toán thành công (số tiền về 0) và tự cập nhật/hoàn tất đơn hàng. ---------- */
        var demNguocQRTimer = null;

        function dungDemNguocQR() {
            if (demNguocQRTimer) {
                clearInterval(demNguocQRTimer);
                demNguocQRTimer = null;
            }
        }

        function resetKhungQR() {
            dungDemNguocQR();
            $("#DaThanhToanQR").val("false");
            $("#demNguocQR").text("10");
            $("#trangThaiQR")
                .css("color", "var(--sc-primary, #E8517F)")
                .html('<i class="bi bi-hourglass-split"></i> Đang chờ xác nhận thanh toán... <span id="demNguocQR">10</span>s');
            var $soTien = $("#soTienCanTT");
            if ($soTien.length) { $soTien.text($soTien.data("goc")); }
            $("#btnXacNhanDatHang").prop("disabled", false);
        }

        function batDauDemNguocQR() {
            resetKhungQR();
            var giay = 10;

            demNguocQRTimer = setInterval(function () {
                giay--;
                $("#demNguocQR").text(giay);

                if (giay <= 0) {
                    dungDemNguocQR();

                    // Mô phỏng cổng thanh toán báo thành công: số tiền cần thanh toán về 0.
                    $("#soTienCanTT").text("0đ");
                    $("#trangThaiQR")
                        .css("color", "#2FA98A")
                        .html('<i class="bi bi-check-circle-fill"></i> Đã nhận được thanh toán! Đang cập nhật đơn hàng...');
                    $("#DaThanhToanQR").val("true");

                    // Tự động hoàn tất đặt hàng ngay khi đã "thanh toán" xong.
                    var $form = $("#btnXacNhanDatHang").closest("form");
                    if ($form.length) {
                        setTimeout(function () { $form.trigger("submit"); }, 600);
                    }
                }
            }, 1000);
        }

        function capNhatKhungQR() {
            var $khung = $("#khungThanhToanQR");
            if ($khung.length === 0) return;

            var laQR = $("#ttQR").is(":checked");
            $khung.toggle(laQR);

            if (laQR) {
                batDauDemNguocQR();
            } else {
                resetKhungQR();
            }
        }
        $(document).on("change", "input[name='HinhThucThanhToan']", capNhatKhungQR);
        if ($("#khungThanhToanQR").length) { capNhatKhungQR(); }

        /* ---------- 7. Mở/đóng sidebar quản trị trên màn hình nhỏ ---------- */
        $("#nutMoSidebar").on("click", function () {
            $("#sidebarQuanTri").toggleClass("open");
        });

        /* ---------- 8. Tính lại giá theo tùy chọn ở trang chi tiết sản phẩm ---------- */
        function tinhLaiGia() {
            var $khung = $("#khungTuyChon");
            if ($khung.length === 0) return;

            var giaGoc = parseFloat($khung.data("gia-goc")) || 0;
            var phuThu = parseFloat($("input[name='size']:checked").data("phu-thu")) || 0;

            $("input[name='toppings']:checked").each(function () {
                phuThu += parseFloat($(this).data("phu-thu")) || 0;
            });

            var soLuong = parseInt($("#SoLuongMua").val(), 10) || 1;
            var tong = (giaGoc + phuThu) * soLuong;

            $("#tamTinhChiTiet").text(tong.toLocaleString("vi-VN") + "đ");
        }

        $(document).on("change", "#khungTuyChon input", tinhLaiGia);
        tinhLaiGia();
    });

})(jQuery);
