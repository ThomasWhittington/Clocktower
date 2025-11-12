import {
    useEffect,
    useState
} from "react";

const ErrorPage = () => {
    const [error, setError] = useState<string>();
    useEffect(() => {
        const urlParams = new URLSearchParams(window.location.search);
        const thisError = urlParams.get('error');
        setError(thisError ?? 'unknown error');
    }, []);

    return (
        <div
            className="flex items-center justify-center min-h-screen">
            <div
                className="text-center">
                <div
                    className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500 mx-auto mb-4"></div>
                <p>{error}</p>
            </div>
        </div>
    );
}
export default ErrorPage;