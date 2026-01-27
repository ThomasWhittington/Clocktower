import {
    DiscordAdminPanel,
    DiscordTownCategory
} from "./components";
import {Spinner} from "@/components/ui";
import {
    useCurrentUserIsStoryteller,
    useDiscordTown
} from "./hooks";

function DiscordTownPanel() {
    const {
        discordTown,
        isLoading,
        error
    } = useDiscordTown();
    const isStoryteller = useCurrentUserIsStoryteller();

    return (
        <div
            id="discord-town-panel"
            className="bg-discord h-full flex flex-col justify-between pr-4">

            {isStoryteller &&
                <DiscordAdminPanel/>
            }

            {isLoading &&
                <Spinner/>}
            {error &&
                <p className="error-text">{error}</p>}
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
        </div>
    );
}

export default DiscordTownPanel;