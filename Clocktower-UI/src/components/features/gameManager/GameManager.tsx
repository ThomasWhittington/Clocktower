import {
    useEffect,
    useState
} from "react";
import {
    Spinner
} from '../../ui';
import styles
    from "./GameManager.module.css";
import GameList
    from "./components";

function GameManager() {
    const [isLoading, setIsLoading] = useState(false);
    const [games, setGames] = useState([]);
    const loadDummyData = async () => {
        setIsLoading(true);
        fetch('/api/games/load', {
            method: 'POST'
        })
            .then((res) => res.json())
            .catch((err) => console.error('API error:', err))
            .finally(() => setIsLoading(false));
    };

    const getGames = async () => {
        setIsLoading(true);
        fetch('/api/games')
            .then((res) => res.json())
            .then((data) => setGames(data))
            .catch((err) => console.error('API error:', err))
            .finally(() => setIsLoading(false));
    }

    useEffect(() => {
        const handleKeyPress = (event: KeyboardEvent) => {
            if (event.key === 'a' || event.key === 'A') {
                console.log('A key pressed!');
                const name = prompt("Player name");
                if (name?.trim()) {
                    //TODO add player to current game id
                }
            }
        };

        // Add event listener
        globalThis.addEventListener('keydown', handleKeyPress);

        // Cleanup
        return () => {
            globalThis.removeEventListener('keydown', handleKeyPress);
        };
    }, []);

    return (
        <div>
            <h2 className="text-3xl font-bold mb-6 text-gray-200">Game Manager</h2>
            {
                isLoading ? (
                    <Spinner/>
                ) : (
                    <>
                        <button
                            onClick={loadDummyData}
                            className={styles.button}>
                            Load Dummy Data
                        </button>
                        <br/>
                        <button
                            onClick={getGames}
                            className={styles.button}>
                            Get games
                        </button>
                        <br/>
                        <input
                            type="text"
                            className={styles.input}/>

                        <GameList
                            games={games}/>
                    </>)
            }
        </div>
    );
}

export default GameManager;