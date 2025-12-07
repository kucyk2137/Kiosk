        const tiles = document.querySelectorAll('.payment-tile');
        const paymentInput = document.querySelector('input[name="SelectedPaymentMethod"]');

        tiles.forEach(tile => {
            tile.addEventListener('click', () => {
                tiles.forEach(t => t.classList.remove('is-selected'));
                tile.classList.add('is-selected');
                paymentInput.value = tile.dataset.method;
            });
        });

        // AUTO-ZAMKNIĘCIE / POWRÓT PO 10 SEKUNDACH PO ZŁOŻENIU ZAMÓWIENIA
        const hasOrderNumber = '@(string.IsNullOrEmpty(Model.CreatedOrderNumber) ? "false" : "true")' === 'true';

        if (hasOrderNumber) {
            setTimeout(() => {
                const btn = document.getElementById('finish-btn');
                if (btn) {
                    btn.click(); // zadziała jak kliknięcie „Zakończ”
                } else {
                    // awaryjnie
                    window.location.href = '@Url.Page("/lockscreen")';
                }
            }, 10000); // 10 000 ms = 10 sekund
        }