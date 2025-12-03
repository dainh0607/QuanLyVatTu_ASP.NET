document.addEventListener('DOMContentLoaded', function () {
    const navLinks = document.querySelectorAll('.policy-nav .nav-link');
    const policySections = document.querySelectorAll('.policy-section');

    navLinks.forEach(link => {
        link.addEventListener('click', function (e) {
            e.preventDefault();

            const targetId = this.getAttribute('href').substring(1);

            navLinks.forEach(navLink => navLink.classList.remove('active'));
            policySections.forEach(section => section.classList.remove('active'));

            this.classList.add('active');
            document.getElementById(targetId).classList.add('active');

            document.getElementById(targetId).scrollIntoView({
                behavior: 'smooth',
                block: 'start'
            });
        });
    });

    const observerOptions = {
        root: null,
        rootMargin: '0px',
        threshold: 0.3
    };

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const id = entry.target.getAttribute('id');
                const correspondingLink = document.querySelector(`.policy-nav .nav-link[href="#${id}"]`);

                if (correspondingLink) {
                    navLinks.forEach(link => link.classList.remove('active'));
                    policySections.forEach(section => section.classList.remove('active'));

                    correspondingLink.classList.add('active');
                    entry.target.classList.add('active');
                }
            }
        });
    }, observerOptions);

    policySections.forEach(section => {
        observer.observe(section);
    });
});