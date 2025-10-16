let dataTable;
let currentFilter = 'all';

$(document).ready(function () {
    initDataTable();
    loadDocuments();

    $('.filter-btn').on('click', function () {
        $('.filter-btn').removeClass('active');
        $(this).addClass('active');
        currentFilter = $(this).data('filter');
        loadDocuments();
    });
});

function initDataTable() {
    dataTable = $('#documentTable').DataTable({
        responsive: true,
        language: {
            paginate: {
                previous: "<i class='mdi mdi-chevron-left'>",
                next: "<i class='mdi mdi-chevron-right'>"
            },
            lengthMenu: "Göster _MENU_ kayıt",
            zeroRecords: "Kayıt bulunamadı",
            info: "_TOTAL_ kayıttan _START_ - _END_ arası gösteriliyor",
            infoEmpty: "Kayıt yok",
            infoFiltered: "(_MAX_ kayıt içinden filtrelendi)",
            search: "Ara:",
            emptyTable: "Tabloda veri bulunmuyor"
        },
        drawCallback: function () {
            $('.dataTables_paginate > .pagination').addClass('pagination-rounded');
        },
        columnDefs: [
            { orderable: false, targets: 6 }
        ],
        order: [[5, 'desc']]
    });
}

function loadDocuments() {
    $.ajax({
        url: '/Document/GetDocuments',
        type: 'POST',
        data: { filterType: currentFilter },
        success: function (response) {
            if (response.success && response.data) {
                renderDocuments(response.data);
            } else {
                dataTable.clear().draw();
                showNoData();
            }
        },
        error: function () {
            dataTable.clear().draw();
            showNoData();
        }
    });
}

function renderDocuments(documents) {
    const noDataMessage = $('#noDataMessage');

    if (!documents || documents.length === 0) {
        dataTable.clear().draw();
        $('.table-responsive').hide();
        noDataMessage.show();
        return;
    }

    $('.table-responsive').show();
    noDataMessage.hide();

    dataTable.clear();

    documents.forEach(function (doc) {
        const statusBadge = getStatusBadge(doc.approvalStatus);
        const statusText = getStatusText(doc.approvalStatus);
        const fileIcon = getFileIcon(doc.filePath);
        const date = new Date(doc.createdDate).toLocaleDateString('tr-TR');

        const documentNameHtml = `
            <div class="d-flex align-items-center">
                <div class="avatar-xs me-3">
                    <span class="avatar-title rounded-circle bg-light text-primary font-size-18">
                        <i class="${fileIcon}"></i>
                    </span>
                </div>
                <div class="flex-1">
                    <h5 class="font-size-14 mb-1">${doc.documentName}</h5>
                    ${doc.description ? `<p class="text-muted mb-0 font-size-12">${doc.description.substring(0, 40)}${doc.description.length > 40 ? '...' : ''}</p>` : ''}
                </div>
            </div>
        `;

        const statusHtml = `<span class="badge ${statusBadge} font-size-12">${statusText}</span>`;

        let actionsHtml = `
            <div class="d-flex gap-3">
                <a href="javascript:void(0);" class="text-primary" onclick="viewDocument(${doc.id})" title="Görüntüle">
                    <i class="mdi mdi-eye font-size-18"></i>
                </a>
                <a href="${doc.filePath}" target="_blank" class="text-success" title="İndir">
                    <i class="mdi mdi-download font-size-18"></i>
                </a>
        `;

        if (doc.approvalStatus === 1) {
            actionsHtml += `
                <a href="javascript:void(0);" class="text-success" onclick="approveDocument(${doc.id})" title="Onayla">
                    <i class="mdi mdi-check-circle font-size-18"></i>
                </a>
                <a href="javascript:void(0);" class="text-warning" onclick="showRejectModal(${doc.id})" title="Reddet">
                    <i class="mdi mdi-close-circle font-size-18"></i>
                </a>
            `;
        }

        actionsHtml += `
                <a href="javascript:void(0);" class="text-danger" onclick="deleteDocument(${doc.id})" title="Sil">
                    <i class="mdi mdi-delete font-size-18"></i>
                </a>
            </div>
        `;

        dataTable.row.add([
            documentNameHtml,
            doc.documentType || '-',
            doc.documentNumber || '-',
            doc.uploadedByUserName || '-',
            statusHtml,
            date,
            actionsHtml
        ]);
    });

    dataTable.draw();
}

function showNoData() {
    $('.table-responsive').hide();
    $('#noDataMessage').show();
}

function getStatusBadge(status) {
    switch (status) {
        case 0: return 'bg-secondary';
        case 1: return 'bg-warning';
        case 2: return 'bg-info';
        case 3: return 'bg-success';
        case 4: return 'bg-danger';
        case 5: return 'bg-dark';
        default: return 'bg-secondary';
    }
}

function getStatusText(status) {
    switch (status) {
        case 0: return 'Taslak';
        case 1: return 'Onay Bekliyor';
        case 2: return 'İnceleniyor';
        case 3: return 'Onaylandı';
        case 4: return 'Reddedildi';
        case 5: return 'İptal Edildi';
        default: return 'Bilinmiyor';
    }
}

function getFileIcon(filePath) {
    const extension = filePath.split('.').pop().toLowerCase();
    switch (extension) {
        case 'pdf': return 'bx bxs-file-pdf';
        case 'doc':
        case 'docx': return 'bx bxs-file-doc';
        case 'xls':
        case 'xlsx': return 'bx bxs-file-excel';
        case 'jpg':
        case 'jpeg':
        case 'png': return 'bx bxs-file-image';
        default: return 'bx bxs-file';
    }
}

