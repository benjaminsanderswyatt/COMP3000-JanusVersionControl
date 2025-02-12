import fetchWithTokenRefresh from '../fetchWithTokenRefresh';

const API_URL = 'https://localhost:82/api/web/account';


export const uploadProfilePicture = async (file, sessionExpired) => {
  try {
    const formData = new FormData();
    formData.append("image", file);

    const responseJson = await fetchWithTokenRefresh(`${API_URL}/changeprofilepicture`, {
      method: 'POST',
      body: formData,
    }, sessionExpired);


    return { success: true, profilePictureUrl: responseJson.profilePictureUrl };

  } catch (error) {
    return { success: false, message: error.message };
  }
};
