import {useState} from "react";
import {Spinner} from '@/components/ui';
import {gamesService} from "@/services";
import type {GamePerspective} from "@/types";
import {useAppStore} from "@/store";
import {GameList} from "@/components/features/gameManager/components";
import {BottomHud} from "@/components/features/gameWindow/components";
import {useNavigate} from "react-router-dom";
import ReactGA from "react-ga4";

function GameManager() {
    const navigate = useNavigate();
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
    const getGames = async () => {
        clearError();
        setIsLoading(true);
        gamesService.getGames()
            .then((data) => setGames(data))
            .catch((err) => handleError(err))
            .finally(() => setIsLoading(false));
    }

    const startGame = async () => {
        if (!currentUser?.id) {
            handleError({message: 'User not authenticated'});
            return;
        }
        clearError();
        setIsLoading(true);
        gamesService.startGame(text, guildId, currentUser.id).then(data => {
            if (data?.id) {
                ReactGA.event({
                    category: "Game Manager",
                    action: "start_game",
                    label: data.id
                });
                setGameId(data.id);
                navigate('/game');
            } else {
                handleError({message: 'Failed to start game: No game ID returned'});
            }
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

                        <button onClick={getGames} className="btn-primary">
                            Get games
                        </button>
                        <br/>
                        <input value={text} onChange={e => setText(e.target.value)}
                        />
                        <button onClick={startGame} className="btn-primary">
                            Start game
                        </button>

                        <GameList games={games}/>
                    </>
                )
            }
            <BottomHud gameId={gameId} storyTellers={[]}/>
        </>
    );
}

export default GameManager;