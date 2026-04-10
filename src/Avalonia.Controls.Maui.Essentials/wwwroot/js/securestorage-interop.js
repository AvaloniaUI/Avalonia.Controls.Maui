const PREFIX = "__avalonia_maui_securestorage_";
const DB_NAME = "Avalonia.Controls.Maui.Essentials";
const STORE_NAME = "SecureStorageKeys";
const KEY_ID = "aes-gcm-v1";
const textEncoder = new TextEncoder();
const textDecoder = new TextDecoder();

function toBase64(bytes) {
    let binary = "";
    for (const b of bytes) binary += String.fromCharCode(b);
    return btoa(binary);
}

function bytesFromBase64(encoded) {
    const binary = atob(encoded);
    const bytes = new Uint8Array(binary.length);
    for (let i = 0; i < binary.length; i++) bytes[i] = binary.charCodeAt(i);
    return bytes;
}

function stringFromBase64(encoded) {
    return textDecoder.decode(bytesFromBase64(encoded));
}

function requestToPromise(request) {
    return new Promise((resolve, reject) => {
        request.onsuccess = () => resolve(request.result);
        request.onerror = () => reject(request.error ?? new Error("IndexedDB request failed"));
    });
}

function transactionToPromise(transaction) {
    return new Promise((resolve, reject) => {
        transaction.oncomplete = () => resolve();
        transaction.onerror = () => reject(transaction.error ?? new Error("IndexedDB transaction failed"));
        transaction.onabort = () => reject(transaction.error ?? new Error("IndexedDB transaction aborted"));
    });
}

function openDb() {
    if (!globalThis.indexedDB) {
        throw new Error("IndexedDB is not available for secure storage.");
    }

    return new Promise((resolve, reject) => {
        const request = globalThis.indexedDB.open(DB_NAME, 1);
        request.onupgradeneeded = () => {
            const db = request.result;
            if (!db.objectStoreNames.contains(STORE_NAME)) {
                db.createObjectStore(STORE_NAME);
            }
        };
        request.onsuccess = () => resolve(request.result);
        request.onerror = () => reject(request.error ?? new Error("Failed to open IndexedDB."));
    });
}

async function getOrCreateKey() {
    if (!globalThis.crypto?.subtle) {
        throw new Error("Web Crypto API is not available for secure storage.");
    }

    const db = await openDb();
    try {
        const transaction = db.transaction(STORE_NAME, "readwrite");
        const store = transaction.objectStore(STORE_NAME);
        const existingKey = await requestToPromise(store.get(KEY_ID));
        if (existingKey) {
            await transactionToPromise(transaction);
            return existingKey;
        }

        const key = await globalThis.crypto.subtle.generateKey(
            { name: "AES-GCM", length: 256 },
            false,
            ["encrypt", "decrypt"]);

        await requestToPromise(store.put(key, KEY_ID));
        await transactionToPromise(transaction);
        return key;
    } finally {
        db.close();
    }
}

async function encryptValue(value) {
    const key = await getOrCreateKey();
    const iv = globalThis.crypto.getRandomValues(new Uint8Array(12));
    const plaintext = textEncoder.encode(value);
    const ciphertext = await globalThis.crypto.subtle.encrypt(
        { name: "AES-GCM", iv },
        key,
        plaintext);

    return JSON.stringify({
        v: 1,
        iv: toBase64(iv),
        ct: toBase64(new Uint8Array(ciphertext))
    });
}

async function decryptValue(raw) {
    const payload = JSON.parse(raw);
    if (payload?.v !== 1 || typeof payload.iv !== "string" || typeof payload.ct !== "string") {
        throw new Error("Invalid secure storage payload.");
    }

    const key = await getOrCreateKey();
    const plaintext = await globalThis.crypto.subtle.decrypt(
        { name: "AES-GCM", iv: bytesFromBase64(payload.iv) },
        key,
        bytesFromBase64(payload.ct));

    return textDecoder.decode(plaintext);
}

export async function getItem(key) {
    const storageKey = PREFIX + key;
    const raw = globalThis.localStorage.getItem(storageKey);
    if (raw === null) return null;

    try {
        return await decryptValue(raw);
    } catch {
        let legacyValue;
        try {
            legacyValue = stringFromBase64(raw);
        } catch {
            globalThis.localStorage.removeItem(storageKey);
            return null;
        }

        try {
            await setItem(key, legacyValue);
        } catch {
            return legacyValue;
        }

        return legacyValue;
    }
}

export async function setItem(key, value) {
    const encryptedValue = await encryptValue(value);
    globalThis.localStorage.setItem(PREFIX + key, encryptedValue);
}

export function removeItem(key) {
    const fullKey = PREFIX + key;
    if (globalThis.localStorage.getItem(fullKey) === null) return false;
    globalThis.localStorage.removeItem(fullKey);
    return true;
}

export function removeAll() {
    const keys = [];
    for (let i = 0; i < globalThis.localStorage.length; i++) {
        const k = globalThis.localStorage.key(i);
        if (k && k.startsWith(PREFIX)) keys.push(k);
    }
    keys.forEach(k => globalThis.localStorage.removeItem(k));
}
