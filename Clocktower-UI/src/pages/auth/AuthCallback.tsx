import {
    useEffect
} from 'react';
import {
    discordService
} from "../../services";

const AuthCallback = () => {
    useEffect(() => {
        const handleAuthCallback = async () => {
            const urlParams = new URLSearchParams(window.location.search);
            const key = urlParams.get('key');
            const error = urlParams.get('error');

            if (error) {
                console.error('Auth failed:', decodeURIComponent(error));
                window.location.href = '/login?error=' + encodeURIComponent(error);
                return;
            }

            if (key) {
                try {
                    const {
                        data,
                        error
                    } = await discordService.getAuthData(key);

                    if (error) {
                        console.error('Failed to get auth data:', error);
                        window.location.href = '/login?error=auth_data_failed';
                    } else {
                        localStorage.setItem('user', JSON.stringify(data));
                        window.location.href = '/';
                    }
                } catch (error) {
                    console.error('Failed to get auth data:', error);
                    window.location.href = '/login?error=auth_data_failed';
                }
            } else {
                window.location.href = '/login?error=missing_key';
            }
        };

        handleAuthCallback();
    }, []);

    return (
        <div
            className="flex items-center justify-center min-h-screen">
            <div
                className="text-center">
                <div
                    className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500 mx-auto mb-4"></div>
                <p>Processing authentication...</p>
            </div>
        </div>
    );
};

export default AuthCallback;