self.addEventListener('fetch', event => {
    console.log("SERVICE WORKER:", event);
});
console.log("SERVICE REGISTERED:");