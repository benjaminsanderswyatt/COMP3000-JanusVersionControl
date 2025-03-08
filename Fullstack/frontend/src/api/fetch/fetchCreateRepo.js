import { fetchWithTokenRefresh } from '../fetchWithTokenRefresh';

const API_URL = 'https://localhost:82/api/web/repo/init';


export async function InitRepo(repoName, repoDescription, isPrivate, sessionExpired) {
try {
    const responseJson = await fetchWithTokenRefresh(API_URL, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        repoName,
        repoDescription,
        isPrivate,
      }),
    }, sessionExpired);


    return {success: true, token: responseJson.message};

  } catch (error) {

    console.error("Error initializing repository:", error);
    return { success: false, message: error.message };
  }
}