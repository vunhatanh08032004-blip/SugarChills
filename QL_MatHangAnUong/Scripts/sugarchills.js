/* ============================================================
   SugarChills - JavaScript dùng chung
   ============================================================ */
(function ($) {
    "use strict";

    /* ---------- Toast thông báo nhỏ ---------- */
    function hienToast(noiDung, laLoi) {
        var $t = $("#scToast");
        if ($t.length === 0) {
            $t = $('<div class="sc-toast" id="scToast"></div>').appendTo("body");
        }
        var icon = laLoi ? "bi-exclamation-circle-fill" : "bi-check-circle-fill";
        $t.html('<i class="bi ' + icon + '"></i> ').append(document.createTextNode(noiDung)).stop(true, true).fadeIn(180);
        clearTimeout($t.data("timer"));
        $t.data("timer", setTimeout(function () { $t.fadeOut(250); }, 2200));
    }

    $(function () {

        /* ---------- 1. Thêm nhanh vào giỏ bằng AJAX ----------
           Luôn ở lại trang hiện tại: thành công thì cập nhật số trên icon giỏ + toast,
           lỗi thì báo toast. Chỉ chuyển trang khi server yêu cầu đăng nhập. */
        $(document).on("submit", ".form-them-nhanh", function (e) {
            e.preventDefault();
            var $form = $(this);
            var $nut = $form.find("button[type=submit]");
            if ($nut.prop("disabled")) return;          // chống bấm liên tục gửi nhiều lần

            $nut.prop("disabled", true).addClass("dang-xu-ly");

            $.ajax({
                url: $form.attr("action"),
                type: "POST",
                data: $form.serialize(),
                dataType: "json",
                headers: { "X-Requested-With": "XMLHttpRequest" }
            }).done(function (kq) {
                if (kq && kq.canDangNhap) {
                    hienToast(kq.thongBao || "Bạn cần đăng nhập để tiếp tục.", true);
                    setTimeout(function () {
                        window.location.href = "/TaiKhoan/DangNhap?returnUrl=" +
                            encodeURIComponent(location.pathname + location.search);
                    }, 900);
                    return;
                }
                if (kq && kq.thanhCong) {
                    var $badge = $("#soLuongGioHang");
                    $badge.text(kq.soLuongGio).removeClass("sc-nay").width();   // reset animation
                    $badge.addClass("sc-nay");
                    hienToast(kq.thongBao);
                } else {
                    hienToast((kq && kq.thongBao) || "Không thêm được sản phẩm.", true);
                }
            }).fail(function (xhr) {
                // Không submit form kiểu thường nữa (sẽ bị chuyển sang trang giỏ hàng)
                if (window.console) console.error("[ThemNhanh] HTTP " + xhr.status, xhr.responseText);
                hienToast("Có lỗi khi thêm vào giỏ, vui lòng thử lại.", true);
            }).always(function () {
                $nut.prop("disabled", false).removeClass("dang-xu-ly");
            });
        });

        /* ---------- 1b. GỬI FORM NỀN (không tải lại trang) ----------
           Dùng cho: giỏ hàng (tăng/giảm, xóa món, xóa hết, áp mã, bỏ mã) và nút "Thêm vào giỏ" ở trang chi tiết.
           Cách làm: fetch POST y như form bình thường -> server xử lý rồi redirect về trang giỏ hàng ->
           đọc HTML trả về, lấy thông báo (TempData) + số lượng giỏ + (nếu cần) thay vùng #khuVucGioHang.
           Nhờ vậy toàn bộ logic tính tiền / mã giảm giá vẫn nằm ở server, không phải viết lại. */
        function guiFormNen(form, khiXong) {
            return fetch(form.action, {
                method: "POST",
                body: new FormData(form),
                credentials: "same-origin"          // gửi kèm cookie Session
            }).then(function (r) {
                if (!r.ok) throw new Error("HTTP " + r.status);
                return r.text().then(function (html) {
                    var doc = new DOMParser().parseFromString(html, "text/html");

                    // Thông báo TempData của trang kết quả -> hiện thành toast
                    var $tb = $(doc.getElementById("scThongBaoTrang")).find(".sc-alert");
                    if ($tb.length) {
                        hienToast($.trim($tb.find("span").text()), /sc-alert-(danger|warning)/.test($tb.attr("class")));
                    }
                    // Số lượng trên icon giỏ ở header
                    var badge = doc.getElementById("soLuongGioHang");
                    if (badge) {
                        var $b = $("#soLuongGioHang");
                        if ($b.text() !== badge.textContent) {
                            $b.text(badge.textContent).removeClass("sc-nay").width();
                            $b.addClass("sc-nay");
                        }
                    }
                    khiXong(doc, r.url);
                });
            });
        }

        var phienCapNhatGio = 0;   // chỉ áp dụng kết quả của yêu cầu mới nhất

        $(document).on("submit", "#khuVucGioHang form", function (e) {
            if (e.isDefaultPrevented()) return;
            if (!window.fetch || !window.DOMParser) return;   // trình duyệt quá cũ: gửi form kiểu thường

            var form = this, $form = $(form);
            e.preventDefault();
            e.stopImmediatePropagation();   // tự xử lý xác nhận ở đây, không để handler "can-xac-nhan" hỏi lần 2

            // Form xóa món / xóa hết: hỏi xác nhận TRƯỚC khi gửi
            if ($form.hasClass("can-xac-nhan") &&
                !window.confirm($form.data("hoi") || "Bạn chắc chắn muốn xóa?")) return;

            var phien = ++phienCapNhatGio;
            $("#khuVucGioHang").addClass("sc-dang-cap-nhat");

            guiFormNen(form, function (doc, urlKetQua) {
                if (phien !== phienCapNhatGio) return;
                var vungMoi = doc.getElementById("khuVucGioHang");
                if (!vungMoi) { window.location.href = urlKetQua; return; }   // server chuyển trang khác
                $("#khuVucGioHang").replaceWith(vungMoi);
            }).catch(function (err) {
                if (window.console) console.error("[GioHang]", err);
                form.submit();                                   // lỗi mạng: gửi kiểu thường cho chắc
            });
        });

        $(document).on("submit", "#formThemVaoGio", function (e) {
            if (e.isDefaultPrevented() || !window.fetch || !window.DOMParser) return;
            e.preventDefault();

            var form = this, $nut = $(form).find("button[type=submit]");
            if ($nut.hasClass("dang-xu-ly")) return;
            $nut.addClass("dang-xu-ly");

            guiFormNen(form, function (doc, urlKetQua) {
                // Server trả về trang đăng nhập (chưa đăng nhập...) thì chuyển hẳn sang đó
                if (!doc.getElementById("khuVucGioHang") && !doc.getElementById("formThemVaoGio")) {
                    window.location.href = urlKetQua;
                }
            }).catch(function (err) {
                if (window.console) console.error("[ThemVaoGio]", err);
                hienToast("Có lỗi khi thêm vào giỏ, vui lòng thử lại.", true);
            }).then(function () {
                $nut.removeClass("dang-xu-ly");
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
        // Đợi 350ms sau lần bấm cuối rồi mới gửi: bấm "+" liên tục 5 lần chỉ gửi 1 yêu cầu
        $(document).on("change", ".o-so-luong-gio", function () {
            var $form = $(this).closest("form");
            clearTimeout($form.data("henGio"));
            $form.data("henGio", setTimeout(function () { $form.trigger("submit"); }, 350));
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

        /* ---------- 9. Nút Scroll to Top ---------- */
        var $scrollBtn = $("#scrollToTopBtn");

        // Hiển thị/ẩn nút khi cuộn trang
        $(window).on("scroll", function () {
            if ($(window).scrollTop() > 200) {
                $scrollBtn.addClass("show");
            } else {
                $scrollBtn.removeClass("show");
            }
        });

        // Cuộn lên đầu trang khi bấm nút
        $scrollBtn.on("click", function (e) {
            e.preventDefault();
            $("html, body").animate({ scrollTop: 0 }, 300, "swing");
        });

        /* ---------- 10. Ảnh lỗi -> tự thay bằng ảnh placeholder ----------
           Thay cho onerror="this.onerror=null;this.src='...'" viết trực tiếp
           trong View (gây cảnh báo "'this' implicitly has type 'any'" của
           trình kiểm tra JS/TS trong Visual Studio khi phân tích thuộc tính
           inline). View chỉ cần gắn data-fallback="URL ảnh thay thế" lên thẻ
           <img>, JS dùng chung này sẽ lo phần xử lý lỗi tải ảnh. */
        $(document).on("error", "img[data-fallback]", function () {
            var $img = $(this);
            $img.off("error");
            $img.attr("src", $img.data("fallback"));
        });
    });

})(jQuery);
