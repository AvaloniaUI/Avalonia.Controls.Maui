export function echo(value) {
    return `echo:${value}`;
}

export function callback(dotNetObjectReference, value) {
    return dotNetObjectReference.invokeMethodAsync('ReceiveFromJavaScript', value);
}
