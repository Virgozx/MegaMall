// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener('DOMContentLoaded', function() {
    // Location Data (Mock)
    const locationData = {
        "Thái Nguyên": {
            "TP Thái Nguyên": ["Phường Trưng Vương", "Phường Hoàng Văn Thụ", "Phường Phan Đình Phùng", "Phường Quang Trung"],
            "TP Sông Công": ["Phường Mỏ Chè", "Phường Thắng Lợi", "Phường Cải Đan"],
            "Huyện Đại Từ": ["Thị trấn Hùng Sơn", "Xã Hà Thượng", "Xã Tân Thái"]
        },
        "Hà Nội": {
            "Quận Ba Đình": ["Phường Phúc Xá", "Phường Trúc Bạch", "Phường Vĩnh Phúc"],
            "Quận Hoàn Kiếm": ["Phường Phúc Tân", "Phường Đồng Xuân", "Phường Hàng Mã"],
            "Quận Cầu Giấy": ["Phường Dịch Vọng", "Phường Mai Dịch", "Phường Nghĩa Đô"]
        },
        "Tp Hồ Chí Minh": {
            "Quận 1": ["Phường Bến Nghé", "Phường Bến Thành", "Phường Cô Giang"],
            "Quận 3": ["Phường Võ Thị Sáu", "Phường 1", "Phường 2"],
            "TP Thủ Đức": ["Phường Thảo Điền", "Phường An Phú", "Phường Bình Chiểu"]
        },
        "Đà Nẵng": {
            "Quận Hải Châu": ["Phường Hải Châu 1", "Phường Hải Châu 2"],
            "Quận Thanh Khê": ["Phường Thanh Khê Tây", "Phường Thanh Khê Đông"]
        }
    };

    // State
    let currentProvince = "Thái Nguyên";
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
            lblDistrictTitle.innerText = "Quận/Huyện";
        } else {
            lblDistrict.innerText = "";
            lblDistrict.classList.add('d-none');
            lblDistrictTitle.classList.remove('fw-bold');
            lblDistrictTitle.classList.add('text-muted');
            lblDistrictTitle.innerText = "Chọn quận huyện";
        }

        // Ward
        if (currentWard) {
            lblWard.innerText = currentWard;
            lblWard.classList.remove('d-none');
            lblWardTitle.classList.add('fw-bold');
            lblWardTitle.classList.remove('text-muted');
            lblWardTitle.innerText = "Phường/Xã";
        } else {
            lblWard.innerText = "";
            lblWard.classList.add('d-none');
            lblWardTitle.classList.remove('fw-bold');
            lblWardTitle.classList.add('text-muted');
            lblWardTitle.innerText = "Chọn phường xã";
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
        showList('province', 'Chọn Tỉnh/Thành', provinces);
    });

    btnDistrict.addEventListener('click', (e) => {
        e.stopPropagation();
        if (!currentProvince) return;
        const districts = Object.keys(locationData[currentProvince] || {});
        showList('district', 'Chọn Quận/Huyện', districts);
    });

    btnWard.addEventListener('click', (e) => {
        e.stopPropagation();
        if (!currentDistrict) return;
        const wards = locationData[currentProvince][currentDistrict] || [];
        showList('ward', 'Chọn Phường/Xã', wards);
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
        let text = currentProvince || "Toàn quốc";
        
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
                    alert(data.message || 'Có lỗi xảy ra khi thêm vào wishlist');
                }
            })
            .catch(error => {
                console.error('Error:', error);
                alert('Có lỗi xảy ra. Vui lòng thử lại sau.');
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
