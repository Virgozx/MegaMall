// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener('DOMContentLoaded', function() {
    // Location Data (Mock)
    const locationData = {
        "Th·i NguyÍn": {
            "TP Th·i NguyÍn": ["Phu?ng Trung Vuong", "Phu?ng Ho‡ng Van Th?", "Phu?ng Phan –Ïnh Ph˘ng", "Phu?ng Quang Trung"],
            "TP SÙng CÙng": ["Phu?ng M? ChË", "Phu?ng Th?ng L?i", "Phu?ng C?i –an"],
            "Huy?n –?i T?": ["Th? tr?n H˘ng Son", "X„ H‡ Thu?ng", "X„ T‚n Th·i"]
        },
        "H‡ N?i": {
            "Qu?n Ba –Ïnh": ["Phu?ng Ph˙c X·", "Phu?ng Tr˙c B?ch", "Phu?ng Vinh Ph˙c"],
            "Qu?n Ho‡n Ki?m": ["Phu?ng Ph˙c T‚n", "Phu?ng –?ng Xu‚n", "Phu?ng H‡ng M„"],
            "Qu?n C?u Gi?y": ["Phu?ng D?ch V?ng", "Phu?ng Mai D?ch", "Phu?ng Nghia –Ù"]
        },
        "Tp H? ChÌ Minh": {
            "Qu?n 1": ["Phu?ng B?n NghÈ", "Phu?ng B?n Th‡nh", "Phu?ng CÙ Giang"],
            "Qu?n 3": ["Phu?ng Vı Th? S·u", "Phu?ng 1", "Phu?ng 2"],
            "TP Th? –?c": ["Phu?ng Th?o –i?n", "Phu?ng An Ph˙", "Phu?ng BÏnh Chi?u"]
        },
        "–‡ N?ng": {
            "Qu?n H?i Ch‚u": ["Phu?ng H?i Ch‚u 1", "Phu?ng H?i Ch‚u 2"],
            "Qu?n Thanh KhÍ": ["Phu?ng Thanh KhÍ T‚y", "Phu?ng Thanh KhÍ –Ùng"]
        }
    };

    // State
    let currentProvince = "Th·i NguyÍn";
    let currentDistrict = "";
    let currentWard = "";
    let selectionMode = ""; // 'province', 'district', 'ward'

    // Elements
    const mainView = document.getElementById('location-main-view');
    const listView = document.getElementById('location-list-view');
    const listTitle = document.getElementById('location-list-title');
    const listItems = document.getElementById('location-list-items');
    
    const btnProvince = document.getElementById('btn-select-province');
    const btnDistrict = document.getElementById('btn-select-district');
    const btnWard = document.getElementById('btn-select-ward');
    
    const lblProvince = document.getElementById('lbl-province');
    const lblDistrict = document.getElementById('lbl-district');
    const lblDistrictTitle = document.getElementById('lbl-district-title');
    const lblWard = document.getElementById('lbl-ward');
    const lblWardTitle = document.getElementById('lbl-ward-title');

    const btnBack = document.getElementById('btn-back-location');
    const btnClear = document.getElementById('btn-location-clear');
    const btnApply = document.getElementById('btn-location-apply');
    const currentLocationLabel = document.getElementById('currentLocationLabel');

    if (!mainView) return; // Exit if elements not found (e.g. different layout)

    // Helper: Switch View
    function showList(mode, title, items) {
        selectionMode = mode;
        listTitle.innerText = title;
        listItems.innerHTML = '';
        
        items.forEach(item => {
            const btn = document.createElement('button');
            btn.className = 'list-group-item list-group-item-action px-3 py-2 border-0';
            btn.innerText = item;
            btn.onclick = () => handleSelection(item);
            listItems.appendChild(btn);
        });

        mainView.classList.add('d-none');
        listView.classList.remove('d-none');
    }

    function showMain() {
        listView.classList.add('d-none');
        mainView.classList.remove('d-none');
    }

    // Helper: Update UI
    function updateUI() {
        // Province
        if (currentProvince) {
            lblProvince.innerText = currentProvince;
            lblProvince.classList.remove('d-none');
            // btnProvince.querySelector('.small.fw-bold').classList.add('text-muted'); 
        } else {
            lblProvince.innerText = "";
            lblProvince.classList.add('d-none');
        }

        // District
        if (currentDistrict) {
            lblDistrict.innerText = currentDistrict;
            lblDistrict.classList.remove('d-none');
            lblDistrictTitle.classList.add('fw-bold');
            lblDistrictTitle.classList.remove('text-muted');
            lblDistrictTitle.innerText = "Qu?n/Huy?n";
        } else {
            lblDistrict.innerText = "";
            lblDistrict.classList.add('d-none');
            lblDistrictTitle.classList.remove('fw-bold');
            lblDistrictTitle.classList.add('text-muted');
            lblDistrictTitle.innerText = "Ch?n qu?n huy?n";
        }

        // Ward
        if (currentWard) {
            lblWard.innerText = currentWard;
            lblWard.classList.remove('d-none');
            lblWardTitle.classList.add('fw-bold');
            lblWardTitle.classList.remove('text-muted');
            lblWardTitle.innerText = "Phu?ng/X„";
        } else {
            lblWard.innerText = "";
            lblWard.classList.add('d-none');
            lblWardTitle.classList.remove('fw-bold');
            lblWardTitle.classList.add('text-muted');
            lblWardTitle.innerText = "Ch?n phu?ng x„";
        }

        // Enable/Disable buttons
        btnDistrict.disabled = !currentProvince;
        btnWard.disabled = !currentDistrict;
    }

    // Handlers
    function handleSelection(value) {
        if (selectionMode === 'province') {
            currentProvince = value;
            currentDistrict = "";
            currentWard = "";
        } else if (selectionMode === 'district') {
            currentDistrict = value;
            currentWard = "";
        } else if (selectionMode === 'ward') {
            currentWard = value;
        }
        updateUI();
        showMain();
    }

    // Event Listeners
    btnProvince.addEventListener('click', (e) => {
        e.stopPropagation(); // Prevent dropdown close
        const provinces = Object.keys(locationData);
        showList('province', 'Ch?n T?nh/Th‡nh', provinces);
    });

    btnDistrict.addEventListener('click', (e) => {
        e.stopPropagation();
        if (!currentProvince) return;
        const districts = Object.keys(locationData[currentProvince] || {});
        showList('district', 'Ch?n Qu?n/Huy?n', districts);
    });

    btnWard.addEventListener('click', (e) => {
        e.stopPropagation();
        if (!currentDistrict) return;
        const wards = locationData[currentProvince][currentDistrict] || [];
        showList('ward', 'Ch?n Phu?ng/X„', wards);
    });

    btnBack.addEventListener('click', (e) => {
        e.stopPropagation();
        showMain();
    });

    // Prevent dropdown closing when clicking inside list view
    listView.addEventListener('click', (e) => e.stopPropagation());
    mainView.addEventListener('click', (e) => e.stopPropagation());

    // Quick Tags
    document.querySelectorAll('.quick-loc-btn').forEach(btn => {
        btn.addEventListener('click', (e) => {
            e.stopPropagation();
            currentProvince = btn.dataset.loc;
            currentDistrict = "";
            currentWard = "";
            updateUI();
        });
    });

    // Clear
    btnClear.addEventListener('click', (e) => {
        e.stopPropagation();
        currentProvince = "";
        currentDistrict = "";
        currentWard = "";
        updateUI();
    });

    // Apply
    btnApply.addEventListener('click', () => {
        let text = currentProvince || "To‡n qu?c";
        
        // Shorten for display
        if (currentWard) text = currentWard;
        else if (currentDistrict) text = currentDistrict;
        else if (currentProvince) text = currentProvince;

        currentLocationLabel.innerText = text;
        
        // Close dropdown
        const dropdownBtn = document.getElementById('locationDropdown');
        // const dropdown = bootstrap.Dropdown.getInstance(dropdownBtn);
        // if(dropdown) dropdown.hide();
        
        // Fallback close
        dropdownBtn.click();
    });

    // Initial UI Update
    updateUI();

    // Wishlist Button Handler
    document.addEventListener('click', function(e) {
        if (e.target.closest('.btn-wishlist')) {
            e.preventDefault();
            e.stopPropagation();
            
            const btn = e.target.closest('.btn-wishlist');
            const productId = btn.getAttribute('data-product-id');
            
            // Get anti-forgery token
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
            
            if (!token) {
                console.error('Anti-forgery token not found');
                return;
            }
            
            // Toggle wishlist
            fetch('/Wishlist/Toggle', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                },
                body: new URLSearchParams({
                    'productId': productId,
                    '__RequestVerificationToken': token
                })
            })
            .then(response => {
                if (!response.ok) {
                    throw new Error('Network response was not ok');
                }
                return response.json();
            })
            .then(data => {
                if (data.success) {
                    // Toggle active class
                    btn.classList.toggle('active');
                    
                    // Update icon
                    const icon = btn.querySelector('i');
                    if (btn.classList.contains('active')) {
                        icon.classList.remove('bi-heart');
                        icon.classList.add('bi-heart-fill');
                    } else {
                        icon.classList.remove('bi-heart-fill');
                        icon.classList.add('bi-heart');
                    }
                    
                    // Update wishlist count in header
                    updateWishlistCount();
                } else {
                    console.error('Toggle failed:', data.message);
                    alert(data.message || 'CÛ l?i x?y ra khi thÍm v‡o wishlist');
                }
            })
            .catch(error => {
                console.error('Error:', error);
                alert('CÛ l?i x?y ra. Vui lÚng th? l?i sau.');
            });
        }
    });
});

