let _voicesLoaded = false;
let _currentUtterance = null;

export async function getVoicesJson() {
    const synth = globalThis.speechSynthesis;
    if (!synth) return "[]";

    let voices = synth.getVoices();
    if (voices.length === 0) {
        // Chromium loads voices asynchronously
        await new Promise((resolve) => {
            const onVoicesChanged = () => {
                synth.removeEventListener("voiceschanged", onVoicesChanged);
                resolve();
            };
            synth.addEventListener("voiceschanged", onVoicesChanged);
            // Timeout after 2s in case voiceschanged never fires (Firefox)
            setTimeout(resolve, 2000);
        });
        voices = synth.getVoices();
    }
    _voicesLoaded = true;

    return JSON.stringify(voices.map((v) => ({
        name: v.name,
        lang: v.lang,
        voiceURI: v.voiceURI,
        isDefault: v.default,
    })));
}

export function speak(text, lang, voiceId, pitch, rate, volume) {
    return new Promise((resolve, reject) => {
        const synth = globalThis.speechSynthesis;
        if (!synth) {
            reject(new Error("SpeechSynthesis not supported"));
            return;
        }

        // Cancel any ongoing speech
        synth.cancel();

        const utterance = new SpeechSynthesisUtterance(text);

        if (voiceId) {
            const voices = synth.getVoices();
            const match = voices.find((v) => v.voiceURI === voiceId || v.name === voiceId);
            if (match) utterance.voice = match;
        }

        if (lang) utterance.lang = lang;
        if (pitch >= 0) utterance.pitch = pitch;
        if (rate >= 0) utterance.rate = rate;
        if (volume >= 0) utterance.volume = volume;

        _currentUtterance = utterance;

        utterance.onend = () => {
            _currentUtterance = null;
            resolve();
        };

        utterance.onerror = (event) => {
            _currentUtterance = null;
            if (event.error === "canceled" || event.error === "interrupted") {
                resolve(); // Treat cancellation as normal completion
            } else {
                reject(new Error(event.error || "Speech synthesis error"));
            }
        };

        synth.speak(utterance);
    });
}

export function cancelSpeech() {
    const synth = globalThis.speechSynthesis;
    if (synth) {
        synth.cancel();
    }
    _currentUtterance = null;
}
