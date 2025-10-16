$(document).ready(function () {
    loadCompanyUsers();

    $('#uploadDocumentForm').on('submit', function (e) {
        e.preventDefault();
        uploadDocument();
    });

    $('#file').on('change', function() {
        const file = this.files[0];
        if (file) {
            const fileSize = file.size / 1024 / 1024;
            const allowedExtensions = ['.pdf', '.doc', '.docx', '.xls', '.xlsx', '.jpg', '.jpeg', '.png'];
            const fileExtension = '.' + file.name.split('.').pop().toLowerCase();

            if (!allowedExtensions.includes(fileExtension)) {
                alert('Geçersiz dosya formatı. İzin verilen formatlar: PDF, DOC, DOCX, XLS, XLSX, JPG, PNG');
                $(this).val('');
                return;
            }

            if (fileSize > 10) {
                alert('Dosya boyutu 10MB\'dan büyük olamaz');
                $(this).val('');
                return;
            }
        }
    });
});

function loadCompanyUsers() {
    $.ajax({
        url: '/Document/GetCompanyUsers',
        type: 'GET',
        success: function (response) {
            if (response.success && response.data) {
                const select = $('#assignedToUserId');
                select.empty();
                select.append('<option value="">Seçiniz</option>');

                response.data.forEach(function (user) {
                    select.append(`<option value="${user.id}">${user.fullName} (${user.email})</option>`);
                });
            }
        },
        error: function () {
            console.error('Kullanıcılar yüklenirken hata oluştu');
        }
    });
}

function uploadDocument() {
    const form = document.getElementById('uploadDocumentForm');
    const formData = new FormData(form);

    const documentName = $('#documentName').val().trim();
    if (!documentName) {
        alert('Lütfen doküman adı giriniz');
        return;
    }

    const file = $('#file')[0].files[0];
    if (!file) {
        alert('Lütfen bir dosya seçiniz');
        return;
    }

    const btnUpload = $('#btnUpload');
    btnUpload.prop('disabled', true);
    btnUpload.html('<i class="bx bx-loader bx-spin me-1"></i> Yükleniyor...');

    $.ajax({
        url: '/Document/UploadDocument',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function (response) {
            if (response.success) {
                alert(response.message);
                window.location.href = '/Document/Index';
            } else {
                alert(response.message);
                btnUpload.prop('disabled', false);
                btnUpload.html('<i class="bx bx-upload me-1"></i> Yükle');
            }
        },
        error: function () {
            alert('Doküman yüklenirken bir hata oluştu');
            btnUpload.prop('disabled', false);
            btnUpload.html('<i class="bx bx-upload me-1"></i> Yükle');
        }
    });
}