// Update wishlist count
function updateWishlistCount() {
    fetch('/Wishlist/Count')
        .then(response => response.json())
        .then(data => {
            const badge = document.querySelector('.wishlist-badge');
            if (badge) {
                badge.textContent = data.count;
                badge.style.display = data.count > 0 ? 'block' : 'none';
            }
        })
        .catch(error => console.error('Error:', error));
}

// Load wishlist count on page load
if (document.querySelector('.wishlist-badge')) {
    updateWishlistCount();
}

// Load wishlist product IDs and update UI
document.addEventListener('DOMContentLoaded', function() {
    if (document.querySelector('.btn-wishlist')) {
        fetch('/Wishlist/GetWishlistProductIds')
            .then(response => response.json())
            .then(data => {
                if (data.productIds && data.productIds.length > 0) {
                    // Update all wishlist buttons
                    data.productIds.forEach(productId => {
                        const btn = document.querySelector(`.btn-wishlist[data-product-id="${productId}"]`);
                        if (btn) {
                            btn.classList.add('active');
                            const icon = btn.querySelector('i');
                            if (icon) {
                                icon.classList.remove('bi-heart');
                                icon.classList.add('bi-heart-fill');
                            }
                        }
                    });
                }
            })
            .catch(error => console.error('Error loading wishlist state:', error));
    }
});

