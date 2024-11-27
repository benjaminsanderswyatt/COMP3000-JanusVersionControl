const API_URL = 'http://localhost:5000/api/items';

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
