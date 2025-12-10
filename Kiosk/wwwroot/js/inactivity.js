(function () {
    const config = window.kioskInactivityConfig;
    if (!config || !config.lockscreenUrl) {
        return;
    }

    const idleTimeoutMs = Number(config.idleTimeoutMs) || 40000;
    const promptTimeoutMs = Number(config.promptTimeoutMs) || 15000;
    const lockscreenUrl = config.lockscreenUrl;

    let idleTimer;
    let promptTimer;
    let countdownInterval;
    let modal;
    let countdownLabel;

    const createModal = () => {
        modal = document.createElement('div');
        modal.className = 'modal inactivity-modal';
        modal.style.display = 'none';
        modal.setAttribute('role', 'dialog');
        modal.setAttribute('aria-live', 'polite');
        modal.innerHTML = `
            <div class="modal-content">
                <h3>Czy chcesz kontynuować?</h3>
                <p class="muted">Brak działania spowoduje powrót do ekranu startowego.</p>
                <div class="modal-actions" style="justify-content:flex-start;">
                    <button type="button" class="btn primary" id="inactivity-continue">Kontynuuj</button>
                </div>
                <p class="muted inactivity-countdown">Sesja wygaśnie za <span id="inactivity-counter"></span>s</p>
            </div>`;
        document.body.appendChild(modal);
        countdownLabel = modal.querySelector('#inactivity-counter');
        modal.querySelector('#inactivity-continue').addEventListener('click', hidePrompt);
    };

    const navigateToLockscreen = () => {
        window.location.href = `${lockscreenUrl}?resetSession=1`;
    };

    const stopCountdown = () => {
        window.clearInterval(countdownInterval);
        countdownInterval = undefined;
    };

    const startCountdown = () => {
        let remaining = Math.ceil(promptTimeoutMs / 1000);
        if (countdownLabel) {
            countdownLabel.textContent = remaining;
        }

        countdownInterval = window.setInterval(() => {
            remaining -= 1;
            if (remaining <= 0) {
                stopCountdown();
            }
            if (countdownLabel) {
                countdownLabel.textContent = Math.max(0, remaining);
            }
        }, 1000);
    };

    const showPrompt = () => {
        if (!modal) {
            createModal();
        }
        modal.style.display = 'flex';
        startCountdown();
        promptTimer = window.setTimeout(navigateToLockscreen, promptTimeoutMs);
    };

    const hidePrompt = () => {
        if (modal) {
            modal.style.display = 'none';
        }
        window.clearTimeout(promptTimer);
        stopCountdown();
        startIdleTimer();
    };

    const startIdleTimer = () => {
        window.clearTimeout(idleTimer);
        idleTimer = window.setTimeout(showPrompt, idleTimeoutMs);
    };

    const onActivity = () => {
        if (modal && modal.style.display !== 'none') {
            hidePrompt();
        } else {
            startIdleTimer();
        }
    };

    ['mousemove', 'mousedown', 'keydown', 'touchstart', 'click'].forEach(evt => {
        document.addEventListener(evt, onActivity, { passive: true });
    });

    startIdleTimer();
})();