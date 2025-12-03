document.addEventListener('DOMContentLoaded', function () {
    const citySelect = document.getElementById('city');
    const districtSelect = document.getElementById('district');

    const districts = {
        hanoi: ['Ba Đình', 'Hoàn Kiếm', 'Hai Bà Trưng', 'Đống Đa', 'Cầu Giấy', 'Thanh Xuân'],
        hcm: ['Quận 1', 'Quận 2', 'Quận 3', 'Quận 4', 'Quận 5', 'Quận 6'],
        danang: ['Hải Châu', 'Thanh Khê', 'Sơn Trà', 'Ngũ Hành Sơn', 'Liên Chiểu']
    };

    citySelect.addEventListener('change', function () {
        const selectedCity = this.value;
        districtSelect.innerHTML = '<option value="">Chọn quận/huyện</option>';

        if (selectedCity && districts[selectedCity]) {
            districts[selectedCity].forEach(district => {
                const option = document.createElement('option');
                option.value = district;
                option.textContent = district;
                districtSelect.appendChild(option);
            });
        }
    });

    const couponBtn = document.querySelector('.coupon-btn');
    const couponInput = document.querySelector('.coupon-input');

    couponBtn.addEventListener('click', function () {
        const couponCode = couponInput.value.trim();

        if (couponCode) {
            if (couponCode === 'MATERIAL10') {
                showMessage('Áp dụng mã giảm giá thành công!', 'success');
            } else {
                showMessage('Mã giảm giá không hợp lệ!', 'error');
            }
        } else {
            showMessage('Vui lòng nhập mã giảm giá!', 'error');
        }
    });

    const checkoutForm = document.getElementById('checkoutForm');
    checkoutForm.addEventListener('submit', function (e) {
        e.preventDefault();

        if (validateForm()) {
            const submitBtn = checkoutForm.querySelector('.checkout-btn');
            const originalText = submitBtn.textContent;
            submitBtn.textContent = 'Đang xử lý...';
            submitBtn.disabled = true;

            setTimeout(() => {
                showMessage('Đặt hàng thành công! Cảm ơn bạn đã mua sắm tại MaterialPro.', 'success');

                submitBtn.textContent = originalText;
                submitBtn.disabled = false;
            }, 2000);
        }
    });

    function validateForm() {
        const requiredFields = checkoutForm.querySelectorAll('[required]');
        let isValid = true;

        requiredFields.forEach(field => {
            if (!field.value.trim()) {
                isValid = false;
                field.style.borderColor = 'var(--accent-red)';

                field.addEventListener('input', function () {
                    this.style.borderColor = 'var(--gray-light)';
                }, { once: true });
            }
        });

        if (!isValid) {
            showMessage('Vui lòng điền đầy đủ thông tin bắt buộc!', 'error');
        }

        return isValid;
    }

    function showMessage(message, type) {
        const toast = document.createElement('div');
        toast.className = `toast-message ${type}`;
        toast.textContent = message;

        toast.style.cssText = `
                position: fixed;
                top: 100px;
                right: 20px;
                background: ${type === 'success' ? 'var(--accent-green)' : 'var(--accent-red)'};
                color: white;
                padding: var(--spacing-md) var(--spacing-lg);
                border-radius: var(--border-radius-md);
                box-shadow: var(--shadow-lg);
                z-index: 10000;
                transform: translateX(100%);
                transition: transform 0.3s;
            `;

        document.body.appendChild(toast);

        setTimeout(() => {
            toast.style.transform = 'translateX(0)';
        }, 100);

        setTimeout(() => {
            toast.style.transform = 'translateX(100%)';
            setTimeout(() => {
                document.body.removeChild(toast);
            }, 300);
        }, 3000);
    }
});