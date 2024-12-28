let cartModal;

document.addEventListener('DOMContentLoaded', async function() {
    try {
        await checkAuth();
        await loadProducts();
        await loadCart();
        
        cartModal = new bootstrap.Modal(document.getElementById('cartModal'));
    } catch (error) {
        console.error('Initialization error:', error);
    }
});

async function loadProducts() {
    try {
        const token = localStorage.getItem('token');
        const response = await fetch('/api/products', {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (!response.ok) {
            throw new Error('Ürünler yüklenemedi');
        }

        const products = await response.json();
        const container = document.getElementById('productsContainer');
        
        // Önce container'ı temizle
        container.innerHTML = '';

        // Her ürün için bir kart oluştur
        products.forEach(product => {
            const card = `
                <div class="col">
                    <div class="card h-100">
                        <img src="${product.imageUrl || '/images/no-image.png'}" 
                             class="card-img-top" 
                             alt="${product.name}"
                             style="height: 200px; object-fit: cover;">
                        <div class="card-body d-flex flex-column">
                            <h5 class="card-title">${product.name}</h5>
                            <p class="card-text">${product.description || ''}</p>
                            <p class="card-text">
                                <small class="text-muted">${product.brand || ''}</small>
                            </p>
                            <p class="card-text fw-bold">${product.price} ₺</p>
                            <p class="card-text">
                                <small class="text-${product.stock > 0 ? 'success' : 'danger'}">
                                    ${product.stock > 0 ? 'Stokta' : 'Stokta Yok'}
                                </small>
                            </p>
                            <button class="btn btn-primary mt-auto" 
                                    onclick="addToCart('${product.id}')"
                                    ${product.stock > 0 ? '' : 'disabled'}>
                                <i class="bi bi-cart-plus"></i> Sepete Ekle
                            </button>
                        </div>
                    </div>
                </div>
            `;
            container.innerHTML += card;
        });
    } catch (error) {
        console.error('Error loading products:', error);
        showToast('Ürünler yüklenirken bir hata oluştu', 'error');
    }
}

function checkAuth() {
    const token = localStorage.getItem('token');
    const userRole = localStorage.getItem('userRole');

    if (!token || !userRole) {
        window.location.replace('/login.html');
        return;
    }

    if (userRole !== 'Customer') {
        alert('Bu sayfaya erişim yetkiniz yok!');
        window.location.replace('/login.html');
        return;
    }
}

function logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('userRole');
    window.location.replace('/login.html');
}

async function loadCart() {
    try {
        const token = localStorage.getItem('token');
        const response = await fetch('/api/cart', {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const cart = await response.json();
        updateCartDisplay(cart);
    } catch (error) {
        console.error('Error loading cart:', error);
    }
}

function updateCartDisplay(cart) {
    const cartItems = document.getElementById('cartItems');
    const emptyCart = document.getElementById('emptyCart');
    const cartTotal = document.getElementById('cartTotal');
    const cartCount = document.getElementById('cartCount');

    if (!cart || cart.items.length === 0) {
        cartItems.innerHTML = '';
        emptyCart.style.display = 'block';
        cartTotal.textContent = '0.00 ₺';
        cartCount.textContent = '0';
        return;
    }

    emptyCart.style.display = 'none';
    cartCount.textContent = cart.items.length.toString();
    cartTotal.textContent = `${cart.totalAmount.toFixed(2)} ₺`;

    cartItems.innerHTML = cart.items.map(item => `
        <div class="d-flex align-items-center mb-3 border-bottom pb-3">
            <img src="${item.imageUrl || '/images/no-image.png'}" 
                 alt="${item.productName}"
                 style="width: 64px; height: 64px; object-fit: cover;"
                 class="me-3">
            <div class="flex-grow-1">
                <h6 class="mb-0">${item.productName}</h6>
                <small class="text-muted">${item.price} ₺</small>
            </div>
            <div class="d-flex align-items-center">
                <button class="btn btn-sm btn-outline-secondary me-2" 
                        onclick="updateQuantity('${item.productId}', ${item.quantity - 1})"
                        ${item.quantity <= 1 ? 'disabled' : ''}>
                    <i class="bi bi-dash"></i>
                </button>
                <span class="mx-2">${item.quantity}</span>
                <button class="btn btn-sm btn-outline-secondary ms-2" 
                        onclick="updateQuantity('${item.productId}', ${item.quantity + 1})">
                    <i class="bi bi-plus"></i>
                </button>
                <button class="btn btn-sm btn-outline-danger ms-3" 
                        onclick="removeFromCart('${item.productId}')">
                    <i class="bi bi-trash"></i>
                </button>
            </div>
        </div>
    `).join('');
}

async function addToCart(productId) {
    try {
        console.log('Adding product to cart:', productId);

        const token = localStorage.getItem('token');
        const response = await fetch('/api/cart/items', {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                productId: productId,
                quantity: 1
            })
        });

        if (!response.ok) {
            const errorData = await response.json();
            throw new Error(errorData.message || 'Ürün sepete eklenemedi');
        }

        await loadCart();
        showToast('Ürün sepete eklendi', 'success');
    } catch (error) {
        console.error('Error adding to cart:', error);
        showToast(error.message || 'Ürün sepete eklenirken bir hata oluştu', 'error');
    }
}

async function updateQuantity(productId, quantity) {
    if (quantity < 1) return;

    try {
        const token = localStorage.getItem('token');
        const response = await fetch(`/api/cart/items/${productId}`, {
            method: 'PUT',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ quantity: quantity })
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        await loadCart();
    } catch (error) {
        console.error('Error updating quantity:', error);
        showToast('Miktar güncellenirken bir hata oluştu', 'error');
    }
}

async function removeFromCart(productId) {
    try {
        const token = localStorage.getItem('token');
        const response = await fetch(`/api/cart/items/${productId}`, {
            method: 'DELETE',
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        await loadCart();
        showToast('Ürün sepetten kaldırıldı');
    } catch (error) {
        console.error('Error removing from cart:', error);
        showToast('Ürün sepetten kaldırılırken bir hata oluştu', 'error');
    }
}

async function clearCart() {
    if (!confirm('Sepeti temizlemek istediğinize emin misiniz?')) {
        return;
    }

    try {
        const token = localStorage.getItem('token');
        const response = await fetch('/api/cart', {
            method: 'DELETE',
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        await loadCart();
        showToast('Sepet temizlendi');
    } catch (error) {
        console.error('Error clearing cart:', error);
        showToast('Sepet temizlenirken bir hata oluştu', 'error');
    }
}

function showCart() {
    cartModal.show();
}

// Toast container'ı sayfa yüklendiğinde oluştur
document.addEventListener('DOMContentLoaded', function() {
    if (!document.getElementById('toastContainer')) {
        const container = document.createElement('div');
        container.id = 'toastContainer';
        container.className = 'toast-container position-fixed bottom-0 end-0 p-3';
        document.body.appendChild(container);
    }
});

function showToast(message, type = 'success') {
    const toastId = 'toast_' + new Date().getTime();
    const bgColor = type === 'success' ? 'bg-success' : 
                    type === 'error' ? 'bg-danger' : 
                    type === 'info' ? 'bg-info' : 'bg-primary';

    const toastHtml = `
        <div id="${toastId}" class="toast ${bgColor} text-white" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="toast-header ${bgColor} text-white">
                <strong class="me-auto">Bildirim</strong>
                <button type="button" class="btn-close btn-close-white" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
            <div class="toast-body">
                ${message}
            </div>
        </div>
    `;

    const container = document.getElementById('toastContainer');
    container.insertAdjacentHTML('beforeend', toastHtml);

    const toastElement = document.getElementById(toastId);
    const toast = new bootstrap.Toast(toastElement, {
        delay: 3000,
        autohide: true
    });

    toast.show();

    // Toast kapandığında DOM'dan kaldır
    toastElement.addEventListener('hidden.bs.toast', function () {
        toastElement.remove();
    });
}

async function checkout() {
    try {
        console.log('Checkout function started');

        const shippingAddress = document.getElementById('shippingAddress').value;
        const paymentMethod = document.getElementById('paymentMethod').value;

        console.log('Form values:', { shippingAddress, paymentMethod });

        if (!shippingAddress || !paymentMethod) {
            showToast('Lütfen tüm alanları doldurun', 'error');
            return;
        }

        // Sepet boş mu kontrol et
        const cartItems = document.getElementById('cartItems');
        if (!cartItems || cartItems.children.length === 0) {
            showToast('Sepetiniz boş. Sipariş oluşturulamadı!', 'error');
            return;
        }

        const token = localStorage.getItem('token');
        if (!token) {
            showToast('Oturum süreniz dolmuş. Lütfen tekrar giriş yapın.', 'error');
            window.location.href = '/login.html';
            return;
        }

        console.log('Making API request with token:', token);
        showToast('Siparişiniz işleniyor...', 'info');

        const response = await fetch('/api/order', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify({
                shippingAddress,
                paymentMethod
            })
        });

        console.log('API Response status:', response.status);
        const responseData = await response.json();
        console.log('API Response data:', responseData);

        if (!response.ok) {
            throw new Error(responseData.message || 'Sipariş oluşturulurken bir hata oluştu');
        }

        showToast('Siparişiniz başarıyla oluşturuldu! Teşekkür ederiz.', 'success');

        // Modal'ı kapat
        const cartModal = bootstrap.Modal.getInstance(document.getElementById('cartModal'));
        cartModal.hide();

        // Formu sıfırla
        document.getElementById('shippingAddress').value = '';
        document.getElementById('paymentMethod').value = '';
        document.getElementById('orderForm').style.display = 'none';
        document.getElementById('checkoutButton').style.display = 'inline-block';
        document.getElementById('confirmOrderButton').style.display = 'none';

        await loadCart();
    } catch (error) {
        console.error('Checkout error:', error);
        showToast(error.message || 'Sipariş oluşturulurken bir hata oluştu', 'error');
    }
}

function showOrderForm() {
    console.log('Showing order form'); // Debug log
    const orderForm = document.getElementById('orderForm');
    const checkoutButton = document.getElementById('checkoutButton');
    const confirmOrderButton = document.getElementById('confirmOrderButton');

    if (!orderForm || !checkoutButton || !confirmOrderButton) {
        console.error('Required elements not found!');
        return;
    }

    orderForm.style.display = 'block';
    checkoutButton.style.display = 'none';
    confirmOrderButton.style.display = 'inline-block';
} 