import {
    useEffect,
    useState
} from "react";
import {
    Spinner
} from '@/components/ui';
import styles
    from "./GameManager.module.css";
import GameList
    from "./components";

import {
    gamesService
} from "@/services";
import type {
    GameState
} from "@/types";

function GameManager() {
    const [isLoading, setIsLoading] = useState(false);
    const [games, setGames] = useState<GameState[]>([]);
    const [game, setGame] = useState<GameState>();
    const [gameId, setGameId] = useState('');
    const [hasError, setHasError] = useState(false);
    const [error, setError] = useState('');

    const clearError = () => {
        setHasError(false);
        setError('');
    };

    const handleError = (err: any) => {
        const errorMessage = err.response?.data?.message ||
            err.response?.data ||
            err.message ||
            'An unexpected error occurred';
        setError(errorMessage);
        setHasError(true);
    };

    const loadDummyData = async () => {
        clearError();
        setIsLoading(true);
        gamesService.loadDummyData()
            .catch((err) => handleError(err))
            .finally(() => setIsLoading(false));
    };

    const getGames = async () => {
        clearError();
        setIsLoading(true);
        gamesService.getGames()
            .then((data) => setGames(data))
            .catch((err) => handleError(err))
            .finally(() => setIsLoading(false));
    }

    const getGame = async (gameId: string) => {
        clearError();
        setIsLoading(true);
        gamesService.getGame(gameId).then(data => setGame(data))
            .catch((err) => handleError(err))
            .finally(() => setIsLoading(false));
    }

    const startGame = async () => {
        clearError();
        setIsLoading(true);
        gamesService.startGame(gameId).then(data => {
            setGame(data);
            getGames();
        })
            .catch((err) => handleError(err))
            .finally(() => setIsLoading(false));
    }

    const handleKeyPress = async (event: KeyboardEvent) => {
        if (event.key === 'a' || event.key === 'A') {
            console.log('A key pressed!');
            const name = prompt("Player name");
            if (name?.trim()) {
                await getGame(name)
            }
        }
    };

    useEffect(() => {
        /*
        globalThis.addEventListener('keydown', handleKeyPress);
        return () => {
            globalThis.removeEventListener('keydown', handleKeyPress);
        };
        */
    }, []);

    return (
        <div>
            <h2 className="text-3xl font-bold mb-6 text-gray-200">Game Manager</h2>
            {
                isLoading ? (
                    <Spinner/>
                ) : (
                    <>
                        {hasError && (
                            <div
                                className="mb-4 p-3 bg-red-100 border border-red-400 text-red-700 rounded">
                                <div
                                    className="flex justify-between items-center">
                                    <span>{error}</span>
                                    <button
                                        onClick={clearError}
                                        className="ml-2 !bg-white  !border-red-400 text-red-500 hover:text-red-700 font-bold"
                                        aria-label="Dismiss error"
                                    >
                                        ×
                                    </button>
                                </div>
                            </div>
                        )}

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
                            value={gameId}
                            onChange={e => setGameId(e.target.value)}
                        />
                        <button
                            onClick={startGame}
                            className={styles.button}>
                            Start game
                        </button>

                        <GameList
                            games={games}/>
                        <br/>

                        <pre>{JSON.stringify(game, null, 2)}</pre>


                    </>)
            }
        </div>
    );
}

export default GameManager;