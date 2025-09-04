function hideArea(areaId) {
    var x = document.getElementById(areaId);
    if (x.style.display === "none") {
        x.style.display = "block";
    } else {
        x.style.display = "none";
    }
}