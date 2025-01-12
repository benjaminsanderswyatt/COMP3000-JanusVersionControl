const API_URL = 'https://localhost:82/api/AccessToken/GenPAT';


export async function GenAccessToken() {
    try {
        const response = await fetch(`${API_URL}`, {
            method: 'POST',
            headers: { 
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${localStorage.getItem("token")}`
                }
        });
        // Check if the response is JSON
        const contentType = response.headers.get("content-type");
        let responseJson = null;

        console.log("Full response: " + response.text);
        console.log("Full body: " + response.body);
        if (contentType && contentType.includes("application/json")) {
            responseJson = await response.json();
        }

        //const responseJson = await response.json();

        if (!response.ok) {
            throw new Error(responseJson.message || "Failed to generate access token.");
        }
    
        return {success: true, token: responseJson.token};
    
    } catch (error) {
        return { success: false, message: error.message };
    }
  }