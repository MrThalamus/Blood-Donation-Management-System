// Sidebar behaviour.
//
// Desktop (>= 992px): the toggle collapses the sidebar to an icon-only rail and
//   back. The choice is remembered in localStorage so it survives navigation.
// Mobile  (< 992px):  the sidebar is off-canvas; the toggle slides it in over a
//   backdrop. The collapsed rail does not apply at this width.
(function () {
    'use strict';

    var DESKTOP_MIN = 992;
    var STORAGE_KEY = 'sidebarCollapsed';

    var sidebar = document.getElementById('appSidebar');
    var toggle = document.getElementById('sidebarToggle');

    if (!sidebar || !toggle) {
        return;
    }

    var backdrop = null;

    function isDesktop() {
        return window.innerWidth >= DESKTOP_MIN;
    }

    function closeOffCanvas() {
        sidebar.classList.remove('open');

        if (backdrop) {
            backdrop.remove();
            backdrop = null;
        }
    }

    function openOffCanvas() {
        sidebar.classList.add('open');

        backdrop = document.createElement('div');
        backdrop.className = 'sidebar-backdrop';
        backdrop.addEventListener('click', closeOffCanvas);
        document.body.appendChild(backdrop);
    }

    function toggleCollapsed() {
        var collapsed = document.documentElement.classList.toggle('sidebar-collapsed');

        try {
            localStorage.setItem(STORAGE_KEY, collapsed ? '1' : '0');
        } catch (e) { /* private mode / storage disabled */ }

        toggle.setAttribute('aria-expanded', collapsed ? 'false' : 'true');
    }

    toggle.addEventListener('click', function () {
        if (isDesktop()) {
            toggleCollapsed();
        } else if (sidebar.classList.contains('open')) {
            closeOffCanvas();
        } else {
            openOffCanvas();
        }
    });

    // Tapping a link on mobile navigates away; close the panel so it is not
    // still open behind the new page if navigation is slow.
    sidebar.addEventListener('click', function (e) {
        if (!isDesktop() && e.target.closest('a')) {
            closeOffCanvas();
        }
    });

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            closeOffCanvas();
        }
    });

    // Growing back to desktop width must not leave the mobile backdrop behind.
    window.addEventListener('resize', function () {
        if (isDesktop()) {
            closeOffCanvas();
        }
    });
})();
