// ===== EMAILJS CONFIG =====
const EMAILJS_PUBLIC_KEY  = "c2mCh8VYFr_l9mdmy";
const EMAILJS_SERVICE_ID  = "service_cjsueiu";
const EMAILJS_TEMPLATE_ID = "template_sb53noo";

emailjs.init(EMAILJS_PUBLIC_KEY);

// ===== MODAL =====
function openModal(id) {
    document.getElementById(id).classList.add('active');
    document.body.style.overflow = 'hidden';
}

function closeModal(id) {
    document.getElementById(id).classList.remove('active');
    document.body.style.overflow = '';
}

document.querySelectorAll('.modal-overlay').forEach(function (el) {
    el.addEventListener('click', function (e) {
        if (e.target === el) closeModal(el.id);
    });
});

// ===== VIEW MODAL =====
function openViewModal(btn) {
    var d = btn.dataset;
    var initials = d.fullname.trim().split(/\s+/).filter(Boolean).map(function (w) { return w[0]; }).slice(0, 2).join('').toUpperCase();
    document.getElementById('view_initials').textContent   = initials;
    document.getElementById('view_fullname').textContent   = d.fullname;
    document.getElementById('view_email_sub').textContent  = d.email || '—';
    document.getElementById('view_phone').textContent      = d.phone || '—';
    document.getElementById('view_email').textContent      = d.email || '—';
    document.getElementById('view_citizenid').textContent  = d.citizenid || '—';
    document.getElementById('view_birthdate').textContent  = d.birthdate || '—';
    document.getElementById('view_createdat').textContent  = d.createdat || '—';
    openModal('viewModal');
}

// ===== OTP HELPER =====
function generateOtp() {
    return Math.floor(100000 + Math.random() * 900000).toString();
}

// ===== ADD MODAL =====
let addOtpGenerated = "";
let addOtpTimer     = null;
let addOtpExpired   = false;

function setAddOtpStatus(msg, type) {
    var el = document.getElementById('addOtpStatus');
    el.textContent = msg;
    el.className = "otp-status" + (type ? " " + type : "");
}

function startAddTimer(seconds) {
    clearInterval(addOtpTimer);
    var timerEl   = document.getElementById('addOtpTimer');
    var remaining = seconds;

    timerEl.textContent = "Hiệu lực: " + remaining + "s";

    addOtpTimer = setInterval(function () {
        remaining--;
        timerEl.textContent = "Hiệu lực: " + remaining + "s";
        if (remaining <= 0) {
            clearInterval(addOtpTimer);
            addOtpExpired   = true;
            addOtpGenerated = "";
            timerEl.textContent = "Mã OTP đã hết hạn.";
            document.getElementById('btnAddSendOtp').disabled    = false;
            document.getElementById('btnAddSendOtp').textContent = "Gửi lại";
            setAddOtpStatus("", "");
        }
    }, 1000);
}

async function sendAddOtp() {
    var emailInput = document.getElementById('add_email');
    var email      = emailInput.value.trim();

    if (!email || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
        emailInput.focus();
        document.getElementById('addOtpGroup').style.display = 'block';
        setAddOtpStatus("Vui lòng nhập email hợp lệ trước.", "error");
        return;
    }

    var btn         = document.getElementById('btnAddSendOtp');
    btn.disabled    = true;
    btn.textContent = "Đang gửi...";
    setAddOtpStatus("", "");
    clearInterval(addOtpTimer);
    addOtpExpired   = false;
    addOtpGenerated = generateOtp();
    document.getElementById('add_otp_verified').value = "false";

    var nameInput = document.querySelector('#addForm input[name="fullname"]');
    var toName    = nameInput && nameInput.value.trim() ? nameInput.value.trim() : email;

    try {
        await emailjs.send(EMAILJS_SERVICE_ID, EMAILJS_TEMPLATE_ID, {
            to_email: email,
            to_name:  toName,
            otp_code: addOtpGenerated
        });

        document.getElementById('addOtpGroup').style.display  = 'block';
        document.getElementById('add_otp_input').value        = '';
        document.getElementById('add_otp_input').disabled     = false;
        setAddOtpStatus("Đã gửi OTP tới " + email, "success");
        startAddTimer(120);
        btn.textContent = "Gửi lại";

    } catch (err) {
        addOtpGenerated = "";
        btn.disabled    = false;
        btn.textContent = "Gửi OTP";
        document.getElementById('addOtpGroup').style.display = 'block';
        setAddOtpStatus("Gửi thất bại. Kiểm tra lại kết nối.", "error");
    }
}

