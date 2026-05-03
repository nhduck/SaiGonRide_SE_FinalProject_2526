(function () {
    'use strict';

    // Cache
    const STORAGE_KEY = 'sr_lang';
    const DEFAULT_LANG = 'en';
    let _cache = {};
    let _currentLang = DEFAULT_LANG;

    // Helpers

    /** Fetch a language JSON file. */
    async function loadLang(code) {
        if (_cache[code]) return _cache[code];
        try {
            const resp = await fetch(`/js/lang/${code}.json?v=${Date.now()}`);
            if (!resp.ok) throw new Error(resp.statusText);
            const data = await resp.json();
            _cache[code] = data;
            return data;
        } catch (err) {
            console.warn(`[i18n] Could not load language "${code}":`, err);
            return null;
        }
    }

    /** Apply translations to the DOM. */
    function applyTranslations(dict) {
        if (!dict) return;

        // Unified processor for data-i18n
        document.querySelectorAll('[data-i18n]').forEach(el => {
            const raw = el.getAttribute('data-i18n');
            let key = raw;
            let attr = null;

            // Check for [attr]key syntax
            const match = raw.match(/^\[(.*)\](.*)$/);
            if (match) {
                attr = match[1];
                key = match[2];
            }

            if (dict[key] !== undefined) {
                let text = dict[key];

                // Check for parameters {0}, {1}
                const p1 = el.getAttribute('data-i18n-param1');
                const p2 = el.getAttribute('data-i18n-param2');
                if (p1 !== null) text = text.replace('{0}', p1);
                if (p2 !== null) text = text.replace('{1}', p2);

                if (attr) {
                    el.setAttribute(attr, text);
                } else {
                    // textContent (preserving icons)
                    const icon = el.querySelector('i, svg');
                    if (icon) {
                        const nodes = el.childNodes;
                        let replaced = false;
                        for (let i = nodes.length - 1; i >= 0; i--) {
                            if (nodes[i].nodeType === Node.TEXT_NODE && nodes[i].textContent.trim()) {
                                nodes[i].textContent = text;
                                replaced = true;
                                break;
                            }
                        }
                        if (!replaced) el.append(document.createTextNode(text));
                    } else {
                        el.textContent = text;
                    }
                }
            }
        });

        // Backward compatibility / legacy attributes
        document.querySelectorAll('[data-i18n-placeholder]').forEach(el => {
            const key = el.getAttribute('data-i18n-placeholder');
            if (dict[key] !== undefined) el.placeholder = dict[key];
        });
        document.querySelectorAll('[data-i18n-title]').forEach(el => {
            const key = el.getAttribute('data-i18n-title');
            if (dict[key] !== undefined) el.title = dict[key];
        });
        document.querySelectorAll('[data-i18n-html]').forEach(el => {
            const key = el.getAttribute('data-i18n-html');
            if (dict[key] !== undefined) el.innerHTML = dict[key];
        });
    }

    /** Language button label to show the active language flag/name. */
    function updateLangButton(code) {
        const btns = [
            document.getElementById('langDropdownBtn'),
            document.getElementById('adminLangDropdownBtn')
        ];

        const flags = {
            'en': '🇬🇧',
            'vi': '🇻🇳'
        };

        const names = {
            'en': 'English',
            'vi': 'Tiếng Việt'
        };

        const flag = flags[code] || '🌐';
        const name = names[code] || code.toUpperCase();

        btns.forEach(btn => {
            if (!btn) return;
            const spanEl = btn.querySelector('span');
            if (spanEl) {
                spanEl.textContent = `${flag} ${name}`;
            }
        });

        // Mark active item in dropdown
        document.querySelectorAll('.sr-lang-item').forEach(item => {
            item.classList.remove('active', 'fw-semibold');
            if (item.getAttribute('data-lang') === code) {
                item.classList.add('active', 'fw-semibold');
            }
        });
    }

    // Public API

    /**
     * Switch the page to the given language code.
     */
    async function changeLanguage(code) {
        const dict = await loadLang(code);
        if (!dict) {
            console.warn(`[i18n] Translation file for "${code}" not found, falling back to ${DEFAULT_LANG}`);
            return;
        }
        _currentLang = code;
        localStorage.setItem(STORAGE_KEY, code);
        document.documentElement.lang = code;
        applyTranslations(dict);
        updateLangButton(code);
    }

    /** Get current translation dictionary. */
    function getDict() {
        return _cache[_currentLang] || {};
    }

    /** Get a translated string by key. */
    function getI18nText(key) {
        const dict = getDict();
        return dict[key] || key;
    }

    /** Get the current active language code. */
    function getCurrentLang() {
        return _currentLang;
    }

    // Initialise on DOM ready
    function init() {
        const saved = localStorage.getItem(STORAGE_KEY) || DEFAULT_LANG;
        changeLanguage(saved);
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    // Expose globally
    window.changeLanguage = changeLanguage;
    window.getCurrentLang = getCurrentLang;
    window.getI18nText = getI18nText;
})();
