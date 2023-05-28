window.mtaTriggerEvent = (kind, csharpIdentifier, args) => {
    mta.triggerEvent(kind, csharpIdentifier, JSON.stringify(args));
};

window.mtaTriggerEventDebug = async (kind, csharpIdentifier, args) => {
    await fetch("http://localhost:22100/invokeVoidAsync", {
        method: "POST", mode: 'no-cors', body: JSON.stringify({
            kind: kind,
            csharpIdentifier: csharpIdentifier,
            args: JSON.stringify(args)
        })
    })
};