function verifyAddOtp() {
    var input = document.getElementById('add_otp_input').value.trim();

    if (addOtpExpired || !addOtpGenerated) {
        setAddOtpStatus("Mã OTP đã hết hạn. Vui lòng gửi lại.", "error");
        return false;
    }
    if (input === addOtpGenerated) {
        clearInterval(addOtpTimer);
        document.getElementById('add_otp_verified').value    = "true";
        document.getElementById('addOtpTimer').textContent   = "";
        document.getElementById('add_otp_input').disabled    = true;
        document.getElementById('btnAddSendOtp').disabled    = true;
        setAddOtpStatus("✓ Xác thực thành công!", "success");
        return true;
    } else {
        setAddOtpStatus("Mã OTP không đúng.", "error");
        return false;
    }
}

function resetAddOtp() {
    clearInterval(addOtpTimer);
    addOtpGenerated = "";
    addOtpExpired   = false;
    document.getElementById('addOtpGroup').style.display       = 'none';
    document.getElementById('addOtpTimer').textContent         = '';
    document.getElementById('add_otp_input').value             = '';
    document.getElementById('add_otp_input').disabled          = false;
    document.getElementById('add_otp_verified').value          = 'false';
    document.getElementById('btnAddSendOtp').disabled          = false;
    document.getElementById('btnAddSendOtp').textContent       = 'Gửi OTP';
    setAddOtpStatus('', '');
}

function openAddModal() {
    document.getElementById('addForm').reset();
    resetAddOtp();
    openModal('addModal');
}

document.getElementById('addForm').addEventListener('submit', function (e) {
    var otpGroup = document.getElementById('addOtpGroup');
    if (otpGroup.style.display !== 'none') {
        if (document.getElementById('add_otp_verified').value !== "true") {
            e.preventDefault();
            if (!verifyAddOtp()) {
                setAddOtpStatus("Vui lòng xác thực OTP trước khi lưu.", "error");
            }
        }
    }
});

// ===== EDIT MODAL =====
let editOtpGenerated  = "";
let editOtpTimer      = null;
let editOtpExpired    = false;
let editOriginalEmail = "";

function setEditOtpStatus(msg, type) {
    var el = document.getElementById('editOtpStatus');
    el.textContent = msg;
    el.className = "otp-status" + (type ? " " + type : "");
}

function startEditTimer(seconds) {
    clearInterval(editOtpTimer);
    var timerEl   = document.getElementById('editOtpTimer');
    var remaining = seconds;

    timerEl.textContent = "Hiệu lực: " + remaining + "s";

    editOtpTimer = setInterval(function () {
        remaining--;
        timerEl.textContent = "Hiệu lực: " + remaining + "s";
        if (remaining <= 0) {
            clearInterval(editOtpTimer);
            editOtpExpired   = true;
            editOtpGenerated = "";
            timerEl.textContent = "Mã OTP đã hết hạn.";
            document.getElementById('btnEditSendOtp').disabled    = false;
            document.getElementById('btnEditSendOtp').textContent = "Gửi lại";
            setEditOtpStatus("", "");
        }
    }, 1000);
}

