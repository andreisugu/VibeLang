// Profile pictures live in IndexedDB; the user row stores only "idb:<id>".
(function () {
    const dbName = 'vibelang-demo';
    const store = 'images';

    function open() {
        return new Promise(function (resolve, reject) {
            const req = indexedDB.open(dbName, 1);
            req.onupgradeneeded = function () {
                const db = req.result;
                if (!db.objectStoreNames.contains(store)) db.createObjectStore(store);
            };
            req.onsuccess = function () { resolve(req.result); };
            req.onerror = function () { reject(req.error); };
        });
    }

    window.vibelangImages = {
        put: async function (id, dataUrl) {
            const db = await open();
            return new Promise(function (resolve, reject) {
                const tx = db.transaction(store, 'readwrite');
                tx.objectStore(store).put(dataUrl, id);
                tx.oncomplete = function () { resolve(); };
                tx.onerror = function () { reject(tx.error); };
            });
        },
        get: async function (id) {
            const db = await open();
            return new Promise(function (resolve, reject) {
                const tx = db.transaction(store, 'readonly');
                const req = tx.objectStore(store).get(id);
                req.onsuccess = function () { resolve(req.result || null); };
                req.onerror = function () { reject(req.error); };
            });
        },
        remove: async function (id) {
            const db = await open();
            return new Promise(function (resolve, reject) {
                const tx = db.transaction(store, 'readwrite');
                tx.objectStore(store).delete(id);
                tx.oncomplete = function () { resolve(); };
                tx.onerror = function () { reject(tx.error); };
            });
        }
    };
})();
