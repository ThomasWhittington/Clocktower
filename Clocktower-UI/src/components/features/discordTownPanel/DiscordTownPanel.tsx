import {
    DiscordAdminPanel,
    DiscordTownCategory
} from "./components";
import {
    Spinner
} from "@/components/ui";
import {
    useDiscordTown
} from "./hooks";
import {
    useAppStore
} from "@/store";

function DiscordTownPanel() {
    const {
        discordTown,
        isLoading,
        error
    } = useDiscordTown();
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
            {discordTown &&
                <div
                    id="discord-town"
                    className="mb-auto">
                    {
                        discordTown?.channelCategories.map(category =>
                            <DiscordTownCategory
                                key={category.id}
                                category={category}/>
                        )
                    }
                </div>
            }
            {gameId &&
                <h2>Current Game: {gameId}</h2>}
        </div>
    );
}

export default DiscordTownPanel;