const API_URL = 'https://localhost:82/api/web/users';


export async function register(username, email, password) {

  console.log(`fetch - user: ${username}, email: ${email}, password: ${password}`);

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



export const deleteUser = async () => {
  try {
    const token = localStorage.getItem('token');

    const response = await fetch(`${API_URL}/delete`, {
      method: 'DELETE',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${token}`,
      },
    });

    const responseJson = await response.json();

    if (!response.ok) {
      throw new Error(responseJson.message || "Failed to delete user");
    }

    return { success: true, message: responseJson.message };

  } catch (error) {
    return { success: false, message: error.message };
  }

};




export const refreshAccessToken = async () => {
  try {
    const response = await fetch(`${API_URL}/refresh`, {
      method: 'POST',
      credentials: 'include',  // Important to include the cookie in the request
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