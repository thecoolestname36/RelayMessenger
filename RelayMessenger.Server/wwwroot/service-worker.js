self.addEventListener('install', async event => {
    console.log('Installing service worker...');
    await self.skipWaiting();
});

self.addEventListener('activate', event => {
    event.waitUntil(self.clients.claim()); // Makes the service worker take control of the page immediately
});

// Handle Periodic Sync
self.addEventListener('periodicsync', event => {
    if (event.tag === 'messenger-sync') {
        event.waitUntil(syncData());
    }
});

async function syncData() {
    try {
        const response = await fetch('/api/new-data');
        debugger;
        const data = await response.json();

        if (data && data.newData) {
            const options = {
                body: 'New message',
                icon: '/favicon.png',
                tag: 'new-message-notification',
            };

            self.registration.showNotification('New Message', options);
        }
    } catch (error) {
        console.error('Error fetching new data:', error);
    }
}

// Handle notification click event
self.addEventListener('notificationclick', event => {
    event.notification.close();
    event.waitUntil(clients.openWindow('/'));
});
