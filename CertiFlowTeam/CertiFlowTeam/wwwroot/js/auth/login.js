$(document).ready(function () {
    const loginForm = $('#loginForm');
    const loginBtn = $('#loginBtn');
    const successAlert = $('#successAlert');
    const errorAlert = $('#errorAlert');
    const successMessage = $('#successMessage');
    const errorMessage = $('#errorMessage');
    const passwordAddon = $('#password-addon');
    const passwordInput = $('#password');

    passwordAddon.on('click', function () {
        if (passwordInput.attr('type') === 'password') {
            passwordInput.attr('type', 'text');
            passwordAddon.html('<i class="mdi mdi-eye-off-outline"></i>');
        } else {
            passwordInput.attr('type', 'password');
            passwordAddon.html('<i class="mdi mdi-eye-outline"></i>');
        }
    });

    loginForm.on('submit', function (e) {
        e.preventDefault();

        if (!this.checkValidity()) {
            e.stopPropagation();
            loginForm.addClass('was-validated');
            return;
        }

        const formData = {
            email: $('#email').val(),
            password: passwordInput.val(),
            rememberMe: $('#rememberMe').is(':checked')
        };

        loginBtn.prop('disabled', true);
        loginBtn.html('<i class="mdi mdi-loading mdi-spin me-1"></i> Giriş yapılıyor...');

        $.ajax({
            url: '/Auth/DoLogin',
            type: 'POST',
            data: formData,
            success: function (response) {
                if (response.success) {
                    showSuccess(response.message);

                    setTimeout(function () {
                        window.location.href = response.redirectUrl;
                    }, 1000);
                } else {
                    showError(response.message);
                    loginBtn.prop('disabled', false);
                    loginBtn.html('<i class="mdi mdi-login me-1"></i> Giriş Yap');
                }
            },
            error: function (xhr, status, error) {
                showError('Giriş işlemi sırasında bir hata oluştu. Lütfen tekrar deneyiniz.');
                loginBtn.prop('disabled', false);
                loginBtn.html('<i class="mdi mdi-login me-1"></i> Giriş Yap');
            }
        });
    });

    function showSuccess(message) {
        errorAlert.addClass('d-none');
        successMessage.text(message);
        successAlert.removeClass('d-none');

        setTimeout(function () {
            successAlert.addClass('d-none');
        }, 5000);
    }

    function showError(message) {
        successAlert.addClass('d-none');
        errorMessage.text(message);
        errorAlert.removeClass('d-none');

        setTimeout(function () {
            errorAlert.addClass('d-none');
        }, 5000);
    }
});
