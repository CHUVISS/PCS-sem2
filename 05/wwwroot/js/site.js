// Auto-dismiss alerts
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.alert-dismissible').forEach(el => {
        setTimeout(() => el.classList.add('fade'), 3000);
        setTimeout(() => el.remove(), 3500);
    });
});
