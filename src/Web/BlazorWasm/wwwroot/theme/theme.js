async function expandMenu() {
    var ele = document.getElementsByClassName('mud-nav-link', 'mud-ripple', 'active');
    var c = ele[0].parentElement.getAttribute('aria-label');
    alert(c);
}