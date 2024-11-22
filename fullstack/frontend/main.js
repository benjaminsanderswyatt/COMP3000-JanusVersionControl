
import { register, login } from './api/auth.js';

import { getProducts, createProduct } from './api/products.js';

getProducts().then(products => {
    console.log(products); // Verify it works
});


document.addEventListener('DOMContentLoaded', () => {
  const loginLink = document.getElementById('loginLink');
  const registerLink = document.getElementById('registerLink');
  const content = document.getElementById('content');

  loginLink.addEventListener('click', () => renderLogin(content));
  registerLink.addEventListener('click', () => renderRegister(content));
});


function renderLogin(container) {
  container.innerHTML = `
    <h2>Login</h2>
    <form id="loginForm">
      <label for="email">Email:</label>
      <input type="email" id="email" required>
      <label for="password">Password:</label>
      <input type="password" id="password" required>
      <button type="submit">Login</button>
    </form>
  `;


  document.getElementById('loginForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    const email = document.getElementById('email').value;
    const password = document.getElementById('password').value;
    const response = await login(email, password);
    if (response.token) {
      alert('Login successful!');
    } else {
      alert('Login failed!');
    }
  });
}


function renderRegister(container) {
  container.innerHTML = `
    <h2>Register</h2>
    <form id="registerForm">
      <label for="username">Username:</label>
      <input type="text" id="username" required>
      <label for="email">Email:</label>
      <input type="email" id="email" required>
      <label for="password">Password:</label>
      <input type="password" id="password" required>
      <button type="submit">Register</button>
    </form>
  `;


  document.getElementById('registerForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    const username = document.getElementById('username').value;
    const email = document.getElementById('email').value;
    const password = document.getElementById('password').value;
    const response = await register(username, email, password);
    if (response.message === 'User registered successfully') {
      alert('Registration successful!');
    } else {
      alert('Registration failed!');
    }
  });
}