async function sendEditOtp() {
    var emailInput = document.getElementById('edit_email');
    var email      = emailInput.value.trim();

    if (!email || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
        emailInput.focus();
        document.getElementById('editOtpGroup').style.display = 'block';
        setEditOtpStatus("Vui lòng nhập email hợp lệ trước.", "error");
        return;
    }

    var btn         = document.getElementById('btnEditSendOtp');
    btn.disabled    = true;
    btn.textContent = "Đang gửi...";
    setEditOtpStatus("", "");
    clearInterval(editOtpTimer);
    editOtpExpired   = false;
    editOtpGenerated = generateOtp();
    document.getElementById('edit_otp_verified').value = "false";

    var nameInput = document.getElementById('edit_fullname');
    var toName    = nameInput && nameInput.value.trim() ? nameInput.value.trim() : email;

    try {
        await emailjs.send(EMAILJS_SERVICE_ID, EMAILJS_TEMPLATE_ID, {
            to_email: email,
            to_name:  toName,
            otp_code: editOtpGenerated
        });

        document.getElementById('editOtpGroup').style.display  = 'block';
        document.getElementById('edit_otp_input').value        = '';
        document.getElementById('edit_otp_input').disabled     = false;
        setEditOtpStatus("Đã gửi OTP tới " + email, "success");
        startEditTimer(120);
        btn.textContent = "Gửi lại";

    } catch (err) {
        editOtpGenerated = "";
        btn.disabled    = false;
        btn.textContent = "Gửi OTP";
        document.getElementById('editOtpGroup').style.display = 'block';
        setEditOtpStatus("Gửi thất bại. Kiểm tra lại kết nối.", "error");
    }
}

function verifyEditOtp() {
    var input = document.getElementById('edit_otp_input').value.trim();

    if (editOtpExpired || !editOtpGenerated) {
        setEditOtpStatus("Mã OTP đã hết hạn. Vui lòng gửi lại.", "error");
        return false;
    }
    if (input === editOtpGenerated) {
        clearInterval(editOtpTimer);
        document.getElementById('edit_otp_verified').value    = "true";
        document.getElementById('editOtpTimer').textContent   = "";
        document.getElementById('edit_otp_input').disabled    = true;
        document.getElementById('btnEditSendOtp').disabled    = true;
        setEditOtpStatus("✓ Xác thực thành công!", "success");
        return true;
    } else {
        setEditOtpStatus("Mã OTP không đúng.", "error");
        return false;
    }
}

function resetEditOtp() {
    clearInterval(editOtpTimer);
    editOtpGenerated = "";
    editOtpExpired   = false;
    document.getElementById('editOtpGroup').style.display       = 'none';
    document.getElementById('editOtpTimer').textContent         = '';
    document.getElementById('edit_otp_input').value             = '';
    document.getElementById('edit_otp_input').disabled          = false;
    document.getElementById('edit_otp_verified').value          = 'false';
    document.getElementById('btnEditSendOtp').disabled          = false;
    document.getElementById('btnEditSendOtp').textContent       = 'Gửi OTP';
    setEditOtpStatus('', '');
}

function openEditModal(btn) {
    var d = btn.dataset;
    document.getElementById('edit_id').value        = d.id;
    document.getElementById('edit_fullname').value  = d.fullname;
    document.getElementById('edit_email').value     = d.email;
    document.getElementById('edit_phone').value     = d.phone;
    document.getElementById('edit_citizenid').value = d.citizenid;
    document.getElementById('edit_birthdate').value = d.birthdate;

    editOriginalEmail = d.email;
    resetEditOtp();
    openModal('editModal');
}

document.getElementById('editForm').addEventListener('submit', function (e) {
    var currentEmail = document.getElementById('edit_email').value.trim();
    if (currentEmail !== editOriginalEmail) {
        if (document.getElementById('edit_otp_verified').value !== "true") {
            e.preventDefault();
            document.getElementById('editOtpGroup').style.display = 'block';
            if (!verifyEditOtp()) {
                setEditOtpStatus("Vui lòng xác thực OTP cho email mới trước khi lưu.", "error");
            }
        }
    }
});

// ===== DELETE MODAL =====
function openDeleteModal(btn) {
    document.getElementById('delete_id').value             = btn.dataset.id;
    document.getElementById('delete_name').textContent     = btn.dataset.fullname;
    openModal('deleteModal');
}

document.getElementById('add_otp_input').addEventListener('input', function () {
    if (this.value.trim().length === 6) {
        verifyAddOtp();
    }
});

document.getElementById('edit_otp_input').addEventListener('input', function () {
    if (this.value.trim().length === 6) {
        verifyEditOtp();
    }
});