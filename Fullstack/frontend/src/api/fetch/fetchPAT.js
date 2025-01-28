import fetchWithTokenRefresh from '../fetchWithTokenRefresh';

const API_URL = 'https://localhost:82/api/AccessToken/GeneratePAT';


export async function GenAccessToken(ExpirationInHours, sessionExpired) {
    try {
        const response = await fetchWithTokenRefresh(`${API_URL}`, {
            method: 'POST',
            headers: { 
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ ExpirationInHours }),
        }, sessionExpired);

        //const responseJson = await response.json();
        const responseText = await response.text();
        console.log("Response text:", responseText);

        if (!responseText) {
            throw new Error("The server returned an empty response.");
        }

        // Parse JSON
        const responseJson = JSON.parse(responseText);

        if (!response.ok) {
            throw new Error(responseJson.message || "GFailed to generate access token.");
        }

        if (!response.ok) {
            throw new Error(responseJson.message || "Failed to generate access token.");
        }
    
        return {success: true, token: responseJson.token};
    
    } catch (error) {
        return { success: false, message: "hello" + error.message };
    }
}