// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener('DOMContentLoaded', function () {

    // ── CATEGORY DROPDOWN ──────────────────────────
    const catToggle = document.getElementById('categoryDropdownToggle');
    const catMenu = document.getElementById('categoryDropdownMenu');

    if (catToggle && catMenu) {
        catToggle.addEventListener('show.bs.dropdown', function () {
            if (catMenu.dataset.loaded === 'true') return;

            fetch('/Customer/Home/GetCategories')
                .then(r => r.json())
                .then(data => {
                    if (!data.length) {
                        catMenu.innerHTML = '<li><span class="dropdown-item text-muted">No categories</span></li>';
                        return;
                    }
                    catMenu.innerHTML = data.map(c =>
                        `<li><a class="dropdown-item" href="/Customer/Home/Index?categoryId=${c.id}">${c.name}</a></li>`
                    ).join('');
                    catMenu.dataset.loaded = 'true';
                })
                .catch(() => {
                    catMenu.innerHTML = '<li><span class="dropdown-item text-danger">Failed to load</span></li>';
                });
        });
    }

    // ── LIVE SEARCH ────────────────────────────────
    const searchInput = document.getElementById('searchInput');
    const searchDropdown = document.getElementById('searchDropdown');
    const searchResults = document.getElementById('searchResults');
    let searchTimer;

    if (searchInput) {
        searchInput.addEventListener('input', function () {
            clearTimeout(searchTimer);
            const query = this.value.trim();

            if (query.length < 2) {
                searchDropdown.style.display = 'none';
                return;
            }

            searchTimer = setTimeout(() => {
                fetch(`/Customer/Home/SearchSuggestions?query=${encodeURIComponent(query)}`)
                    .then(r => r.json())
                    .then(data => {
                        if (!data.length) {
                            searchResults.innerHTML = `<div class="search-no-result">No results for "${query}"</div>`;
                        } else {
                            searchResults.innerHTML = data.map(p => `
                                <a href="/Customer/Home/Details/${p.id}" class="search-item">
                                    <img src="${p.imageUrl || '/images/no-image.png'}" alt="${p.title}"
                                         onerror="this.src='/images/no-image.png'" />
                                    <div>
                                        <div class="s-title">${p.title}</div>
                                        <div class="s-author">${p.author}</div>
                                    </div>
                                </a>`).join('');
                        }
                        searchResults.innerHTML += `
                            <a class="search-view-all"
                               href="/Customer/Home/Search?query=${encodeURIComponent(query)}">
                               View all results for "${query}" →
                            </a>`;
                        searchDropdown.style.display = 'block';
                    })
                    .catch(() => {
                        searchResults.innerHTML = `<div class="search-no-result">Something went wrong</div>`;
                        searchDropdown.style.display = 'block';
                    });
            }, 300);
        });

        // Close on outside click
        document.addEventListener('click', function (e) {
            if (!document.getElementById('searchWrapper').contains(e.target)) {
                searchDropdown.style.display = 'none';
            }
        });

        // Enter key → go to search page
        searchInput.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' && this.value.trim()) {
                window.location.href = `/Customer/Home/Search?query=${encodeURIComponent(this.value.trim())}`;
            }
        });
    }

});