// Auto-Suggest Search Functionality
document.addEventListener('DOMContentLoaded', function() {
    const searchInput = document.getElementById('searchInputHero');
    const suggestionsBox = document.getElementById('searchSuggestions');
    
    if (!searchInput || !suggestionsBox) return;
    
    let debounceTimer;
    
    // Input event handler with debounce
    searchInput.addEventListener('input', function() {
        const keyword = this.value.trim();
        clearTimeout(debounceTimer);
        
        if (keyword.length < 2) {
            suggestionsBox.style.display = 'none';
            return;
        }
        
        debounceTimer = setTimeout(() => {
            fetch('/Search/Suggestions?keyword=' + encodeURIComponent(keyword))
                .then(response => response.json())
                .then(data => {
                    renderSuggestions(data);
                })
                .catch(error => {
                    console.error('Error fetching suggestions:', error);
                    suggestionsBox.style.display = 'none';
                });
        }, 300);
    });
    
    // Render suggestions
    function renderSuggestions(suggestions) {
        if (!suggestions || suggestions.length === 0) {
            suggestionsBox.style.display = 'none';
            return;
        }
        
        let html = '<ul class="list-group">';
        suggestions.forEach(item => {
            html += `
                <li class="list-group-item list-group-item-action suggestion-item border-0 py-2 px-3" 
                    data-keyword="${item.keyword}" 
                    style="cursor: pointer;">
                    <div class="d-flex justify-content-between align-items-center">
                        <div>
                            <i class="bi bi-search me-2 text-muted"></i>
                            <strong>${item.keyword}</strong>
                        </div>
                        <small class="text-muted">${item.category}</small>
                    </div>
                </li>
            `;
        });
        html += '</ul>';
        
        suggestionsBox.innerHTML = html;
        suggestionsBox.style.display = 'block';
    }
    
    // Click handler for suggestions
    suggestionsBox.addEventListener('click', function(e) {
        const suggestionItem = e.target.closest('.suggestion-item');
        if (suggestionItem) {
            const keyword = suggestionItem.getAttribute('data-keyword');
            window.location.href = '/Search?q=' + encodeURIComponent(keyword);
        }
    });
    
    // Hide suggestions on click outside
    document.addEventListener('click', function(e) {
        if (!e.target.closest('#searchInputHero') && !e.target.closest('#searchSuggestions')) {
            suggestionsBox.style.display = 'none';
        }
    });
    
    // Hide suggestions on Escape key
    searchInput.addEventListener('keydown', function(e) {
        if (e.key === 'Escape') {
            suggestionsBox.style.display = 'none';
        }
    });
});

