export const API_BASE_URL = 'http://localhost:8000/api'

export async function getActiveLoans(userId) {
    if (!userId) {
        throw new Error('User ID cannot be empty')
    }

    const response = await fetch (`${API_BASE_URL}/users/${userId}/loans`, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
        }
    })

    if (response.status === 404)
    {
        return [];
    }

    if (!response.ok)
    {
        const errorData = await response.json().catch(() => ({message: 'Server error'}));
        throw new Error(errorData.error || response.statusText);
    }

    return response.json();
}