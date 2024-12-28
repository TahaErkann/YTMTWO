async function applyFilters() {
    const brand = document.getElementById('brandFilter').value;
    const minPrice = document.getElementById('minPrice').value;
    const maxPrice = document.getElementById('maxPrice').value;

    try {
        let url = '/api/products/active';
        const params = new URLSearchParams();
        
        if (brand) params.append('brand', brand);
        if (minPrice) params.append('minPrice', minPrice);
        if (maxPrice) params.append('maxPrice', maxPrice);

        if (params.toString()) {
            url += '?' + params.toString();
        }

        const response = await fetch(url);
        const products = await response.json();
        updateProductGrid(products);
    } catch (error) {
        console.error('Filtreleme hatası:', error);
    }
} 