const EMAILJS_PUBLIC_KEY  = "c2mCh8VYFr_l9mdmy";
const EMAILJS_SERVICE_ID  = "service_cjsueiu";
const EMAILJS_TEMPLATE_ID = "template_sb53noo";

emailjs.init(EMAILJS_PUBLIC_KEY);

let addOtpCode = "";
let addEmailVerified = false;

let editOtpCode = "";
let editEmailVerified = false;
let editOriginalEmail = "";

function generateOtp() {
    return Math.floor(100000 + Math.random() * 900000).toString();
}

async function sendOtp() {
    const email = document.getElementById("add_email").value.trim();
    if (!email || !email.includes("@")) {
        alert("Vui lòng nhập email hợp lệ trước khi gửi OTP.");
        return;
    }
    const btn = document.getElementById("btnSendOtp");
    btn.disabled = true;
    btn.textContent = "Đang gửi...";
    addOtpCode = generateOtp();
    addEmailVerified = false;
    try {
        await emailjs.send(EMAILJS_SERVICE_ID, EMAILJS_TEMPLATE_ID, {
            to_email: email,
            otp_code: addOtpCode,
            to_name: document.getElementById("add_fullname").value || "Nhân viên"
        });
        document.getElementById("otpGroup").style.display = "block";
        document.getElementById("verifiedGroup").style.display = "none";
        document.getElementById("otpHint").textContent = "✔ Mã OTP đã gửi tới " + email;
        document.getElementById("otpHint").style.color = "#16a34a";
        startOtpTimer(btn, "add");
    } catch (err) {
        console.error(err);
        alert("Gửi OTP thất bại. Kiểm tra lại cấu hình EmailJS.");
        btn.disabled = false;
        btn.textContent = "Gửi OTP";
    }
}

async function sendEditOtp() {
    const email = document.getElementById("edit_email").value.trim();
    if (!email || !email.includes("@")) {
        alert("Vui lòng nhập email hợp lệ trước khi gửi OTP.");
        return;
    }
    const btn = document.getElementById("btnEditSendOtp");
    btn.disabled = true;
    btn.textContent = "Đang gửi...";
    editOtpCode = generateOtp();
    editEmailVerified = false;
    try {
        await emailjs.send(EMAILJS_SERVICE_ID, EMAILJS_TEMPLATE_ID, {
            to_email: email,
            otp_code: editOtpCode,
            to_name: document.getElementById("edit_fullname").value || "Nhân viên"
        });
        document.getElementById("editOtpGroup").style.display = "block";
        document.getElementById("editVerifiedGroup").style.display = "none";
        document.getElementById("editOtpHint").textContent = "✔ Mã OTP đã gửi tới " + email;
        document.getElementById("editOtpHint").style.color = "#16a34a";
        startOtpTimer(btn, "edit");
    } catch (err) {
        console.error(err);
        alert("Gửi OTP thất bại.");
        btn.disabled = false;
        btn.textContent = "Gửi OTP";
    }
}

function startOtpTimer(btn, mode) {
    let seconds = 120;
    const interval = setInterval(() => {
        seconds--;
        btn.textContent = "Gửi lại (" + seconds + "s)";
        if (seconds <= 0) {
            clearInterval(interval);
            btn.disabled = false;
            btn.textContent = "Gửi lại OTP";
            if (mode === "add") addOtpCode = "";
            else editOtpCode = "";
        }
    }, 1000);
}

document.addEventListener("DOMContentLoaded", function () {
    document.getElementById("add_otp").addEventListener("input", function () {
        if (this.value.trim() === addOtpCode && addOtpCode !== "") {
            addEmailVerified = true;
            document.getElementById("otpGroup").style.display = "none";
            document.getElementById("verifiedGroup").style.display = "block";
        }
    });
    document.getElementById("edit_otp").addEventListener("input", function () {
        if (this.value.trim() === editOtpCode && editOtpCode !== "") {
            editEmailVerified = true;
            document.getElementById("editOtpGroup").style.display = "none";
            document.getElementById("editVerifiedGroup").style.display = "block";
        }
    });
    document.querySelectorAll(".modal-overlay").forEach(function (overlay) {
        overlay.addEventListener("click", function (e) {
            if (e.target === overlay) overlay.classList.remove("active");
        });
    });
});

