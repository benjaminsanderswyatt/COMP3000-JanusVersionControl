const API_URL = 'https://localhost:5000/api/items';


/*
// Example
const fetchData = async () => {
    const token = localStorage.getItem('token');

    const response = await fetch('URI', {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${token}`
        }
    });

    if (response.ok) {
        const data = await response.json();
        console.log(data);
    } else {
        console.error('Failed to fetch data');
    }
};

  try {
    const response = await fetch(`${API_URL}/register`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username, email, password }),
    });

    const responseJson = await response.json();

    if (!response.ok) {
      throw new Error(responseJson.message || "Registration failed")
    }

    return responseJson;

  } catch (error) {
    return {success: false, message: error.message};
  }

*/







export const fetchItems = async () => {
  try {
    const response = await fetch(API_URL);
    if (!response.ok) {
      throw new Error('Failed to fetch items');
    }
    return await response.json();
  } catch (error) {
    console.error(error);
    return [];
  }
};

export const addItem = async (name) => {
  try {
    const response = await fetch(API_URL, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ name }),
    });
    if (!response.ok) {
      throw new Error('Failed to add item');
    }
    return await response.json();
  } catch (error) {
    console.error(error);
    return null;
  }
};
