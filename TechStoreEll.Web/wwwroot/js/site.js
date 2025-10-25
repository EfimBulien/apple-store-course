function showThemeNotification(theme) {
    const existingNotification = document.querySelector('.theme-notification');
    if (existingNotification) {
        existingNotification.remove();
    }

    const notification = document.createElement('div');
    notification.className = `theme-notification alert alert-${theme === 'dark' ? 'dark' : 'info'} alert-dismissible fade show position-fixed`;

    notification.style.cssText = `
        top: 200px;
        left: 20px;
        z-index: 9998;
        max-width: 250px;
        box-shadow: 0 4px 12px rgba(0,0,0,0.15);
        border: none;
    `;

    const themeIcon = theme === 'dark' ? 'bi-moon-stars' : 'bi-sun';
    const themeText = theme === 'dark' ? 'Тёмная тема' : 'Светлая тема';

    notification.innerHTML = `
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        <div class="d-flex align-items-center">
            <i class="bi ${themeIcon} me-2 fs-5"></i>
            <div>
                <strong>${themeText}</strong>
                <div class="small">Тема изменена</div>
            </div>
        </div>
    `;

    document.body.appendChild(notification);

    setTimeout(() => {
        if (notification.parentNode) {
            const bsAlert = new bootstrap.Alert(notification);
            bsAlert.close();
        }
    }, 3000);
}

function showHotkeysHint(forceShow = false) {
    if (!forceShow && localStorage.getItem('hotkeysHintClosed') === 'true') {
        return;
    }

    const existingHint = document.querySelector('.hotkeys-hint');
    if (existingHint) {
        existingHint.remove();
    }

    const hint = document.createElement('div');
    hint.className = 'hotkeys-hint alert alert-info alert-dismissible fade show position-fixed';

    hint.style.cssText = `
        top: 80px;
        left: 20px;
        z-index: 9999;
        max-width: 300px;
        box-shadow: 0 4px 12px rgba(0,0,0,0.1);
    `;

    const isAuthenticated = document.body.getAttribute('data-authenticated') === 'true';
    const isAdmin = document.body.getAttribute('data-is-admin') === 'true';

    let hotkeysHTML = `
        <div><kbd>Ctrl+0</kbd> - Смена темы</div>
        <div><kbd>Ctrl+1</kbd> - Главная</div>
        <div><kbd>Ctrl+2</kbd> - Профиль</div>
        <div><kbd>Ctrl+P</kbd> - Политика конфиденциальности</div>
        <div><kbd>Ctrl+O</kbd> - О нас</div>
        <div><kbd>Ctrl+T</kbd> - Условия использования</div>
        <div><kbd>Ctrl+K</kbd> - Контакты</div>
    `;

    if (isAuthenticated && !isAdmin) {
        hotkeysHTML += `
            <div><kbd>Ctrl+3</kbd> - Корзина</div>
            <div><kbd>Ctrl+4</kbd> - Заказы</div>
            <div><kbd>Ctrl+5</kbd> - Адреса</div>
        `;
    } else if (isAdmin) {
        hotkeysHTML += `
            <div><kbd>Ctrl+3</kbd> - Админ панель</div>
            <div><kbd>Ctrl+4</kbd> - Создание товара</div>
            <div><kbd>Ctrl+5</kbd> - Пополнение склада</div>
            <div><kbd>Ctrl+6</kbd> - Аналитика</div>
        `;
    }

    hint.innerHTML = `
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        <h6 class="alert-heading mb-2">
            <i class="bi bi-keyboard me-2"></i>Горячие клавиши
        </h6>
        <div class="small">
            ${hotkeysHTML}
        </div>
    `;

    hint.querySelector('.btn-close').addEventListener('click', () => {
        localStorage.setItem('hotkeysHintClosed', 'true');
    });

    document.body.appendChild(hint);

    if (!forceShow) {
        setTimeout(() => {
            if (hint.parentNode) {
                const bsAlert = new bootstrap.Alert(hint);
                bsAlert.close();
            }
        }, 8000);
    }
}

