export function requestWakeLock() {
    if ('wakeLock' in navigator) {
        return navigator.wakeLock.request('screen')
            .then(sentinel => sentinel)
            .catch(() => null);
    }
    return Promise.resolve(null);
}

export function releaseWakeLock(sentinel) {
    if (sentinel) {
        return sentinel.release();
    }
    return Promise.resolve();
}