/* Notification Logic */

/* Notification Logic */
document.addEventListener('DOMContentLoaded', function() {
    const notificationDropdown = document.getElementById('notificationDropdown');
    const notificationCount = document.getElementById('notificationCount');
    const notificationList = document.getElementById('notificationList');

    if (!notificationDropdown || !notificationCount || !notificationList) return;

    function getStatusBadge(status) {
        switch(status) {
            case 'PendingPayment': return '<span class="badge bg-warning text-dark">Ch·ªù thanh to√°n</span>';
            case 'Paid': return '<span class="badge bg-info">ƒê√£ thanh to√°n</span>';
            case 'Processing': return '<span class="badge bg-primary">ƒêang x·ª≠ l√Ω</span>';
            case 'Shipped': return '<span class="badge bg-info">ƒêang giao</span>';
            case 'Delivered': return '<span class="badge bg-success">ƒê√£ giao</span>';
            case 'Cancelled': return '<span class="badge bg-danger">ƒê√£ h·ªßy</span>';
            case 'Refunded': return '<span class="badge bg-secondary">Ho√†n ti·ªÅn</span>'; 
            default: return '<span class="badge bg-secondary">' + status + '</span>';
        }
    }

    function fetchNotifications() {
        fetch('/Order/GetOrderNotifications')
            .then(response => {
                if (!response.ok) throw new Error('Network response was not ok');
                return response.json();
            })
            .then(data => {
                if (data.count > 0) {
                    notificationCount.textContent = data.count > 99 ? '99+' : data.count;
                    notificationCount.style.display = 'flex';
                } else {
                    notificationCount.style.display = 'none';
                }

                if (data.orders && data.orders.length > 0) {
                    let html = '';
                    data.orders.forEach(order => {
                        html += `<a href="/Order/Details/${order.id}" class="list-group-item list-group-item-action p-3">
                            <div class="d-flex w-100 justify-content-between mb-1">
                                <strong class="text-truncate" style="max-width: 150px;">ƒê∆°n h√†ng #${order.id}</strong>
                                <small class="text-muted">${order.orderDate}</small>
                            </div>
                            <div class="d-flex justify-content-between align-items-center">
                                <div>${getStatusBadge(order.status)}</div>
                                <small class="fw-bold text-primary">${new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(order.totalAmount)}</small>
                            </div>
                        </a>`;
                    });
                    notificationList.innerHTML = html;
                } else {
                    notificationList.innerHTML = `
                        <div class="text-center p-4 text-muted">
                            <i class="bi bi-inbox fs-2 mb-2 d-block"></i>
                            <p class="mb-0 small">B·∫°n ch∆∞a c√≥ ƒë∆°n h√†ng n√†o.</p>
                        </div>
                    `;
                }
            })
            .catch(error => {
                console.error('Error fetching notifications:', error);
                notificationList.innerHTML = `
                    <div class="text-center p-3 text-danger">
                        <small>Kh√¥ng th·ªÉ t·∫£i th√¥ng b√°o.</small>
                    </div>
                `;
            });
    }

    fetchNotifications();

    // Use Bootstrap event for dropdown
    const dropdownInstance = document.getElementById('notificationDropdown');
    if (dropdownInstance) {
        dropdownInstance.addEventListener('show.bs.dropdown', function () {
           fetchNotifications();
        });
    }
});

/* Wishlist Initialization */
document.addEventListener('DOMContentLoaded', function() {
    initWishlistButtons();
});

function initWishlistButtons() {
    const wishlistButtons = document.querySelectorAll('.btn-wishlist');
    if (wishlistButtons.length === 0) return;

    fetch('/Wishlist/GetWishlistProductIds')
        .then(response => {
            if (!response.ok) throw new Error('Network response was not ok');
            return response.json();
        })
        .then(data => {
            const likedIds = new Set(data.productIds);
            wishlistButtons.forEach(btn => {
                const id = parseInt(btn.getAttribute('data-product-id'));
                if (likedIds.has(id)) {
                    btn.classList.add('active');
                    const icon = btn.querySelector('i');
                    if (icon) {
                        icon.classList.remove('bi-heart');
                        icon.classList.add('bi-heart-fill');
                    }
                }
            });
        })
        .catch(console.error);
}
