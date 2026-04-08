let _callback = null;

export function isOnline() {
    return globalThis.navigator.onLine;
}

export function getConnectionType() {
    const conn = globalThis.navigator.connection ||
                 globalThis.navigator.mozConnection ||
                 globalThis.navigator.webkitConnection;
    return conn?.type ?? "unknown";
}

export function subscribe(callback) {
    _callback = callback;
    globalThis.addEventListener("online", onStatusChange);
    globalThis.addEventListener("offline", onStatusChange);

    const conn = globalThis.navigator.connection;
    if (conn) {
        conn.addEventListener("change", onStatusChange);
    }
}

export function unsubscribe() {
    globalThis.removeEventListener("online", onStatusChange);
    globalThis.removeEventListener("offline", onStatusChange);

    const conn = globalThis.navigator.connection;
    if (conn) {
        conn.removeEventListener("change", onStatusChange);
    }
    _callback = null;
}

function onStatusChange() {
    if (_callback) {
        _callback();
    }
}
