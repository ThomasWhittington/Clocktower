import type {GameState} from "@/types";
import {useAppStore} from "@/store";
import type {FC} from "react";


interface GameListProps {
    games: GameState[];
}

export const GameList: FC<GameListProps> = ({games}) => {
    const setGameId = useAppStore((state) => state.setGameId);
    return (
        <div className="mt-4">
            <h3 className="text-xl font-semibold text-gray-200 mb-4">Games List</h3>
            <div className="space-y-4">
                {games.length === 0 ? (<p className="text-gray-400">No games available</p>) : (
                    games.map((game: GameState) => (
                        <div key={game.id} className="bg-gray-800 p-4 rounded-lg w-1/4">
                            <h4 className="text-lg font-medium text-gray-200">{game.id}</h4>
                            <h5 className="sm text-gray-600">{game.guildId}</h5>
                            <p className="text-gray-400">Players: {game.players.length}{game.maxPlayers > 0 ? `/${game.maxPlayers}` : ''}</p>
                            <p className="text-gray-400">Spectators: {game.spectators.length}</p>
                            <p className="text-gray-400">StoryTellers: {game.storyTellers.length}</p>
                            {game.isFull && <p>FULL</p>}
                            {game.createdDate && <h5 className="sm text-gray-600">{new Date(game.createdDate).toDateString()}</h5>}
                            <h5 className="sm text-gray-600">{game.createdBy.id}</h5>

                            <button className="btn-outline" onClick={() => setGameId(game.id)}>
                                Select
                            </button>
                        </div>
                    ))
                )}
            </div>
        </div>
    );
};

