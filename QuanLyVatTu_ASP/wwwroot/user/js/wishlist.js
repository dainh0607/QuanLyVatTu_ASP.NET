document.addEventListener('DOMContentLoaded', function () {
    const removeButtons = document.querySelectorAll('.remove-wishlist');
    const wishlistItems = document.querySelectorAll('.wishlist-item');
    const emptyWishlist = document.querySelector('.empty-wishlist');

    removeButtons.forEach(button => {
        button.addEventListener('click', function () {
            const wishlistItem = this.closest('.wishlist-item');

            wishlistItem.style.opacity = '0';
            wishlistItem.style.transform = 'translateX(-20px)';

            setTimeout(() => {
                wishlistItem.remove();

                if (document.querySelectorAll('.wishlist-item').length === 0) {
                    emptyWishlist.classList.remove('hidden');
                }

                updateWishlistStats();
            }, 300);
        });
    });

    const addToCartButtons = document.querySelectorAll('.add-to-cart');
    addToCartButtons.forEach(button => {
        button.addEventListener('click', function () {
            if (!this.disabled) {
                const productName = this.closest('.wishlist-item').querySelector('.item-name').textContent;

                showMessage(`Đã thêm "${productName}" vào giỏ hàng!`, 'success');

            }
        });
    });

    const viewDetailsButtons = document.querySelectorAll('.view-details');
    viewDetailsButtons.forEach(button => {
        button.addEventListener('click', function () {
            window.location.href = '/product-detail';
        });
    });

    function updateWishlistStats() {
        const itemCount = document.querySelectorAll('.wishlist-item').length;
        const statValue = document.querySelector('.stat-item:first-child .stat-value');

        if (statValue) {
            statValue.textContent = itemCount;
        }
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