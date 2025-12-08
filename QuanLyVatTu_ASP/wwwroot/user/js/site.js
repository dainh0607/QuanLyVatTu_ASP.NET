// site.js - Enhanced with smooth interactions

document.addEventListener('DOMContentLoaded', function () {
    // =====================
    // Global Variables
    // =====================
    const body = document.body;
    const isMobile = window.innerWidth <= 992;

    // =====================
    // Mobile Menu Toggle
    // =====================
    const mobileMenuToggle = document.getElementById('mobileMenuToggle');
    const mainNav = document.getElementById('mainNav');

    if (mobileMenuToggle && mainNav) {
        mobileMenuToggle.addEventListener('click', function () {
            mainNav.classList.toggle('active');
            body.style.overflow = mainNav.classList.contains('active') ? 'hidden' : '';
            mobileMenuToggle.innerHTML = mainNav.classList.contains('active')
                ? '<i class="fas fa-times"></i>'
                : '<i class="fas fa-bars"></i>';
        });

        // Close menu when clicking outside
        document.addEventListener('click', function (event) {
            if (mainNav.classList.contains('active') &&
                !mainNav.contains(event.target) &&
                !mobileMenuToggle.contains(event.target)) {
                mainNav.classList.remove('active');
                body.style.overflow = '';
                mobileMenuToggle.innerHTML = '<i class="fas fa-bars"></i>';
            }
        });

        // Close menu on resize if it's open and window becomes large
        window.addEventListener('resize', function () {
            if (window.innerWidth > 992 && mainNav.classList.contains('active')) {
                mainNav.classList.remove('active');
                body.style.overflow = '';
                mobileMenuToggle.innerHTML = '<i class="fas fa-bars"></i>';
            }
        });
    }

    // =====================
    // Sticky Header
    // =====================
    const mainHeader = document.querySelector('.main-header');
    let lastScrollTop = 0;

    window.addEventListener('scroll', function () {
        const scrollTop = window.pageYOffset || document.documentElement.scrollTop;

        // Add/remove scrolled class
        if (scrollTop > 50) {
            mainHeader.classList.add('scrolled');
        } else {
            mainHeader.classList.remove('scrolled');
        }

        // Hide/show header on scroll (mobile only)
        if (isMobile) {
            if (scrollTop > lastScrollTop && scrollTop > 100) {
                // Scrolling down
                mainHeader.style.transform = 'translateY(-100%)';
            } else {
                // Scrolling up
                mainHeader.style.transform = 'translateY(0)';
            }
        }

        lastScrollTop = scrollTop;
    });

    // =====================
    // Search Functionality
    // =====================
    const searchInput = document.getElementById('searchInput');
    const searchForm = document.querySelector('.search-form');
    const searchSuggestions = document.getElementById('searchSuggestions');

    if (searchInput && searchSuggestions) {
        let searchTimeout;

        searchInput.addEventListener('focus', function () {
            if (this.value.trim().length > 2) {
                searchSuggestions.classList.add('active');
            }
        });

        searchInput.addEventListener('input', function () {
            clearTimeout(searchTimeout);
            const query = this.value.trim();

            if (query.length > 2) {
                searchTimeout = setTimeout(() => {
                    fetchSearchSuggestions(query);
                }, 300);
            } else {
                searchSuggestions.classList.remove('active');
            }
        });

        // Close suggestions when clicking outside
        document.addEventListener('click', function (event) {
            if (!searchForm.contains(event.target)) {
                searchSuggestions.classList.remove('active');
            }
        });

        // Search form submission
        if (searchForm) {
            searchForm.addEventListener('submit', function (e) {
                const query = searchInput.value.trim();
                if (!query) {
                    e.preventDefault();
                    searchInput.focus();
                    searchInput.style.borderColor = 'var(--danger)';
                    setTimeout(() => {
                        searchInput.style.borderColor = '';
                    }, 1000);
                }
            });
        }
    }

    // =====================
    // User Dropdown
    // =====================
    const userDropdowns = document.querySelectorAll('.user-dropdown');

    userDropdowns.forEach(dropdown => {
        const toggle = dropdown.querySelector('.user-info');
        const menu = dropdown.querySelector('.user-menu');

        if (toggle && menu) {
            toggle.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();

                // Close other open dropdowns
                document.querySelectorAll('.user-menu.show').forEach(otherMenu => {
                    if (otherMenu !== menu) {
                        otherMenu.classList.remove('show');
                    }
                });

                menu.classList.toggle('show');
            });

            // Close dropdown when clicking outside
            document.addEventListener('click', function (event) {
                if (!dropdown.contains(event.target)) {
                    menu.classList.remove('show');
                }
            });
        }
    });

    // =====================
    // Cart Functionality
    // =====================
    function updateCartCount(count) {
        const cartBadges = document.querySelectorAll('.cart-count, .quick-access-bar .badge');
        cartBadges.forEach(badge => {
            badge.textContent = count;
            badge.style.animation = 'none';
            setTimeout(() => {
                badge.style.animation = 'pulse 0.5s ease';
            }, 10);
        });

        const cartIcon = document.querySelector('.cart-icon');
        if (count > 0) {
            cartIcon.classList.add('has-items');
        } else {
            cartIcon.classList.remove('has-items');
        }

        // Save to localStorage
        localStorage.setItem('cartCount', count);
    }

    // Initialize cart count
    const savedCartCount = localStorage.getItem('cartCount') || 0;
    updateCartCount(parseInt(savedCartCount));

    // Simulate adding to cart
    document.addEventListener('click', function (e) {
        if (e.target.closest('.add-to-cart')) {
            e.preventDefault();
            const currentCount = parseInt(document.querySelector('.cart-count').textContent || 0);
            updateCartCount(currentCount + 1);

            // Show success message
            showNotification('Đã thêm sản phẩm vào giỏ hàng!', 'success');
        }
    });

    // =====================
    // Quick Access Bar
    // =====================
    const quickAccessBar = document.querySelector('.quick-access-bar');
    if (quickAccessBar && isMobile) {
        // Highlight active quick item based on current page
        const currentPath = window.location.pathname;
        const quickItems = quickAccessBar.querySelectorAll('.quick-item');

        quickItems.forEach(item => {
            const href = item.getAttribute('href');
            if (href === currentPath || (href === '/' && currentPath === '/')) {
                item.classList.add('active');
            }
        });

        // Hide/show bar on scroll
        let lastScroll = 0;
        window.addEventListener('scroll', function () {
            const currentScroll = window.pageYOffset;

            if (currentScroll > lastScroll && currentScroll > 100) {
                // Scrolling down
                quickAccessBar.style.transform = 'translateY(100%)';
            } else {
                // Scrolling up
                quickAccessBar.style.transform = 'translateY(0)';
            }

            lastScroll = currentScroll;
        });
    }

    // =====================
    // Back to Top Button
    // =====================
    const backToTop = document.createElement('a');
    backToTop.href = '#';
    backToTop.className = 'back-to-top';
    backToTop.innerHTML = '<i class="fas fa-chevron-up"></i>';
    backToTop.setAttribute('aria-label', 'Lên đầu trang');
    document.body.appendChild(backToTop);

    window.addEventListener('scroll', function () {
        if (window.pageYOffset > 300) {
            backToTop.classList.add('visible');
        } else {
            backToTop.classList.remove('visible');
        }
    });

    backToTop.addEventListener('click', function (e) {
        e.preventDefault();
        window.scrollTo({
            top: 0,
            behavior: 'smooth'
        });
    });

    // =====================
    // Loading Overlay
    // =====================
    const loadingOverlay = document.createElement('div');
    loadingOverlay.className = 'loading-overlay';
    loadingOverlay.innerHTML = '<div class="spinner"></div>';
    document.body.appendChild(loadingOverlay);

    function showLoading() {
        loadingOverlay.classList.add('active');
    }

    function hideLoading() {
        loadingOverlay.classList.remove('active');
    }

    // =====================
    // Notification System
    // =====================
    function showNotification(message, type = 'info') {
        const notification = document.createElement('div');
        notification.className = `notification notification-${type}`;
        notification.innerHTML = `
            <div class="notification-content">
                <i class="fas fa-${type === 'success' ? 'check-circle' : type === 'error' ? 'exclamation-circle' : 'info-circle'}"></i>
                <span>${message}</span>
            </div>
            <button class="notification-close">
                <i class="fas fa-times"></i>
            </button>
        `;

        document.body.appendChild(notification);

        // Add styles for notification
        if (!document.querySelector('#notification-styles')) {
            const styles = document.createElement('style');
            styles.id = 'notification-styles';
            styles.textContent = `
                .notification {
                    position: fixed;
                    top: 20px;
                    right: 20px;
                    background: var(--white);
                    border-radius: var(--radius-md);
                    box-shadow: var(--shadow-lg);
                    padding: 15px 20px;
                    display: flex;
                    align-items: center;
                    justify-content: space-between;
                    gap: 15px;
                    min-width: 300px;
                    max-width: 400px;
                    z-index: 9999;
                    transform: translateX(400px);
                    transition: transform 0.3s ease;
                }
                .notification.notification-success {
                    border-left: 4px solid var(--success);
                }
                .notification.notification-error {
                    border-left: 4px solid var(--danger);
                }
                .notification.notification-info {
                    border-left: 4px solid var(--info);
                }
                .notification.show {
                    transform: translateX(0);
                }
                .notification-content {
                    display: flex;
                    align-items: center;
                    gap: 10px;
                }
                .notification-content i {
                    font-size: 1.2rem;
                }
                .notification-success .notification-content i {
                    color: var(--success);
                }
                .notification-error .notification-content i {
                    color: var(--danger);
                }
                .notification-info .notification-content i {
                    color: var(--info);
                }
                .notification-close {
                    background: none;
                    border: none;
                    color: var(--gray);
                    cursor: pointer;
                    padding: 5px;
                }
                .notification-close:hover {
                    color: var(--dark);
                }
            `;
            document.head.appendChild(styles);
        }

        // Show notification
        setTimeout(() => {
            notification.classList.add('show');
        }, 10);

        // Auto remove after 5 seconds
        const autoRemove = setTimeout(() => {
            notification.classList.remove('show');
            setTimeout(() => {
                if (notification.parentNode) {
                    notification.parentNode.removeChild(notification);
                }
            }, 300);
        }, 5000);

        // Close button
        const closeBtn = notification.querySelector('.notification-close');
        closeBtn.addEventListener('click', function () {
            clearTimeout(autoRemove);
            notification.classList.remove('show');
            setTimeout(() => {
                if (notification.parentNode) {
                    notification.parentNode.removeChild(notification);
                }
            }, 300);
        });
    }

    // =====================
    // Product Image Zoom
    // =====================
    document.querySelectorAll('.product-image').forEach(image => {
        image.addEventListener('mouseenter', function () {
            this.style.transform = 'scale(1.05)';
        });

        image.addEventListener('mouseleave', function () {
            this.style.transform = 'scale(1)';
        });
    });

    // =====================
    // Form Enhancements
    // =====================
    document.querySelectorAll('input, textarea, select').forEach(input => {
        // Add focus effects
        input.addEventListener('focus', function () {
            this.parentElement.classList.add('focused');
        });

        input.addEventListener('blur', function () {
            if (!this.value) {
                this.parentElement.classList.remove('focused');
            }
        });

        // Add validation styles
        input.addEventListener('invalid', function (e) {
            e.preventDefault();
            this.classList.add('invalid');
            showNotification('Vui lòng kiểm tra lại thông tin đã nhập', 'error');
        });

        input.addEventListener('input', function () {
            this.classList.remove('invalid');
        });
    });

    // =====================
    // Helper Functions
    // =====================
    function fetchSearchSuggestions(query) {
        showLoading();

        // Simulate API call
        setTimeout(() => {
            const suggestions = [
                'Xi măng Hà Tiên',
                'Sắt thép Việt Nhật',
                'Gạch ống 4 lỗ',
                'Dây điện Cadivi',
                'Công tắc ổ cắm Sino',
                'Đèn LED downlight',
                'Máy khoan Bosch',
                'Sơn Dulux',
                'Ống nước Bình Minh',
                'Thiết bị vệ sinh INAX'
            ].filter(item =>
                item.toLowerCase().includes(query.toLowerCase())
            );

            if (suggestions.length > 0) {
                searchSuggestions.innerHTML = suggestions
                    .map(item => `<a href="/Search?q=${encodeURIComponent(item)}" class="dropdown-item">${item}</a>`)
                    .join('');
            } else {
                searchSuggestions.innerHTML = '<div class="dropdown-item text-muted">Không tìm thấy kết quả</div>';
            }

            searchSuggestions.classList.add('active');
            hideLoading();
        }, 500);
    }

    // =====================
    // Initialize
    // =====================
    console.log('Site initialized successfully!');
});

// Add CSS animations
const style = document.createElement('style');
style.textContent = `
    @keyframes pulse {
        0% { transform: scale(1); }
        50% { transform: scale(1.2); }
        100% { transform: scale(1); }
    }
    
    @keyframes slideInUp {
        from {
            transform: translateY(20px);
            opacity: 0;
        }
        to {
            transform: translateY(0);
            opacity: 1;
        }
    }
    
    @keyframes fadeIn {
        from {
            opacity: 0;
        }
        to {
            opacity: 1;
        }
    }
`;
document.head.appendChild(style);