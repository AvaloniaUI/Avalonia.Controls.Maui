let _voicesLoaded = false;
let _currentUtterance = null;
let _cancelRequested = false;

async function waitForVoicesAsync(synth, timeoutMs = 5000) {
    let voices = synth.getVoices();
    if (voices.length > 0) {
        _voicesLoaded = true;
        return voices;
    }

    await new Promise((resolve) => {
        let resolved = false;
        const finish = () => {
            if (resolved) return;
            resolved = true;
            synth.removeEventListener("voiceschanged", onVoicesChanged);
            clearInterval(pollId);
            clearTimeout(timeoutId);
            resolve();
        };

        const onVoicesChanged = () => {
            if (synth.getVoices().length > 0) {
                finish();
            }
        };

        const pollId = setInterval(() => {
            if (synth.getVoices().length > 0) {
                finish();
            }
        }, 100);

        const timeoutId = setTimeout(finish, timeoutMs);

        synth.addEventListener("voiceschanged", onVoicesChanged);
    });

    voices = synth.getVoices();
    _voicesLoaded = voices.length > 0;
    return voices;
}

export async function getVoicesJson() {
    const synth = globalThis.speechSynthesis;
    if (!synth) return "[]";

    const voices = await waitForVoicesAsync(synth);

    return JSON.stringify(voices.map((v) => ({
        name: v.name,
        lang: v.lang,
        voiceURI: v.voiceURI,
        isDefault: v.default,
    })));
}

export async function speak(text, lang, voiceId, pitch, rate, volume) {
    const synth = globalThis.speechSynthesis;
    if (!synth) {
        throw new Error("SpeechSynthesis not supported");
    }

    const voices = await waitForVoicesAsync(synth);
    _cancelRequested = false;

    return await new Promise((resolve, reject) => {
        const synth = globalThis.speechSynthesis;
        let settled = false;
        let started = false;
        let startWatchdogId = 0;

        const utterance = new SpeechSynthesisUtterance(text);

        if (voiceId) {
            const match = voices.find((v) => v.voiceURI === voiceId || v.name === voiceId);
            if (match) utterance.voice = match;
        }
        else if (lang) {
            const normalizedLang = lang.toLowerCase();
            const match = voices.find((v) => (v.lang || "").toLowerCase() === normalizedLang)
                || voices.find((v) => (v.lang || "").toLowerCase().startsWith(`${normalizedLang}-`))
                || voices.find((v) => (v.lang || "").toLowerCase().startsWith(normalizedLang.split("-")[0]));
            if (match) utterance.voice = match;
        }
        else {
            const defaultVoice = voices.find((v) => v.default) || voices[0];
            if (defaultVoice) utterance.voice = defaultVoice;
        }

        if (lang) utterance.lang = lang;
        if (pitch >= 0) utterance.pitch = pitch;
        if (rate >= 0) utterance.rate = rate;
        if (volume >= 0) utterance.volume = volume;

        _currentUtterance = utterance;

        const resolveOnce = () => {
            if (settled) return;
            settled = true;
            clearTimeout(startWatchdogId);
            _cancelRequested = false;
            resolve();
        };

        const rejectOnce = (error) => {
            if (settled) return;
            settled = true;
            clearTimeout(startWatchdogId);
            _cancelRequested = false;
            reject(error);
        };

        utterance.onstart = () => {
            started = true;
        };

        utterance.onend = () => {
            _currentUtterance = null;
            resolveOnce();
        };

        utterance.onerror = (event) => {
            _currentUtterance = null;
            if ((event.error === "canceled" || event.error === "interrupted") && _cancelRequested) {
                resolveOnce();
            } else {
                rejectOnce(new Error(event.error || "Speech synthesis error"));
            }
        };

        startWatchdogId = globalThis.setTimeout(() => {
            if (!started) {
                synth.cancel();
                _currentUtterance = null;
                rejectOnce(new Error("Speech synthesis did not start"));
            }
        }, 3000);

        if (synth.speaking || synth.pending) {
            synth.cancel();
        }

        if (synth.paused) {
            synth.resume();
        }

        globalThis.setTimeout(() => {
            try {
                synth.speak(utterance);
                if (synth.paused) {
                    synth.resume();
                }
            } catch (error) {
                _currentUtterance = null;
                rejectOnce(error instanceof Error ? error : new Error(String(error)));
            }
        }, 0);
    });
}

export function cancelSpeech() {
    const synth = globalThis.speechSynthesis;
    _cancelRequested = true;
    if (synth) {
        synth.cancel();
    }
    _currentUtterance = null;
}
