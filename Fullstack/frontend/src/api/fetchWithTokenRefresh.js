import { refreshAccessToken } from "./fetch/fetchUsers";

// sessionExpired prop needs to call AuthContext logout and then navigate to login page
export const fetchWithTokenRefresh = async (url, options = {}, sessionExpired) => {

  let token = localStorage.getItem('token');  // Get the access token from localStorage

  if (!token) {
    throw new Error('No access token available');
  }

  // Add the token to the Authorization header
  const headers = {
    ...options.headers,
    'Authorization': `Bearer ${token}`,
    credentials: 'include'
  };

  const response = await fetch(url, { ...options, headers });

  if (response.status === 401) { // Unauthorised
    // Token expired or invalid, try to refresh the token
    const newToken = await refreshAccessToken();

    if (newToken) {
      // Save the new token
      localStorage.setItem('token', newToken);

      // Retry the original request with the new token
      return fetch(url, { 
        ...options,
        headers: { ...headers, 'Authorization': `Bearer ${newToken}` }
       });
    } else {
      sessionExpired();
      throw new Error('Failed to refresh token');
    }
  }

  const responseJson = await response.json();


  if (!response.ok) {
    throw new Error(responseJson.message);
  }

  return responseJson;
};






export const fetchFileWithTokenRefresh = async (url, options = {}, sessionExpired) => {
  let token = localStorage.getItem('token');

  if (!token) {
    throw new Error('No access token available');
  }

  const headers = {
    ...options.headers,
    'Authorization': `Bearer ${token}`,
    credentials: 'include'
  };

  let response = await fetch(url, { ...options, headers });

  if (response.status === 401) {
    const newToken = await refreshAccessToken();

    if (newToken) {
      localStorage.setItem('token', newToken);
      headers['Authorization'] = `Bearer ${newToken}`;
      response = await fetch(url, { ...options, headers });
    } else {
      sessionExpired();
      throw new Error('Failed to refresh token');
    }
  }

  if (!response.ok) {
    let errorMessage;
    try {
      const errorJson = await response.json();
      errorMessage = errorJson.message || 'Request failed';
    } catch {
      const errorText = await response.text();
      errorMessage = errorText || 'Request failed';
    }
    throw new Error(errorMessage);
  }

  return response; // Return the raw response object
};