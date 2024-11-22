const BASE_URL = 'http://localhost:82/api/products';
let token = '';

export function setToken(newToken) {
  token = newToken;
}

export async function getProducts() {
  const response = await fetch(BASE_URL, {
    headers: { Authorization: `Bearer ${token}` },
  });
  return response.json();
}

export async function createProduct(name, price, description) {
  const response = await fetch(BASE_URL, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify({ name, price, description }),
  });
  return response.json();
}