import {DiscordAdminPanel, DiscordTownCategory} from "./components";
import {Spinner} from "@/components/ui";
import {useDiscordTown, useUser} from "./hooks";
import {useAppStore} from "@/store";
import {UserUtils} from "@/utils";

function DiscordTownPanel() {
    const {
        discordTown,
        isLoading,
        error
    } = useDiscordTown();
    const {currentUser} = useAppStore();
    const {thisUser} = useUser(currentUser?.id);

    return (
        <div
            id="discord-town-panel"
            className="bg-discord h-full flex flex-col justify-between pr-4">

            {UserUtils.isStoryTeller(thisUser) &&
                <DiscordAdminPanel/>
            }

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
        </div>
    );
}

export default DiscordTownPanel;