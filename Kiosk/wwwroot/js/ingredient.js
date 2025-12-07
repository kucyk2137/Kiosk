        const addModal = document.getElementById('add-item-modal');
        const defaultContainer = document.getElementById('add-default-ingredients');
        const optionalContainer = document.getElementById('add-optional-ingredients');
        const qtyMinus = document.getElementById('qty-minus');
        const qtyPlus = document.getElementById('qty-plus');
        const qtyValue = document.getElementById('qty-value');
        const addCancel = document.getElementById('add-cancel');
        const addConfirm = document.getElementById('add-confirm');
        const addOptionalCount = document.getElementById('add-optional-count');

        const maxOptionalAddons = 3;
        let optionalSelectedCount = 0;
        let currentForm = null;
        let currentMenuItemId = null;

        function updateOptionalCounter() {
            addOptionalCount.textContent = `${optionalSelectedCount}/${maxOptionalAddons}`;
        }

        function createChip(name, initialCount, isOptional = false) {
            const div = document.createElement('div');
            div.classList.add('ingredient-chip');
            div.dataset.name = name;
            div.dataset.count = Math.max(0, initialCount || 0);

            const label = document.createElement('span');
            label.classList.add('ingredient-name');
            label.textContent = name;

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
        }

        document.querySelectorAll('.open-ingredient-modal').forEach(btn => {
            btn.addEventListener('click', () => {
                currentForm = btn.closest('form');
                currentMenuItemId = btn.dataset.menuItemId;

                // ilość zawsze startuje od 1 przy dodawaniu
                qtyValue.textContent = '1';
                defaultContainer.innerHTML = '';
                optionalContainer.innerHTML = '';
                optionalSelectedCount = 0;
                updateOptionalCounter();

                fetch(`?handler=Ingredients&menuItemId=${currentMenuItemId}`)
                    .then(r => r.json())
                    .then(data => {
                        const defaults = data.defaults || data.Defaults || [];
                        const optionals = data.optionals || data.Optionals || [];

                        // główne składniki domyślnie zaznaczone
                        defaults.forEach(name => {
                            const chip = createChip(name, 1);
                            defaultContainer.appendChild(chip);
                        });

                        // dodatki domyślnie odznaczone
                        optionals.forEach(name => {
                            const chip = createChip(name, 0, true);
                            optionalContainer.appendChild(chip);
                        });

                        updateOptionalCounter();
                        addModal.style.display = 'flex';
                    });
            });
        });

        qtyMinus.addEventListener('click', () => {
            let q = parseInt(qtyValue.textContent || "1");
            if (q > 1) q--;
            qtyValue.textContent = q;
        });

        qtyPlus.addEventListener('click', () => {
            let q = parseInt(qtyValue.textContent || "1");
            q++;
            qtyValue.textContent = q;
        });

        addCancel.addEventListener('click', () => {
            addModal.style.display = 'none';
            currentForm = null;
            currentMenuItemId = null;
        });

        addConfirm.addEventListener('click', () => {
            if (!currentForm) return;

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

            const selectedJson = JSON.stringify(selected);

            currentForm.querySelector('input[name="selectedIngredients"]').value = selectedJson;
            currentForm.querySelector('input[name="quantity"]').value = parseInt(qtyValue.textContent || "1");

            addModal.style.display = 'none';
            currentForm.submit();
        });
