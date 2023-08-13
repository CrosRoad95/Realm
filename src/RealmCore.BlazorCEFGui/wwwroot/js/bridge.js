let invokesCounter = 0;
const pendingInvokes = new Map();

window.mtaTriggerEvent = (kind, csharpIdentifier, args) => {
    mta.triggerEvent(kind, csharpIdentifier, JSON.stringify(args));
};

window.mtaTriggerEventDebug = async (kind, csharpIdentifier, args) => {
    await fetch("http://localhost:22100/invokeVoidAsync", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            kind: kind,
            csharpIdentifier: csharpIdentifier,
            args: JSON.stringify(args)
        })
    })
};

function createPromiseWrapper() {
    let resolve, reject;
    const promise = new Promise((innerResolve, innerReject) => {
        resolve = innerResolve;
        reject = innerReject;
    });

    return {
        promise,
        resolve,
        reject
    };
}

window.mtaTriggerEventWithResult = async (kind, csharpIdentifier, args) => {
    const promiseWrapper = createPromiseWrapper();
    pendingInvokes[invokesCounter] = promiseWrapper;

    mta.triggerEvent(kind, csharpIdentifier, invokesCounter, JSON.stringify(args));
    invokesCounter++;
    return await promiseWrapper.promise;
};

window.mtaTriggerEventWithResultDebug = async (kind, csharpIdentifier, args) => {
    var response = await fetch("http://localhost:22100/invokeAsync", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            kind: kind,
            csharpIdentifier: csharpIdentifier,
            args: JSON.stringify(args)
        })
    })
    return await response.text();
};

const invokeAsyncSuccess = (promiseId, data) => {
    pendingInvokes[promiseId].resolve(data);
};

const invokeAsyncError = (promiseId, reason) => {
    pendingInvokes[promiseId].reject(reason);
};

function registerCSharpFunction(cSharpApp) {
    window.app = cSharpApp;
}

const navigate = (path, force) => {
    window.app.invokeMethod("NavigateTo", path, force)
};