function openAddModal() {
    addOtpCode = ""; addEmailVerified = false;
    document.getElementById("add_fullname").value  = "";
    document.getElementById("add_email").value     = "";
    document.getElementById("add_birthdate").value = "";
    document.getElementById("add_citizenid").value = "";
    document.getElementById("add_otp").value       = "";
    document.getElementById("otpGroup").style.display     = "none";
    document.getElementById("verifiedGroup").style.display = "none";
    document.getElementById("otpHint").textContent = "";
    document.getElementById("btnSendOtp").disabled    = false;
    document.getElementById("btnSendOtp").textContent = "Gửi OTP";
    document.getElementById("addModal").classList.add("active");
}

function openEditModal(id, fullname, email, birthdate, citizenid) {
    editOriginalEmail = email;
    editEmailVerified = false;
    editOtpCode = "";
    document.getElementById("edit_id").value         = id;
    document.getElementById("edit_fullname").value   = fullname;
    document.getElementById("edit_email").value      = email;
    document.getElementById("edit_birthdate").value  = birthdate;
    document.getElementById("edit_citizenid").value  = citizenid;
    document.getElementById("edit_otp").value        = "";
    document.getElementById("editOtpGroup").style.display     = "none";
    document.getElementById("editVerifiedGroup").style.display = "none";
    document.getElementById("editOtpHint").textContent = "";
    document.getElementById("btnEditSendOtp").disabled    = false;
    document.getElementById("btnEditSendOtp").textContent = "Gửi OTP";
    document.getElementById("editModal").classList.add("active");
}

function openDeleteModal(id, name, email) {
    document.getElementById("delete_id").value = id;
    document.getElementById("delete_name").textContent = name + " (" + email + ")";
    document.getElementById("deleteModal").classList.add("active");
}

function closeModal(id) {
    document.getElementById(id).classList.remove("active");
}

function submitAdd() {
    const fullname   = document.getElementById("add_fullname").value.trim();
    const email      = document.getElementById("add_email").value.trim();
    const birthdate  = document.getElementById("add_birthdate").value;
    const citizenid  = document.getElementById("add_citizenid").value.trim();

    if (!fullname || !email) {
        alert("Vui lòng điền đầy đủ họ tên và email.");
        return;
    }
    if (!addEmailVerified) {
        alert("Vui lòng xác thực email bằng mã OTP trước khi lưu.");
        return;
    }
    const form = createHiddenForm("/Staff/Index?handler=Add", {
        fullname, email, birthdate, citizenid
    });
    form.submit();
}

function submitEdit() {
    const id        = document.getElementById("edit_id").value;
    const fullname  = document.getElementById("edit_fullname").value.trim();
    const email     = document.getElementById("edit_email").value.trim();
    const birthdate = document.getElementById("edit_birthdate").value;
    const citizenid = document.getElementById("edit_citizenid").value.trim();

    if (!fullname || !email) {
        alert("Vui lòng điền đầy đủ họ tên và email.");
        return;
    }
    if (email !== editOriginalEmail && !editEmailVerified) {
        alert("Email đã thay đổi. Vui lòng xác thực OTP cho email mới.");
        return;
    }
    const form = createHiddenForm("/Staff/Index?handler=Edit", {
        id, fullname, email, birthdate, citizenid
    });
    form.submit();
}

function submitDelete() {
    const id = document.getElementById("delete_id").value;
    const form = createHiddenForm("/Staff/Index?handler=Delete", { id });
    form.submit();
}

function createHiddenForm(action, fields) {
    const form = document.createElement("form");
    form.method = "POST";
    form.action = action;
    const token = document.querySelector('input[name="__RequestVerificationToken"]');
    if (token) {
        const t = document.createElement("input");
        t.type = "hidden";
        t.name = "__RequestVerificationToken";
        t.value = token.value;
        form.appendChild(t);
    }
    for (const [name, value] of Object.entries(fields)) {
        const input = document.createElement("input");
        input.type  = "hidden";
        input.name  = name;
        input.value = value || "";
        form.appendChild(input);
    }
    document.body.appendChild(form);
    return form;
}
