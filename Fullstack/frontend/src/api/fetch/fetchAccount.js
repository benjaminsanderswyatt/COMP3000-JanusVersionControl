import fetchWithTokenRefresh from '../fetchWithTokenRefresh';

const API_URL = 'https://localhost:82/api/web/account';


export const uploadProfilePicture = async (file, sessionExpired) => {
  try {
    const formData = new FormData();
    formData.append("file", file);

    const responseJson = await fetchWithTokenRefresh(`${API_URL}/changeprofilepicture`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: FormData,
    }, sessionExpired);


    return { success: true, profilePictureUrl: responseJson.profilePictureUrl };

  } catch (error) {
    return { success: false, message: error.message };
  }
};

export const getProfilePicture = async (sessionExpired) => {
  try {
    const response = await fetchWithTokenRefresh(`${API_URL}/getprofilepicture`, {
      method: 'GET',
    }, sessionExpired);

    return response.profilePictureUrl || null;

  } catch (error){

    console.error("Error fetching profile picture", error);
    return null;
  }
};




