document.addEventListener('DOMContentLoaded', function () {
    const quantityInputs = document.querySelectorAll('.quantity-input');
    const minusBtns = document.querySelectorAll('.quantity-btn.minus');
    const plusBtns = document.querySelectorAll('.quantity-btn.plus');

    minusBtns.forEach((btn, index) => {
        btn.addEventListener('click', function () {
            let value = parseInt(quantityInputs[index].value);
            if (value > 1) {
                quantityInputs[index].value = value - 1;
                updateCartItem(index);
            }
        });
    });

    plusBtns.forEach((btn, index) => {
        btn.addEventListener('click', function () {
            let value = parseInt(quantityInputs[index].value);
            quantityInputs[index].value = value + 1;
            updateCartItem(index);
        });
    });

    const removeBtns = document.querySelectorAll('.item-remove');
    removeBtns.forEach(btn => {
        btn.addEventListener('click', function () {
            const cartItem = this.closest('.cart-item');
            cartItem.style.opacity = '0';
            setTimeout(() => {
                cartItem.remove();
                updateCartSummary();
            }, 300);
        });
    });

    function updateCartItem(index) {
        console.log('Updated quantity for item', index);
        updateCartSummary();
    }

    function updateCartSummary() {
        console.log('Updated cart summary');
    }
});