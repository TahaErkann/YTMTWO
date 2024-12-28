let productModal;
let deleteModal;
let selectedProductId;

document.addEventListener('DOMContentLoaded', async function() {
    try {
        await checkAuth();
        await loadProducts();
        
        // Bootstrap modallarını başlat
        productModal = new bootstrap.Modal(document.getElementById('productModal'));
        deleteModal = new bootstrap.Modal(document.getElementById('deleteModal'));
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
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const products = await response.json();
        displayProducts(products);
    } catch (error) {
        console.error('Error loading products:', error);
        alert('Ürünler yüklenirken bir hata oluştu: ' + error.message);
    }
}

function displayProducts(products) {
    const tableBody = document.getElementById('productsTable');
    tableBody.innerHTML = '';

    products.forEach(product => {
        const row = document.createElement('tr');
        row.innerHTML = `
            <td>
                <img src="${product.imageUrl || '/images/no-image.png'}" 
                     class="product-image" 
                     alt="${product.name}">
            </td>
            <td>${product.name}</td>
            <td>${product.description || '-'}</td>
            <td>${product.price} ₺</td>
            <td>${product.brand || '-'}</td>
            <td>${product.stock}</td>
            <td>
                <span class="badge ${product.isActive ? 'bg-success' : 'bg-danger'}">
                    ${product.isActive ? 'Aktif' : 'Pasif'}
                </span>
            </td>
            <td>
                <button class="btn btn-sm btn-primary" onclick="showEditProductModal('${product.id}')">
                    <i class="bi bi-pencil"></i>
                </button>
                <button class="btn btn-sm btn-danger" onclick="showDeleteModal('${product.id}')">
                    <i class="bi bi-trash"></i>
                </button>
            </td>
        `;
        tableBody.appendChild(row);
    });
}

function showAddProductModal() {
    document.getElementById('modalTitle').textContent = 'Yeni Ürün Ekle';
    document.getElementById('productForm').reset();
    document.getElementById('productId').value = '';
    productModal.show();
}

async function showEditProductModal(id) {
    try {
        const token = localStorage.getItem('token');
        const response = await fetch(`/api/products/${id}`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const product = await response.json();
        
        document.getElementById('modalTitle').textContent = 'Ürün Düzenle';
        document.getElementById('productId').value = product.id;
        document.getElementById('name').value = product.name;
        document.getElementById('description').value = product.description || '';
        document.getElementById('price').value = product.price;
        document.getElementById('brand').value = product.brand || '';
        document.getElementById('imageUrl').value = product.imageUrl || '';
        document.getElementById('stock').value = product.stock;
        document.getElementById('isActive').checked = product.isActive;

        productModal.show();
    } catch (error) {
        console.error('Error loading product:', error);
        alert('Ürün bilgileri yüklenirken bir hata oluştu: ' + error.message);
    }
}

async function saveProduct() {
    try {
        const token = localStorage.getItem('token');
        const productId = document.getElementById('productId').value;
        
        const productData = {
            name: document.getElementById('name').value,
            description: document.getElementById('description').value,
            price: parseFloat(document.getElementById('price').value),
            brand: document.getElementById('brand').value,
            imageUrl: document.getElementById('imageUrl').value,
            stock: parseInt(document.getElementById('stock').value),
            isActive: document.getElementById('isActive').checked
        };

        const url = productId ? `/api/products/${productId}` : '/api/products';
        const method = productId ? 'PUT' : 'POST';

        if (productId) {
            productData.id = productId;
        }

        const response = await fetch(url, {
            method: method,
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(productData)
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        productModal.hide();
        await loadProducts();
        alert(productId ? 'Ürün başarıyla güncellendi.' : 'Ürün başarıyla eklendi.');
    } catch (error) {
        console.error('Error saving product:', error);
        alert('Ürün kaydedilirken bir hata oluştu: ' + error.message);
    }
}

function showDeleteModal(id) {
    selectedProductId = id;
    deleteModal.show();
}

async function confirmDelete() {
    try {
        const token = localStorage.getItem('token');
        const response = await fetch(`/api/products/${selectedProductId}`, {
            method: 'DELETE',
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        deleteModal.hide();
        await loadProducts();
        alert('Ürün başarıyla silindi.');
    } catch (error) {
        console.error('Error deleting product:', error);
        alert('Ürün silinirken bir hata oluştu: ' + error.message);
    }
}

function checkAuth() {
    const token = localStorage.getItem('token');
    const userRole = localStorage.getItem('userRole');

    if (!token || !userRole) {
        window.location.replace('/login.html');
        return;
    }

    if (userRole !== 'Admin') {
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