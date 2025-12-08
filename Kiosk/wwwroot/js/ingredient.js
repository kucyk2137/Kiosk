document.addEventListener('DOMContentLoaded', () => {
    const addModal = document.getElementById('add-item-modal');
    const defaultContainer = document.getElementById('add-default-ingredients');
    const optionalContainer = document.getElementById('add-optional-ingredients');
    const qtyMinus = document.getElementById('qty-minus');
    const qtyPlus = document.getElementById('qty-plus');
    const qtyValue = document.getElementById('qty-value');
    const addCancel = document.getElementById('add-cancel');
    const addConfirm = document.getElementById('add-confirm');
    const addOptionalCount = document.getElementById('add-optional-count');

    const setChoiceModal = document.getElementById('set-choice-modal');
    const setWithoutBtn = document.getElementById('set-without-btn');
    const setWithBtn = document.getElementById('set-with-btn');

    const setListModal = document.getElementById('set-list-modal');
    const setListContainer = document.getElementById('set-list-container');
    const setListClose = document.getElementById('set-list-close');

    const maxOptionalAddons = 3;
    let optionalSelectedCount = 0;
    let currentForm = null;
    let currentMenuItemId = null;

    function updateOptionalCounter() {
        if (!addOptionalCount) return;
        addOptionalCount.textContent = `${optionalSelectedCount}/${maxOptionalAddons}`;
    }

    function formatLabel(name, price) {
        const numericPrice = Number(price || 0);
        return numericPrice > 0 ? `${name} (+${numericPrice.toFixed(2)} zł)` : name;
    }

    function createChip(name, price, initialCount, isOptional = false) {
        const div = document.createElement('div');
        div.classList.add('ingredient-chip');
        div.dataset.name = name;
        div.dataset.price = Number(price || 0);
        div.dataset.count = Math.max(0, initialCount || 0);

        const label = document.createElement('span');
        label.classList.add('ingredient-name');
        label.textContent = formatLabel(name, price);

        const controls = document.createElement('div');
        controls.classList.add('ingredient-controls');

        const minus = document.createElement('button');
        minus.type = 'button';
        minus.textContent = '−';

        const countDisplay = document.createElement('span');
        countDisplay.classList.add('ingredient-count');

        const plus = document.createElement('button');
        plus.type = 'button';
        plus.textContent = '+';

        const updateDisplay = () => {
            const count = parseInt(div.dataset.count || '0', 10);
            countDisplay.textContent = count;
            div.classList.toggle('selected', count > 0);
        };

        minus.addEventListener('click', () => {
            let count = parseInt(div.dataset.count || '0', 10);
            if (count <= 0) return;

            count -= 1;
            div.dataset.count = count;

            if (isOptional) {
                optionalSelectedCount -= 1;
                updateOptionalCounter();
            }

            updateDisplay();
        });

        plus.addEventListener('click', () => {
            let count = parseInt(div.dataset.count || '0', 10);

            if (isOptional && optionalSelectedCount >= maxOptionalAddons) return;

            count += 1;
            div.dataset.count = count;

            if (isOptional) {
                optionalSelectedCount += 1;
                updateOptionalCounter();
            }

            updateDisplay();
        });

        controls.appendChild(minus);
        controls.appendChild(countDisplay);
        controls.appendChild(plus);

        div.appendChild(label);
        div.appendChild(controls);

        if (isOptional) {
            optionalSelectedCount += parseInt(div.dataset.count || '0', 10);
        }

        updateDisplay();

        return div;
    }

    function resetIngredientModal() {
        if (!qtyValue || !defaultContainer || !optionalContainer) return;

        qtyValue.textContent = '1';
        defaultContainer.innerHTML = '';
        optionalContainer.innerHTML = '';
        optionalSelectedCount = 0;
        updateOptionalCounter();
    }

    function openIngredientModal() {
        if (!currentForm || !currentMenuItemId || !addModal) return;

        resetIngredientModal();

        fetch(`?handler=Ingredients&menuItemId=${currentMenuItemId}`)
            .then(r => {
                if (!r.ok) {
                    throw new Error('Nie udało się pobrać składników');
                }
                return r.json();
            })
            .then(data => {
                const defaults = data.defaults || data.Defaults || [];
                const optionals = data.optionals || data.Optionals || [];

                const normalize = (item) => {
                    if (typeof item === 'string') {
                        return { name: item, price: 0 };
                    }
                    return {
                        name: item.name || item.Name,
                        price: item.price ?? item.Price ?? item.additionalPrice ?? item.AdditionalPrice ?? 0
                    };
                };

                defaults.map(normalize).forEach(({ name, price }) => {
                    const chip = createChip(name, price, 1, false);
                    defaultContainer.appendChild(chip);
                });

                optionals.map(normalize).forEach(({ name, price }) => {
                    const chip = createChip(name, price, 0, true);
                    optionalContainer.appendChild(chip);
                });

                updateOptionalCounter();
                addModal.style.display = 'flex';
            })
            .catch(err => {
                console.error(err);
                // awaryjnie: można po prostu od razu submitować formularz bez popupu
            });
    }

    function renderSetCard(setData) {
        const card = document.createElement('div');
        card.classList.add('recommended-card');

        const media = document.createElement('div');
        media.classList.add('recommended-media');

        const img = document.createElement('img');
        img.src = setData.image;
        img.alt = setData.name;
        media.appendChild(img);

        const info = document.createElement('div');
        info.classList.add('recommended-info');

        const title = document.createElement('div');
        title.classList.add('cell-title');
        title.textContent = setData.name;

        const desc = document.createElement('p');
        desc.classList.add('muted');
        desc.textContent = setData.description || '';

        const itemsList = document.createElement('p');
        itemsList.classList.add('muted');
        const itemNames = (setData.items || []).map(i => i.name).join(', ');
        itemsList.textContent = `Zawiera: ${itemNames}`;

        const price = document.createElement('div');
        price.classList.add('recommended-price');
        price.textContent = `${Number(setData.price || 0).toFixed(2)} zł`;

        const actions = document.createElement('div');
        actions.classList.add('recommended-actions');

        const selectBtn = document.createElement('button');
        selectBtn.type = 'button';
        selectBtn.classList.add('recommend-add-btn');
        selectBtn.textContent = 'Wybierz zestaw';
        selectBtn.addEventListener('click', () => {
            addSetToCart(setData);
        });

        actions.appendChild(selectBtn);

        info.appendChild(title);
        info.appendChild(desc);
        info.appendChild(itemsList);
        info.appendChild(price);
        info.appendChild(actions);

        card.appendChild(media);
        card.appendChild(info);

        return card;
    }

    function addSetToCart(setData) {
        if (!currentForm) return;

        const menuInput = currentForm.querySelector('input[name="menuItemId"]');
        const catInput = currentForm.querySelector('input[name="categoryId"]');
        const ingredientsInput = currentForm.querySelector('input[name="selectedIngredients"]');
        const qtyInput = currentForm.querySelector('input[name="quantity"]');

        if (menuInput) menuInput.value = setData.id;
        if (catInput) catInput.value = setData.categoryId;
        if (ingredientsInput) ingredientsInput.value = '[]';
        if (qtyInput) qtyInput.value = '1';

        // od tej chwili edytujemy składniki dla pozycji zestawu (MenuItem zestawu)
        currentMenuItemId = setData.id;

        if (setListModal) {
            setListModal.style.display = 'none';
        }

        // zamiast od razu dodać do koszyka -> otwieramy popup składników
        openIngredientModal();
    }

    function loadSetsForProduct(menuItemId) {
        if (!setListContainer || !setListModal) {
            // awaryjnie – brak modala, zachowujemy się jak zwykły produkt
            openIngredientModal();
            return;
        }

        setListContainer.innerHTML = '<p class="muted">Ładowanie zestawów...</p>';

        fetch(`?handler=SetsForProduct&menuItemId=${menuItemId}`)
            .then(r => {
                if (!r.ok) {
                    throw new Error('Nie udało się pobrać zestawów');
                }
                return r.json();
            })
            .then(data => {
                if (!data || data.length === 0) {
                    // brak zestawów – wracamy do zwykłego popupu składników
                    setListModal.style.display = 'none';
                    openIngredientModal();
                    return;
                }

                setListContainer.innerHTML = '';
                data.forEach(setData => {
                    setListContainer.appendChild(renderSetCard(setData));
                });

                setListModal.style.display = 'flex';
            })
            .catch(err => {
                console.error(err);
                setListModal.style.display = 'none';
                openIngredientModal();
            });
    }

    // --- Obsługa modala składników ---

    if (qtyMinus && qtyValue) {
        qtyMinus.addEventListener('click', () => {
            let q = parseInt(qtyValue.textContent || '1', 10);
            if (q > 1) q--;
            qtyValue.textContent = q.toString();
        });
    }

    if (qtyPlus && qtyValue) {
        qtyPlus.addEventListener('click', () => {
            let q = parseInt(qtyValue.textContent || '1', 10);
            q++;
            qtyValue.textContent = q.toString();
        });
    }

    addCancel?.addEventListener('click', () => {
        if (addModal) addModal.style.display = 'none';
        currentForm = null;
        currentMenuItemId = null;
    });

    addConfirm?.addEventListener('click', () => {
        if (!currentForm || !defaultContainer || !optionalContainer || !qtyValue) return;

        const selected = [];

        defaultContainer.querySelectorAll('.ingredient-chip').forEach(chip => {
            const count = parseInt(chip.dataset.count || '0', 10);
            for (let i = 0; i < count; i++) {
                selected.push(chip.dataset.name);
            }
        });

        optionalContainer.querySelectorAll('.ingredient-chip').forEach(chip => {
            const count = parseInt(chip.dataset.count || '0', 10);
            for (let i = 0; i < count; i++) {
                selected.push(chip.dataset.name);
            }
        });

        const selectedJson = JSON.stringify(selected);

        const ingredientsInput = currentForm.querySelector('input[name="selectedIngredients"]');
        const qtyInput = currentForm.querySelector('input[name="quantity"]');

        if (ingredientsInput) ingredientsInput.value = selectedJson;
        if (qtyInput) qtyInput.value = parseInt(qtyValue.textContent || '1', 10);

        if (addModal) addModal.style.display = 'none';
        currentForm.submit();
    });

    // --- Obsługa wyboru: bez zestawu / w zestawie ---

    setWithoutBtn?.addEventListener('click', () => {
        if (setChoiceModal) setChoiceModal.style.display = 'none';
        openIngredientModal();
    });

    setWithBtn?.addEventListener('click', () => {
        if (setChoiceModal) setChoiceModal.style.display = 'none';
        loadSetsForProduct(currentMenuItemId);
    });

    setListClose?.addEventListener('click', () => {
        if (setListModal) setListModal.style.display = 'none';
    });

    setChoiceModal?.addEventListener('click', (event) => {
        if (event.target === setChoiceModal) {
            setChoiceModal.style.display = 'none';
        }
    });

    setListModal?.addEventListener('click', (event) => {
        if (event.target === setListModal) {
            setListModal.style.display = 'none';
        }
    });

    // --- Podpięcie przycisków "Dodaj do koszyka" ---

    document.querySelectorAll('.open-ingredient-modal').forEach(btn => {
        btn.addEventListener('click', () => {
            currentForm = btn.closest('form');
            currentMenuItemId = btn.dataset.menuItemId;

            const hasSet = (btn.dataset.hasSet || '').toLowerCase() === 'true';
            const isSetItem = (btn.dataset.isSet || '').toLowerCase() === 'true';

            // 1) Produkt jest już zestawem lub nie ma zestawów -> od razu popup składników
            if (isSetItem || !hasSet) {
                openIngredientModal();
                return;
            }

            // 2) Produkt ma zestawy -> pokaż wybór
            if (setChoiceModal) {
                setChoiceModal.style.display = 'flex';
            } else {
                // fallback: jak zwykły produkt
                openIngredientModal();
            }
        });
    });
});
