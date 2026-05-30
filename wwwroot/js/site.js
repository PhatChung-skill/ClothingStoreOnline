// Toggle hiện/ẩn mật khẩu
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.password-input-group').forEach(function (group) {
        var input = group.querySelector('input');
        var btn = group.querySelector('.password-toggle-btn');
        if (!input || !btn) return;

        btn.addEventListener('click', function () {
            var isPassword = input.type === 'password';
            input.type = isPassword ? 'text' : 'password';
            var icon = btn.querySelector('i');
            if (icon) {
                icon.classList.toggle('bi-eye', !isPassword);
                icon.classList.toggle('bi-eye-slash', isPassword);
            }
            btn.setAttribute('aria-label', isPassword ? 'Ẩn mật khẩu' : 'Hiện mật khẩu');
        });
    });

    // Chỉ cho phép số trong ô OTP
    document.querySelectorAll('.otp-input').forEach(function (input) {
        input.addEventListener('input', function () {
            this.value = this.value.replace(/\D/g, '').slice(0, 6);
        });
    });
});
