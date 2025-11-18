import {
    useEffect,
    useState
} from "react";
import {
    Spinner
} from "@/components/ui";

const ErrorPage = () => {
    const [error, setError] = useState<string>();
    useEffect(() => {
        const urlParams = new URLSearchParams(globalThis.location.search);
        const thisError = urlParams.get('error');
        setError(thisError ?? 'unknown error');
    }, []);

    return (
        <div
            className="error flex items-center justify-center min-h-screen">
            <div
                className="text-center">
                <Spinner
                    className="mx-auto justify-items-center"/>
                <p>{error}</p>
            </div>
        </div>
    );
}
export default ErrorPage;