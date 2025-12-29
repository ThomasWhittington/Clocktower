import {useState} from "react";
import {Spinner} from '@/components/ui';

import {discordService, gamesService} from "@/services";
import type {GamePerspective} from "@/types";
import {useAppStore} from "@/store";
import {GameList} from "@/components/features/gameManager/components";

function GameManager() {
    const [isLoading, setIsLoading] = useState(false);
    const [games, setGames] = useState<GamePerspective[]>([]);
    const [text, setText] = useState('');
    const [hasError, setHasError] = useState(false);
    const [error, setError] = useState('');
    const guildId = useAppStore((state) => state.guildId);
    const gameId = useAppStore((state) => state.gameId);
    const currentUser = useAppStore((state) => state.currentUser);
    const setGameId = useAppStore((state) => state.setGameId);


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

    const startGame = async () => {
        clearError();
        setIsLoading(true);
        gamesService.startGame(text, guildId, currentUser?.id!).then(data => {
            setGameId(data?.id ?? null);
            getGames();
        })
            .catch((err) => handleError(err))
            .finally(() => setIsLoading(false));
    }

    const inviteUser = async () => {
        if (!gameId) return;
        clearError();
        setIsLoading(true);
        await discordService.inviteUser(gameId, text).then(_ => {
        })
            .catch((err) => handleError(err))
            .finally(() => setIsLoading(false));
    }

    const pingUser = async () => {
        if (!gameId) return;
        clearError();
        setIsLoading(true);
        await discordService.pingUser(text).then(_ => {
        })
            .catch((err) => handleError(err))
            .finally(() => setIsLoading(false));
    }
    return (

        <>
            <h2 className="text-3xl font-bold mb-6 text-gray-200">Game Manager</h2>
            {
                isLoading ? (
                    <Spinner/>
                ) : (
                    <>
                        {hasError && (
                            <div
                                className="mb-4 p-3 bg-red-100 border border-red-400 text-red-700 rounded w-1/4">
                                <div
                                    className="flex justify-between items-center">
                                    <span>{error}</span>
                                    <button onClick={clearError}
                                            className="ml-2 border-red-400 text-red-500 hover:text-red-700 font-bold"
                                            aria-label="Dismiss error"
                                    >
                                        ×
                                    </button>
                                </div>
                            </div>
                        )}

                        <h1>Current Game: {gameId}</h1>

                        <button onClick={loadDummyData} className="btn-primary">
                            Load Dummy Data
                        </button>
                        <br/>
                        <button onClick={getGames} className="btn-primary">
                            Get games
                        </button>
                        <br/>
                        <input value={text} onChange={e => setText(e.target.value)}
                        />
                        <button onClick={startGame} className="btn-primary">
                            Start game
                        </button>
                        {gameId &&
                            <>
                                <button onClick={inviteUser} className="btn-primary">
                                    Invite User
                                </button>

                                <button onClick={pingUser} className="btn-secondary">
                                    Ping User
                                </button>
                            </>
                        }
                        <GameList games={games}/>
                    </>
                )
            }
        </>
    );
}

export default GameManager;