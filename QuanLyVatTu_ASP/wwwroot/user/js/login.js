const container = document.getElementById("container");
const registerBtn = document.getElementById("register");
const loginBtn = document.getElementById("login");

registerBtn.addEventListener("click", () => {
  container.classList.add("active");
});

loginBtn.addEventListener("click", () => {
  container.classList.remove("active");
});

// Check URL params
const urlParams = new URLSearchParams(window.location.search);
if (urlParams.get("show") === "register") {
  container.classList.add("active");
}
