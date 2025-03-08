import { fetchWithTokenRefresh } from '../fetchWithTokenRefresh';

const API_URL = 'https://localhost:82/api/AccessToken/GeneratePAT';


export async function GenAccessToken(ExpirationInHours, sessionExpired) {
    try {
        const responseJson = await fetchWithTokenRefresh(`${API_URL}`, {
            method: 'POST',
            headers: { 
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ ExpirationInHours }),
        }, sessionExpired);

    
        return {success: true, token: responseJson.token};
    
    } catch (error) {
        return { success: false, message: error.message };
    }
}