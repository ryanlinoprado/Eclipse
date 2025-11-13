document.addEventListener('DOMContentLoaded', function () {
    // Category Filtering
    const categoryButtons = document.querySelectorAll('.category-btn');
    const faqCategories = document.querySelectorAll('.faq-category');
    const faqItems = document.querySelectorAll('.faq-item');
    const noResults = document.getElementById('noResults');

    categoryButtons.forEach(button => {
        button.addEventListener('click', function () {
            const category = this.getAttribute('data-category');

            // Update active button
            categoryButtons.forEach(btn => btn.classList.remove('active'));
            this.classList.add('active');

            // Filter categories
            filterFAQs(category, '');
        });
    });

    // Search Functionality
    const searchInput = document.getElementById('faqSearch');
    searchInput.addEventListener('input', function () {
        const searchTerm = this.value.toLowerCase().trim();
        const activeCategory = document.querySelector('.category-btn.active').getAttribute('data-category');

        filterFAQs(activeCategory, searchTerm);
    });

    function filterFAQs(category, searchTerm) {
        let hasVisibleItems = false;

        faqItems.forEach(item => {
            const question = item.querySelector('.faq-question span').textContent.toLowerCase();
            const answer = item.querySelector('.faq-answer').textContent.toLowerCase();
            const itemCategory = item.closest('.faq-category').getAttribute('data-category');

            const matchesSearch = question.includes(searchTerm) || answer.includes(searchTerm);
            const matchesCategory = category === 'all' || itemCategory === category;

            if (matchesSearch && matchesCategory) {
                item.style.display = 'block';
                item.closest('.faq-category').style.display = 'block';
                hasVisibleItems = true;
            } else {
                item.style.display = 'none';

                // Hide category if no visible items
                const categoryElement = item.closest('.faq-category');
                const visibleItemsInCategory = categoryElement.querySelectorAll('.faq-item[style=""]');
                if (visibleItemsInCategory.length === 0) {
                    categoryElement.style.display = 'none';
                }
            }
        });

        // Show/hide no results message
        if (hasVisibleItems) {
            noResults.classList.add('d-none');
        } else {
            noResults.classList.remove('d-none');
        }
    }

    // Auto-expand searched items
    searchInput.addEventListener('keyup', function (e) {
        if (e.key === 'Enter' && this.value.trim()) {
            const firstVisibleItem = document.querySelector('.faq-item[style=""]');
            if (firstVisibleItem) {
                const targetId = firstVisibleItem.querySelector('.faq-question').getAttribute('data-bs-target');
                const collapseElement = document.querySelector(targetId);
                if (collapseElement) {
                    const collapse = new bootstrap.Collapse(collapseElement);
                    collapse.show();
                }
            }
        }
    });

    // Smooth scrolling for category titles
    document.querySelectorAll('.category-title').forEach(title => {
        title.style.cursor = 'pointer';
        title.addEventListener('click', function () {
            this.scrollIntoView({ behavior: 'smooth', block: 'center' });
        });
    });

    // Auto-expand FAQ if URL has hash
    if (window.location.hash) {
        const targetElement = document.querySelector(window.location.hash);
        if (targetElement && targetElement.classList.contains('collapse')) {
            const collapse = new bootstrap.Collapse(targetElement);
            collapse.show();
        }
    }

    // Add click animation to FAQ items
    faqItems.forEach(item => {
        item.addEventListener('click', function (e) {
            if (!e.target.closest('.faq-question')) {
                const question = this.querySelector('.faq-question');
                question.style.transform = 'scale(0.98)';
                setTimeout(() => {
                    question.style.transform = '';
                }, 150);
            }
        });
    });
});