function showHotkeysHintAuto() {
    showHotkeysHint(false);
}

function showHotkeysHintForced() {
    showHotkeysHint(true);
}

document.addEventListener('DOMContentLoaded', function() {
    const isAuthenticated = document.body.getAttribute('data-authenticated') === 'true';
    const isAdmin = document.body.getAttribute('data-is-admin') === 'true';

    const hotkeys = {
        'ctrl+0': toggleTheme,
        'ctrl+1': '/Home/Index',
        'ctrl+2': '/Profile/Index',
        'ctrl+p': '/Home/Privacy',
        'ctrl+t': '/Home/Terms',
        'ctrl+o': '/Home/About',
        'ctrl+k': '/Home/Contact'
    };

    if (isAuthenticated && !isAdmin) {
        Object.assign(hotkeys, {
            'ctrl+3': '/Cart/Index',
            'ctrl+4': '/Order/Index',
            'ctrl+5': '/Address/Index'
        });
    } else if (isAdmin) {
        Object.assign(hotkeys, {
            'ctrl+3': '/Admin/AdminPanel',
            'ctrl+4': '/Product/Create',
            'ctrl+5': '/Restock',
            'ctrl+6': '/Admin/Analytics'
        });
    }

    function toggleTheme() {
        const htmlElement = document.documentElement;
        const themeSwitch = document.getElementById("themeSwitch");
        const currentTheme = htmlElement.getAttribute("data-bs-theme") || "light";
        const newTheme = currentTheme === "light" ? "dark" : "light";

        htmlElement.setAttribute("data-bs-theme", newTheme);
        if (themeSwitch) {
            themeSwitch.checked = (newTheme === "dark");
        }
        localStorage.setItem("theme", newTheme);

        if (isAuthenticated) {
            saveThemeToServer(newTheme);
        }

        showThemeNotification(newTheme);
    }

    async function saveThemeToServer(theme) {
        try {
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
            await fetch("/UserSettings/UpdateTheme", {
                method: "POST",
                headers: {
                    "Content-Type": "application/x-www-form-urlencoded",
                    ...(token && { "RequestVerificationToken": token })
                },
                body: `theme=${theme}`
            });
        } catch (error) {
            console.error("Failed to update theme on server:", error);
        }
    }

    document.addEventListener('keydown', function(e) {
        let combination = '';
        if (e.ctrlKey) combination += 'ctrl+';
        if (e.altKey) combination += 'alt+';

        const key = e.key.toLowerCase();
        combination += key;

        if (hotkeys[combination]) {
            e.preventDefault();

            if (typeof hotkeys[combination] === 'function') {
                hotkeys[combination]();
            } else {
                window.location.href = hotkeys[combination];
            }
        }
    });

    setTimeout(showHotkeysHintAuto, 1000);
    addHotkeysButtonToNav();
    document.body.setAttribute('data-authenticated', isAuthenticated.toString());
});

function addHotkeysButtonToNav() {
    const nav = document.querySelector('.navbar-nav');
    if (nav && !document.querySelector('.hotkeys-nav-button')) {
        const hotkeysItem = document.createElement('li');
        hotkeysItem.className = 'nav-item hotkeys-nav-button';
        hotkeysItem.innerHTML = `
            <button class="btn btn-link nav-link" onclick="showHotkeysHintForced()" title="Горячие клавиши (Ctrl+?)">
                <i class="bi bi-keyboard"></i>
            </button>
        `;
        nav.appendChild(hotkeysItem);
    }
}

function clearUserSettingsOnLogout() {
    const logoutForms = document.querySelectorAll('form[action*="Logout"], form[action*="logout"]');

    logoutForms.forEach(form => {
        form.addEventListener('submit', function() {
            localStorage.removeItem('hotkeysHintClosed');
            localStorage.removeItem('theme');
            localStorage.removeItem('theme_synced');
            localStorage.removeItem('userSettings');
        });
    });
}

document.addEventListener('DOMContentLoaded', clearUserSettingsOnLogout);