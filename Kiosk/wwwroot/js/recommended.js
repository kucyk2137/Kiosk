document.addEventListener('DOMContentLoaded', () => {
    const cartButton = document.getElementById('cart-btn');
    const cartForm = document.getElementById('cart-form');
    const recommendedModal = document.getElementById('recommended-modal');

    if (!cartButton || !cartForm) {
        return;
    }

    const cartCount = parseInt(cartButton.dataset.cartCount || '0', 10);

    const openModal = () => {
        if (recommendedModal) {
            recommendedModal.style.display = 'flex';
        }
    };

    const closeModal = () => {
        if (recommendedModal) {
            recommendedModal.style.display = 'none';
        }
    };

    cartButton.addEventListener('click', (event) => {
        if (recommendedModal && cartCount > 0) {
            event.preventDefault();
            openModal();
        } else {
            cartForm.submit();
        }
    });

    document.getElementById('recommended-go-to-cart')?.addEventListener('click', () => {
        cartForm.submit();
    });

    document.getElementById('recommended-continue')?.addEventListener('click', () => {
        closeModal();
    });

    recommendedModal?.addEventListener('click', (event) => {
        if (event.target === recommendedModal) {
            closeModal();
        }
    });

    document.querySelectorAll('.recommended-add-form').forEach(form => {
        form.addEventListener('submit', (event) => {
            event.preventDefault(); 

            const formData = new FormData(form);

            fetch(window.location.pathname, {
                method: 'POST',
                body: formData,
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            })
                .then(() => {
                    const btn = form.querySelector('.recommend-add-btn');
                    if (btn) {
                        btn.textContent = 'Dodano!';
                        btn.disabled = true;
                    }
                })
                .catch(err => {
                    console.error('Błąd przy dodawaniu produktu:', err);
                });
        });
    });
});