var icon = document.getElementById('logo');

icon.addEventListener('click', changeIcon);

function changeIcon() {
  if (icon.src.includes('icon.png')) {
    icon.src = './assets/icon-aqua.png';
  } else {
    icon.src = './assets/icon.png';
  }

  localStorage.setItem('icon', icon.src);
}

document.addEventListener('DOMContentLoaded', function () {
  if (localStorage.getItem('icon')) {
    icon.src = localStorage.getItem('icon');
  }
});