        const editModal = document.getElementById('edit-item-modal');
        const defaultContainer = document.getElementById('edit-default-ingredients');
        const optionalContainer = document.getElementById('edit-optional-ingredients');
        const defaultSection = document.getElementById('edit-default-section');
        const optionalSection = document.getElementById('edit-optional-section');
        const qtyMinus = document.getElementById('qty-minus');
        const qtyPlus = document.getElementById('qty-plus');
        const qtyValue = document.getElementById('qty-value');
        const editCancel = document.getElementById('edit-cancel');
        const editSave = document.getElementById('edit-save');
        const editOptionalCount = document.getElementById('edit-optional-count');
        const hiddenMenuItemId = document.getElementById('edit-menu-item-id');
        const hiddenOldSelected = document.getElementById('edit-old-selected-ingredients');
        const hiddenNewSelected = document.getElementById('edit-new-selected-ingredients');
        const hiddenQuantity = document.getElementById('edit-quantity');
        const maxOptionalAddons = 3;
        let optionalSelectedCount = 0;

        const updateOptionalCounter = () => {
            editOptionalCount.textContent = `${optionalSelectedCount}/${maxOptionalAddons}`;
        };

        const setSectionVisibility = (section, isVisible) => {
            if (!section) return;
            section.classList.toggle('is-hidden', !isVisible);
            section.hidden = !isVisible;
            if (isVisible) {
                section.style.removeProperty('display');
            } else {
                section.style.display = 'none';
            }
        };

        const toggleIngredientSections = (hasDefaults, hasOptionals) => {
            setSectionVisibility(defaultSection, hasDefaults);
            setSectionVisibility(optionalSection, hasOptionals);
        };

        const countOccurrences = (list) => {
            return list.reduce((acc, name) => {
                acc[name] = (acc[name] || 0) + 1;
                return acc;
            }, {});
        };
         const formatLabel = (name, price) => {
            const numericPrice = Number(price || 0);
            return numericPrice > 0 ? `${name} (+${numericPrice.toFixed(2)} zł)` : name;
        };

        const createChip = (name, price, initialCount, isOptional = false) => {
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
                const count = parseInt(div.dataset.count || '0');
                countDisplay.textContent = count;
                div.classList.toggle('selected', count > 0);
            };

            minus.addEventListener('click', () => {
                let count = parseInt(div.dataset.count || '0');
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
                let count = parseInt(div.dataset.count || '0');

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
                optionalSelectedCount += parseInt(div.dataset.count || '0');
            }

            updateDisplay();

            return div;
        };

        document.querySelectorAll('.edit-item-btn').forEach(btn => {
            btn.addEventListener('click', () => {
                const menuItemId = btn.dataset.menuItemId;
                const selectedIngredients = btn.dataset.selectedIngredients || "[]";
                const quantity = parseInt(btn.dataset.quantity || "1");

                hiddenMenuItemId.value = menuItemId;
                hiddenOldSelected.value = selectedIngredients;

                qtyValue.textContent = quantity;
                hiddenQuantity.value = quantity;

                let selectedList = [];
                try {
                    selectedList = JSON.parse(selectedIngredients || "[]");
                } catch {
                    selectedList = [];
                }

                const counts = countOccurrences(selectedList);

                optionalSelectedCount = 0;
                updateOptionalCounter();
                defaultContainer.innerHTML = '';
                optionalContainer.innerHTML = '';
                toggleIngredientSections(false, false);

                fetch(`?handler=Ingredients&menuItemId=${menuItemId}`)
                    .then(r => r.json())
                    .then(data => {
                        const defaults = data.defaults || data.Defaults || [];
                        const optionals = data.optionals || data.Optionals || [];

                        const normalize = (item) => {
                            if (typeof item === 'string') return { name: item, price: 0 };
                            return {
                                name: item.name || item.Name,
                                price: item.price ?? item.Price ?? item.additionalPrice ?? item.AdditionalPrice ?? 0
                            };
                        };

                        const normalizedDefaults = defaults
                            .map(normalize)
                            .filter(item => item.name && item.name.trim() !== '');
                        const normalizedOptionals = optionals
                            .map(normalize)
                            .filter(item => item.name && item.name.trim() !== '');

                        normalizedDefaults.forEach(({ name, price }) => {
                            const chip = createChip(name, price, counts[name] ?? 0);
                            defaultContainer.appendChild(chip);
                        });

                        normalizedOptionals.forEach(({ name, price }) => {
                            const chip = createChip(name, price, counts[name] ?? 0, true);
                            optionalContainer.appendChild(chip);
                        });

                        updateOptionalCounter();
                        toggleIngredientSections(normalizedDefaults.length > 0, normalizedOptionals.length > 0);
                        editModal.style.display = 'flex';
                    });
            });
        });

        qtyMinus.addEventListener('click', () => {
            let q = parseInt(qtyValue.textContent || "1");
            if (q > 1) q--;
            qtyValue.textContent = q;
            hiddenQuantity.value = q;
        });

        qtyPlus.addEventListener('click', () => {
            let q = parseInt(qtyValue.textContent || "1");
            q++;
            qtyValue.textContent = q;
            hiddenQuantity.value = q;
        });

        editCancel.addEventListener('click', () => {
            editModal.style.display = 'none';
        });

        editSave.addEventListener('click', () => {
            const selected = [];

            defaultContainer.querySelectorAll('.ingredient-chip').forEach(chip => {
                const count = parseInt(chip.dataset.count || '0');
                for (let i = 0; i < count; i++) {
                    selected.push(chip.dataset.name);
                }
            });

            optionalContainer.querySelectorAll('.ingredient-chip').forEach(chip => {
                const count = parseInt(chip.dataset.count || '0');
                for (let i = 0; i < count; i++) {
                    selected.push(chip.dataset.name);
                }
            });

            hiddenNewSelected.value = JSON.stringify(selected);
            document.getElementById('edit-item-form').submit();
        });