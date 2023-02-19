export function initialiseFileDropZone(dragAndDropContainer, fileUploadControl) {
    const inputFile = fileUploadControl.querySelector("input");

    function onDragHover(e) {
        e.preventDefault();
        dragAndDropContainer.classList.add("hover");
    }

    function onDragLeave(e) {
        e.preventDefault();
        dragAndDropContainer.classList.remove("hover");
    }

    function onDrop(e) {
        e.preventDefault();
        dragAndDropContainer.classList.remove("hover");

        inputFile.files = e.dataTransfer.files;
        const event = new Event('change', { bubbles: true })
        inputFile.dispatchEvent(event);
    }

    function onPaste(e) {
        inputFile.files = e.clipboardData.files;
        const event = new Event('change', { bubbles: true });
        inputFile.dispatchEvent(event);
    }

    dragAndDropContainer.addEventListener("dragenter", onDragHover);
    dragAndDropContainer.addEventListener("dragover", onDragHover);
    dragAndDropContainer.addEventListener("dragleave", onDragLeave);
    dragAndDropContainer.addEventListener("drop", onDrop);
    dragAndDropContainer.addEventListener("paste", onPaste);

    return {
        dispose: () => {

            dragAndDropContainer.removeEventListener("dragenter", onDragHover);
            dragAndDropContainer.removeEventListener("dragover", onDragHover);
            dragAndDropContainer.removeEventListener("dragleave", onDragLeave);
            dragAndDropContainer.removeEventListener("drop", onDrop);
            dragAndDropContainer.removeEventListener("paste", onPaste);
        }
    }
}

export function clearUploadControl(fileUploadControl) {
    const inputFile = fileUploadControl.querySelector("input");
    inputFile.value = null;
    return inputFile;
}

export function log(data) {
    console.log(data);
}

