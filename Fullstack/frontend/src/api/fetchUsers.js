import { jwtDecode } from "jwt-decode";
import { useNavigate } from 'react';

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

    // Save the access token in localStorage
    localStorage.setItem('token', responseJson.token);
    return { success: true };
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


export const checkAndRefreshToken = async () => {
  const navigate = useNavigate();

  const token = localStorage.getItem('token');
  
  if (token) {
    const { exp } = jwtDecode(token); // Decode the JWT token for expiry
    
    // Check if token has expired
    if (Date.now() >= exp * 1000) {

      try {
        // Call the refresh endpoint to get a new token
        const response = await fetch(`${API_URL}/refresh`, {
          method: 'POST',
          credentials: 'include', // Include cookies (refresh token in HttpOnly cookie)
        });

        const responseJson = await response.json();

        if (response.ok) {
          // Save new access token in localStorage
          localStorage.setItem('token', responseJson.token);
        } else {
          // Handle refresh token fail
          localStorage.removeItem('token');
          navigate("/repositories", { replace: true }) // Redirect to login
        }


      } catch (error) {
        console.error('Failed to refresh token:', error);
        localStorage.removeItem('token');
        navigate("/repositories", { replace: true }); // Redirect to login
      }

    }

  }
};