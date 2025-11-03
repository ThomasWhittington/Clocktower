import {
    useEffect,
    useState
} from 'react';
import {
    type AuthState
} from '../types/auth';
import {
    authService
} from '../services';

export const useAuth = (): AuthState & {
    login: () => void;
    logout: () => void
} => {
    const [authState, setAuthState] = useState<AuthState>({
        user: null,
        isAuthenticated: false,
        isLoading: true
    });

    useEffect(() => {
        const user = authService.getUser();
        setAuthState({
            user,
            isAuthenticated: !!user,
            isLoading: false
        });
    }, []);

    const login = () => {
        authService.initiateDiscordLogin();
    };

    const logout = () => {
        authService.clearUser();
        setAuthState({
            user: null,
            isAuthenticated: false,
            isLoading: false
        });
    };

    return {
        ...authState,
        login,
        logout
    };
};