function viewDocument(documentId) {
    const modal = new bootstrap.Modal(document.getElementById('documentDetailModal'));
    const content = $('#documentDetailContent');

    content.html(`
        <div class="text-center">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Yükleniyor...</span>
            </div>
        </div>
    `);

    modal.show();

    $.ajax({
        url: '/Document/GetDocumentDetail',
        type: 'POST',
        data: { documentId: documentId },
        success: function (response) {
            if (response.success && response.data) {
                const doc = response.data;
                const statusBadge = getStatusBadge(doc.approvalStatus);
                const statusText = getStatusText(doc.approvalStatus);
                const createdDate = new Date(doc.createdDate).toLocaleString('tr-TR');

                let html = `
                    <div class="row mb-3">
                        <div class="col-md-6">
                            <strong>Doküman Adı:</strong>
                            <p>${doc.documentName}</p>
                        </div>
                        <div class="col-md-6">
                            <strong>Durum:</strong>
                            <p><span class="badge ${statusBadge}">${statusText}</span></p>
                        </div>
                    </div>
                    <div class="row mb-3">
                        <div class="col-md-6">
                            <strong>Doküman Tipi:</strong>
                            <p>${doc.documentType || '-'}</p>
                        </div>
                        <div class="col-md-6">
                            <strong>Doküman Numarası:</strong>
                            <p>${doc.documentNumber || '-'}</p>
                        </div>
                    </div>
                    <div class="row mb-3">
                        <div class="col-12">
                            <strong>Açıklama:</strong>
                            <p>${doc.description || '-'}</p>
                        </div>
                    </div>
                    <div class="row mb-3">
                        <div class="col-md-6">
                            <strong>Yükleyen:</strong>
                            <p>${doc.uploadedByUserName || '-'}</p>
                        </div>
                        <div class="col-md-6">
                            <strong>Yüklenme Tarihi:</strong>
                            <p>${createdDate}</p>
                        </div>
                    </div>
                    ${doc.assignedToUserName ? `
                        <div class="row mb-3">
                            <div class="col-md-6">
                                <strong>Atanan Kişi:</strong>
                                <p>${doc.assignedToUserName}</p>
                            </div>
                        </div>
                    ` : ''}
                    ${doc.approvalDate ? `
                        <div class="row mb-3">
                            <div class="col-md-6">
                                <strong>Onaylayan:</strong>
                                <p>${doc.approvedByUserName || '-'}</p>
                            </div>
                            <div class="col-md-6">
                                <strong>Onay Tarihi:</strong>
                                <p>${new Date(doc.approvalDate).toLocaleString('tr-TR')}</p>
                            </div>
                        </div>
                    ` : ''}
                    ${doc.rejectionReason ? `
                        <div class="row mb-3">
                            <div class="col-12">
                                <strong>Red Nedeni:</strong>
                                <div class="alert alert-danger">${doc.rejectionReason}</div>
                            </div>
                        </div>
                    ` : ''}
                    <div class="row">
                        <div class="col-12">
                            <a href="${doc.filePath}" target="_blank" class="btn btn-primary">
                                <i class="bx bx-download me-1"></i> Dosyayı İndir
                            </a>
                        </div>
                    </div>
                `;

                content.html(html);
            } else {
                content.html(`<div class="alert alert-danger">${response.message || 'Doküman yüklenemedi'}</div>`);
            }
        },
        error: function () {
            content.html('<div class="alert alert-danger">Doküman detayı yüklenirken hata oluştu</div>');
        }
    });
}

function approveDocument(documentId) {
    if (!confirm('Bu dokümanı onaylamak istediğinize emin misiniz?')) {
        return;
    }

    $.ajax({
        url: '/Document/ApproveDocument',
        type: 'POST',
        data: { documentId: documentId },
        success: function (response) {
            if (response.success) {
                alert(response.message);
                loadDocuments();
            } else {
                alert(response.message);
            }
        },
        error: function () {
            alert('Doküman onaylanırken hata oluştu');
        }
    });
}

function showRejectModal(documentId) {
    $('#rejectDocumentId').val(documentId);
    $('#rejectionReason').val('');
    const modal = new bootstrap.Modal(document.getElementById('rejectModal'));
    modal.show();
}

function confirmReject() {
    const documentId = $('#rejectDocumentId').val();
    const rejectionReason = $('#rejectionReason').val().trim();

    if (!rejectionReason) {
        alert('Lütfen red nedenini giriniz');
        return;
    }

    $.ajax({
        url: '/Document/RejectDocument',
        type: 'POST',
        data: {
            documentId: documentId,
            rejectionReason: rejectionReason
        },
        success: function (response) {
            if (response.success) {
                alert(response.message);
                bootstrap.Modal.getInstance(document.getElementById('rejectModal')).hide();
                loadDocuments();
            } else {
                alert(response.message);
            }
        },
        error: function () {
            alert('Doküman reddedilirken hata oluştu');
        }
    });
}

function deleteDocument(documentId) {
    if (!confirm('Bu dokümanı silmek istediğinize emin misiniz?')) {
        return;
    }

    $.ajax({
        url: '/Document/DeleteDocument',
        type: 'POST',
        data: { documentId: documentId },
        success: function (response) {
            if (response.success) {
                alert(response.message);
                loadDocuments();
            } else {
                alert(response.message);
            }
        },
        error: function () {
            alert('Doküman silinirken hata oluştu');
        }
    });
}
