const API_URL = 'https://localhost:82/api/web/users';


export async function register(username, email, password) {

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

}


export const login = async (email, password) => {

  try {
    const response = await fetch(`${API_URL}/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password }),
    });

    const responseJson = await response.json();

    if (!response.ok) {
      throw new Error(responseJson.message || "Failed to log in")
    }

    return {success: true, token: responseJson.token};

  } catch (error) {
    return { success: false, message: error.message };
  }
};