import {
    useEffect,
    useState
} from "react";
import Spinner
    from "./Spinner.tsx";

function GameManager() {
    const [isLoading, setIsLoading] = useState(false);
    const [dummyData, setDummyData] = useState();
    const loadDummyData = async () => {
        setIsLoading(true);
        try {
            const response = await fetch('/api/games/load', {
                method: 'POST'
            });
            if (response.status === 200) {
                const data = await response.json();
                setDummyData(data);
                setIsLoading(false);
                return response;
            } else {
                setTimeout(loadDummyData, 1000); // Retry after 1 second
            }
        } catch (err) {
            console.error('API error:', err);
            setTimeout(loadDummyData, 1000); // Retry after error
        }
    };

    useEffect(() => {
        loadDummyData()
            .catch((error) => {
                console.error('Failed to load dummy data:', error);
            });
    }, []);

    return (
        <div>
            <h2 className="text-3xl font-bold mb-6 text-gray-200">Game Manager</h2>
            {
                isLoading ? (
                    <Spinner/>
                ) : (
                    <>
                        <pre>{JSON.stringify(dummyData, null, 2)}</pre>
                        <button
                            onClick={loadDummyData}
                            className="mt-4 bg-blue-500 hover:bg-blue-700 text-purple-200 font-bold py-2 px-4 rounded"
                        >
                            Refresh Data
                        </button>
                    </>)
            }
        </div>
    );
}

export default GameManager;