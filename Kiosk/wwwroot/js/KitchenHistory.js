        const historyGrid = document.getElementById('history-grid');
        const formatDate = (dateString) => {
            const date = new Date(dateString);
        return `${date.toLocaleDateString()} • ${date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}`;
        };
         const parseIngredients = (ingredients) => {
            try {
                const parsed = JSON.parse(ingredients || '[]');
        return Array.isArray(parsed) ? parsed : [];
            } catch {
                return [];
            }
        };
        const buildIngredientsMarkup = (item) => {
            const selected = new Set(parseIngredients(item.ingredients));
        const defaultIngredients = item.defaultIngredients || [];
        const optionalIngredients = item.optionalIngredients || [];

            const selectedDefaults = defaultIngredients.filter(name => selected.has(name));
            const removedDefaults = defaultIngredients.filter(name => !selected.has(name));
            const selectedOptionals = optionalIngredients.filter(name => selected.has(name));

            const toEntries = (list, className = '') => list.map(text => ({text, className}));
            const renderEntries = (entries) => entries.length
        ? entries.map(({text, className}) => `<li${className ? ` class=\"${className}\"` : ''}>${text}</li>`).join('')
        : '<li class="muted">Brak</li>';

        const defaultEntries = [
        ...toEntries(selectedDefaults),
                ...toEntries(removedDefaults.map(name => `bez - ${name}`), 'removed-ingredient')
        ];
        const optionalEntries = toEntries(selectedOptionals);

        if (!defaultEntries.length && !optionalEntries.length) {
                return '';
            }
        return `
        <div class="item-ingredients">
            <div class="ingredient-group">
                <div class="ingredient-label">Składniki podstawowe</div>
                <ul class="ingredient-list">
                    ${renderEntries(defaultEntries)}
                </ul>
            </div>
            <div class="ingredient-group">
                <div class="ingredient-label">Dodatki</div>
                <ul class="ingredient-list">
                    ${renderEntries(optionalEntries)}
                </ul>
            </div>
        </div>`;
        };
        const renderHistory = (orders) => {
            historyGrid.innerHTML = '';

        if (!orders.length) {
            historyGrid.innerHTML = `
                    <div class="empty-state">
                        <p class="eyebrow">Brak historii</p>
                        <h3>Nie zamknięto jeszcze żadnych zamówień</h3>
                    </div>`;
        return;
            }

            orders.forEach(order => {
                const card = document.createElement('article');
        card.className = 'kitchen-card kitchen-card--history';
        card.innerHTML = `
        <header class="card-head">
            <div>
                <div class="eyebrow">Zamówienie ${order.orderNumber || `#${order.orderId}`}</div>
                <div class="order-time">${order.orderType || 'Na miejscu'} • ${formatDate(order.orderDate)}</div>
            </div>
            <div class="badge-row">
                <span class="pill">${order.paymentMethod}</span>
                <span class="pill pill--success">Zamknięte</span>
            </div>
        </header>
        <div class="card-body">
            ${order.items.map(item => `
                            <div class="order-item">
                                <span class="qty">${item.quantity}x</span>
                                <div class="item-details">
                                    <div class="item-name">${item.dishName}</div>
                                    ${buildIngredientsMarkup(item)}
                                </div>
                            </div>
                        `).join('')}
        </div>`;

        historyGrid.appendChild(card);
            });
        };

        const loadHistory = async () => {
            historyGrid.classList.add('is-loading');
        try {
                const response = await fetch('/api/orders/history');
        const data = await response.json();
        renderHistory(data);
            } catch (err) {
            historyGrid.innerHTML = '<p class="muted">Nie udało się pobrać historii.</p>';
            } finally {
            historyGrid.classList.remove('is-loading');
            }
        };

        loadHistory();
