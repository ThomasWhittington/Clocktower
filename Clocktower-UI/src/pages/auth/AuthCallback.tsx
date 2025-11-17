import {
    useEffect
} from 'react';
import {
    discordService
} from "@/services";
import {
    useAppStore
} from "@/store";

const AuthCallback = () => {
    const {
        setJwt,
        setCurrentUser
    } = useAppStore.getState();

    useEffect(() => {
        const handleAuthCallback = async () => {
            const urlParams = new URLSearchParams(globalThis.location.search);
            const key = urlParams.get('key');
            const error = urlParams.get('error');

            if (error) {
                globalThis.location.href = '/login?error=' + encodeURIComponent(error);
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
                        globalThis.location.href = '/login?error=auth_data_failed';
                    } else {
                        setCurrentUser({
                            id: data?.gameUser?.id ?? '',
                            name: data?.gameUser?.name ?? '',
                            avatarUrl: data?.gameUser?.avatarUrl ?? ''
                        });
                        setJwt(data?.jwt ?? undefined);

                        globalThis.location.href = '/';
                    }
                } catch (error) {
                    console.error('Failed to get auth data:', error);
                    globalThis.location.href = '/login?error=auth_data_failed';
                }
            } else {
                globalThis.location.href = '/login?error=missing_key';
            }
        };

        handleAuthCallback().then(_ => {
        });
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