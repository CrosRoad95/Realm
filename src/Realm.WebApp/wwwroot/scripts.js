function scrollToEnd(element) {
    element.scrollTop = element.scrollHeight;
}

function scrollToEndById(id) {
    const element = document.querySelector(id);
    element.scrollTop = element.scrollHeight;
}

function focusElementById(id) {
    document.querySelector(id).focus();
}
