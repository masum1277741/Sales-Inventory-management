// ── CLOZEY ERP — Service Worker ────────────────────────────────────────────
const APP_VERSION = 'v1.0.0';
const CACHE_STATIC = `clozey-static-${APP_VERSION}`;
const CACHE_PAGES = `clozey-pages-${APP_VERSION}`;
const OFFLINE_URL = '/offline.html';

// Static assets to precache (CSS, JS, fonts, icons)
const PRECACHE_ASSETS = [
    OFFLINE_URL,
    '/manifest.json',
    '/icons/icon-192.png',
    '/icons/icon-512.png',
    // Bootstrap & plugins (CDN fallback)
    '/css/site.css'
];

// ── Install ────────────────────────────────────────────────────────────────
self.addEventListener('install', event => {
    console.log('[SW] Installing...');
    event.waitUntil(
        caches.open(CACHE_STATIC)
            .then(cache => cache.addAll(PRECACHE_ASSETS).catch(e => console.warn('[SW] Precache failed:', e)))
            .then(() => self.skipWaiting())
    );
});


self.addEventListener('activate', event => {
    console.log('[SW] Activating...');
    event.waitUntil(
        caches.keys().then(keys =>
            Promise.all(
                keys
                    .filter(k => k !== CACHE_STATIC && k !== CACHE_PAGES)
                    .map(k => {
                        console.log('[SW] Deleting old cache:', k);
                        return caches.delete(k);
                    })
            )
        ).then(() => self.clients.claim())
    );
});

// ── Fetch ──────────────────────────────────────────────────────────────────
self.addEventListener('fetch', event => {
    const { request } = event;
    const url = new URL(request.url);

    
    if (url.origin !== self.location.origin) return;

    // POST/PUT/DELETE — always network (no cache)
    if (request.method !== 'GET') return;

    // Static assets → Cache First
    if (isStaticAsset(url.pathname)) {
        event.respondWith(cacheFirst(request, CACHE_STATIC));
        return;
    }


    if (request.headers.get('Accept')?.includes('text/html')) {
        event.respondWith(networkFirstWithOfflineFallback(request));
        return;
    }

    // API/JSON → Network Only
    if (url.pathname.startsWith('/api/') ||
        request.headers.get('Accept')?.includes('application/json')) {
        return;
    }

    // Default → Network First
    event.respondWith(networkFirst(request, CACHE_STATIC));
});

// ── Strategy: Cache First ──────────────────────────────────────────────────
async function cacheFirst(request, cacheName) {
    const cached = await caches.match(request);
    if (cached) return cached;

    try {
        const response = await fetch(request);
        if (response.ok) {
            const cache = await caches.open(cacheName);
            cache.put(request, response.clone());
        }
        return response;
    } catch {
        return new Response('Asset not available offline', { status: 503 });
    }
}

// ── Strategy: Network First ────────────────────────────────────────────────
async function networkFirst(request, cacheName) {
    try {
        const response = await fetch(request);
        if (response.ok) {
            const cache = await caches.open(cacheName);
            cache.put(request, response.clone());
        }
        return response;
    } catch {
        const cached = await caches.match(request);
        return cached || new Response('Not available offline', { status: 503 });
    }
}

// ── Strategy: Network First + Offline fallback page ────────────────────────
async function networkFirstWithOfflineFallback(request) {
    try {
        const response = await fetch(request);
        return response;
    } catch {
        const cached = await caches.match(request);
        if (cached) return cached;

        // Show offline page
        const offlinePage = await caches.match(OFFLINE_URL);
        return offlinePage || new Response(
            '<h1>You are offline</h1><p>Please check your connection.</p>',
            { headers: { 'Content-Type': 'text/html' } }
        );
    }
}

function isStaticAsset(pathname) {
    return pathname.startsWith('/css/') ||
        pathname.startsWith('/js/') ||
        pathname.startsWith('/lib/') ||
        pathname.startsWith('/icons/') ||
        pathname.startsWith('/logo/') ||
        pathname.startsWith('/uploads/') ||
        pathname.endsWith('.css') ||
        pathname.endsWith('.js') ||
        pathname.endsWith('.png') ||
        pathname.endsWith('.jpg') ||
        pathname.endsWith('.webp') ||
        pathname.endsWith('.woff2') ||
        pathname.endsWith('.ico');
}

// ── Listen for skip waiting message ────────────────────────────────────────
self.addEventListener('message', event => {
    if (event.data?.type === 'SKIP_WAITING') self.skipWaiting();
});