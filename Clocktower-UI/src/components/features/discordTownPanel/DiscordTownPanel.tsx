import {
    DiscordAdminPanel,
    DiscordTown
} from "./components";
import {
    Spinner
} from "@/components/ui";
import {
    useTownOccupancy
} from "./hooks";
import {
    useAppStore
} from "@/store";

function DiscordTownPanel() {
    const {
        townOccupancy,
        isLoading,
        error
    } = useTownOccupancy();
    const gameId = useAppStore((state) => state.gameId);

    return (
        <div
            id="discord-town-panel"
            className="bg-discord h-full flex flex-col justify-between pr-4">
            <DiscordAdminPanel/>

            {isLoading &&
                <Spinner/>}
            {error &&
                <p className="text-red-500 text-sm">{error}</p>}
            {townOccupancy &&
                <div
                    className="mb-auto">
                    <DiscordTown
                        townOccupancy={townOccupancy}/>
                </div>
            }
            {gameId &&
                <h2>Current Game: {gameId}</h2>}
        </div>
    );
}

export default DiscordTownPanel;