import type {
    TownOccupants
} from "@/types";
import {
    DiscordTownCategory
} from "./index.ts";

function DiscordTown({townOccupancy}: {
    townOccupancy: TownOccupants
}) {
    return (
        <div id="discord-town">
            {
                townOccupancy?.channelCategories.map(category =>
                    <DiscordTownCategory
                        key={category.id}
                        category={category}/>
                )
            }
        </div>
    );
}

export default DiscordTown;