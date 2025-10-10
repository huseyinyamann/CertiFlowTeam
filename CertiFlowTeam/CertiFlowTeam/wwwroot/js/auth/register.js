$(document).ready(function () {
    const registerForm = $('#registerForm');
    const registerBtn = $('#registerBtn');
    const successAlert = $('#successAlert');
    const errorAlert = $('#errorAlert');
    const successMessage = $('#successMessage');
    const errorMessage = $('#errorMessage');

    registerForm.on('submit', function (e) {
        e.preventDefault();

        if (!this.checkValidity()) {
            e.stopPropagation();
            registerForm.addClass('was-validated');
            return;
        }

        const password = $('#password').val();
        const passwordConfirm = $('#passwordConfirm').val();

        if (password !== passwordConfirm) {
            showError('Şifreler eşleşmiyor!');
            return;
        }

        if (password.length < 6) {
            showError('Şifre en az 6 karakter olmalıdır!');
            return;
        }

        const formData = {
            companyName: $('#companyName').val(),
            taxNumber: $('#taxNumber').val(),
            address: $('#address').val(),
            companyPhone: $('#companyPhone').val(),
            companyEmail: $('#companyEmail').val(),
            authorizedPerson: $('#authorizedPerson').val(),
            authorizedPhone: $('#authorizedPhone').val(),
            fullName: $('#fullName').val(),
            userEmail: $('#userEmail').val(),
            userPhone: $('#userPhone').val(),
            password: password,
            passwordConfirm: passwordConfirm
        };

        registerBtn.prop('disabled', true);
        registerBtn.html('<i class="mdi mdi-loading mdi-spin me-1"></i> Kayıt yapılıyor...');

        $.ajax({
            url: '/Auth/DoRegister',
            type: 'POST',
            data: formData,
            success: function (response) {
                if (response.success) {
                    showSuccess(response.message);
                    registerForm[0].reset();
                    registerForm.removeClass('was-validated');

                    setTimeout(function () {
                        window.location.href = response.redirectUrl;
                    }, 1500);
                } else {
                    showError(response.message);
                    registerBtn.prop('disabled', false);
                    registerBtn.html('<i class="mdi mdi-account-plus me-1"></i> Kayıt Ol');
                }
            },
            error: function (xhr, status, error) {
                showError('Kayıt işlemi sırasında bir hata oluştu. Lütfen tekrar deneyiniz.');
                registerBtn.prop('disabled', false);
                registerBtn.html('<i class="mdi mdi-account-plus me-1"></i> Kayıt Ol');
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
