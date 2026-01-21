class ApiClient {
    constructor(baseUrl = '') {
        this.baseUrl = baseUrl;
    }

    /**
     * Lấy AntiForgeryToken từ input hidden (thường có trong các form ASP.NET Core)
     */
    _getAntiForgeryToken() {
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenInput ? tokenInput.value : null;
    }

    async request(url, method = 'GET', data = null) {
        const headers = {
            'Content-Type': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        };

        // Tự động thêm CSRF Token nếu có
        const token = this._getAntiForgeryToken();
        if (token) {
            headers['RequestVerificationToken'] = token;
        }

        const options = {
            method: method,
            headers: headers
        };

        if (data) {
            options.body = JSON.stringify(data);
        }

        try {
            const response = await fetch(`${this.baseUrl}${url}`, options);

            // Kiểm tra trạng thái HTTP (200-299 là thành công)
            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(errorText || `Lỗi API (${response.status})`);
            }

            // Tự động parse JSON nếu có thể
            const contentType = response.headers.get("content-type");
            if (contentType && contentType.indexOf("application/json") !== -1) {
                return await response.json();
            } else {
                return await response.text();
            }

        } catch (error) {
            console.error('Lỗi khi gọi API:', error);
            throw error; // Ném lỗi ra để code bên ngoài xử lý tiếp
        }
    }

    get(url) {
        return this.request(url, 'GET');
    }

    post(url, data) {
        return this.request(url, 'POST', data);
    }

    put(url, data) {
        return this.request(url, 'PUT', data);
    }

    delete(url) {
        return this.request(url, 'DELETE');
    }
}

// Export instance global để dùng luôn (tùy chọn)
window.api = new ApiClient();
