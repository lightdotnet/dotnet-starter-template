async function downloadBase64String(fileName, base64Content) {
    const binary = atob(base64Content);
    const array = new Uint8Array(binary.length);
    for (let i = 0; i < binary.length; i++) {
        array[i] = binary.charCodeAt(i);
    }
    const blob = new Blob([array], { type: "application/octet-stream" });
    const link = document.createElement("a");
    link.href = URL.createObjectURL(blob);
    link.download = fileName;
    link.click();
    URL.revokeObjectURL(link.href);
}

async function downloadStream(fileName, fileStream) {
    const arrayBuffer = await fileStream.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);

    triggerDownload(fileName, url);
    URL.revokeObjectURL(url);
}

function printCurrentPage() {
    window.print();
    window.onfocus = function () { setTimeout(function () { window.close(); }, 100); }
}

async function playAudio(playerName) {
    var audioPlayer = document.getElementById(playerName);

    if (audioPlayer == null)
        return;

    audioPlayer.play();
}

async function pauseAudio(playerName) {
    var audioPlayer = document.getElementById(playerName);

    if (audioPlayer == null)
        return;

    audioPlayer.pause();
}

async function requestNotificationPermission() {
    Notification.requestPermission().then(function (result) {
        if (result === 'granted') {
            console.log('Notification permission granted.');
        } else {
            console.log('Notification permission denied.');
        }
    });
}

function showNotification(title, options) {
    if (Notification.permission === 'granted') {
        new Notification(title, options);
    }
}