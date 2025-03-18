import { fetchWithTokenRefresh } from '../fetchWithTokenRefresh';

const API_URL = 'https://localhost:82/api/AccessToken';


export async function GenAccessToken(ExpirationInHours, sessionExpired) {
    try {
        const responseJson = await fetchWithTokenRefresh(`${API_URL}/GeneratePAT`, {
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


export async function RevokePAT(patToken, sessionExpired) {
    try {
        const responseJson = await fetchWithTokenRefresh(`${API_URL}/RevokePATFrontend`, {
            method: 'POST',
            headers: { 
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ PatToken: patToken }),

        },sessionExpired);
  
        return { success: true, message: responseJson.message };

    } catch (error) {
        return { success: false, message: error.message };
    }
}