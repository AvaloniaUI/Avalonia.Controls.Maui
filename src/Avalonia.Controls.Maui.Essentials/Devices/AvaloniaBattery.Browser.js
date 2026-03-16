let battery = null;
let batteryCallback = null;

export async function initBattery() {
    if ('getBattery' in navigator) {
        battery = await navigator.getBattery();
        return true;
    }
    return false;
}

export function getBatteryLevel() {
    return battery ? battery.level : 1.0;
}

export function getBatteryCharging() {
    return battery ? battery.charging : false;
}

export function subscribeBatteryEvents(callback) {
    batteryCallback = callback;
    if (battery) {
        battery.addEventListener('levelchange', () => {
            batteryCallback();
        });
        battery.addEventListener('chargingchange', () => {
            batteryCallback();
        });
    }
}
