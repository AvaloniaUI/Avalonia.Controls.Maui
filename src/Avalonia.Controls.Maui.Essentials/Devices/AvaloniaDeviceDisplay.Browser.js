export function requestWakeLock() {
    if ('wakeLock' in navigator) {
        console.log("Requesting Screen Wake Lock...");
        return navigator.wakeLock.request('screen')
            .then(sentinel => {
                console.log("Screen Wake Lock acquired.");
                return sentinel;
            })
            .catch(err => {
                console.error(`Failed to acquire Screen Wake Lock: ${err.message}`);
                return null;
            });
    } else {
        console.warn("Screen Wake Lock API is not supported in this browser.");
    }
    return Promise.resolve(null);
}

export function releaseWakeLock(sentinel) {
    if (sentinel) {
        console.log("Releasing Screen Wake Lock...");
        return sentinel.release()
            .then(() => {
                console.log("Screen Wake Lock released.");
            });
    }
    return Promise.resolve();
}
