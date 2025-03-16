import { fetchWithTokenRefresh } from '../fetchWithTokenRefresh';

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

    return {success: true, message: responseJson};

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
      credentials: 'include',
    });

    const responseJson = await response.json();

    if (!response.ok) {
      throw new Error(responseJson.message || "Failed to log in");
    }

    return { success: true, token: responseJson.token};

  } catch (error) {

    return { success: false, message: error.message };
  }
};


export const refreshAccessToken = async () => {
  try {
    const response = await fetch(`${API_URL}/refresh`, {
      method: 'POST',
      credentials: 'include',
    });

    if (!response.ok) {
      throw new Error("Failed to refresh token");
    }

    const responseJson = await response.json();
    return responseJson.token;  // New access token
  } catch (error) {
    console.error("Refresh token error:", error);
    return null;
  }
};

// Handles deleting the refresh token stored in db
export const logout = async () => {
  try {
    const response = await fetch(`${API_URL}/logout`, {
      method: 'POST',
      credentials: 'include',
    });

    if (!response.ok) {
      throw new Error("Failed to delete refresh token");
    }

    const responseJson = await response.json();

    return {success: true, message: responseJson.message};
    
  } catch (error) {
    console.error("Logout error:", error);
    return {success: false, message: error.message};
  }
}


// ---------------- Protected fetch ----------------

export const deleteUser = async (sessionExpired) => {
  try {
    const token = localStorage.getItem('token');

    // Use fetchWithTokenRefresh instead of direct fetch
    const responseJson = await fetchWithTokenRefresh(`https://localhost:82/api/web/account/delete`, {
      method: 'DELETE',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${token}`,
      },
    }, sessionExpired); // Pass sessionExpired callback to handle token refresh failure


    return { success: true, message: responseJson.message };

  } catch (error) {
    return { success: false, message: error.message };
  }
};


