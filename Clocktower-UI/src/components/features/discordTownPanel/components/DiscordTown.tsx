import type {
    TownOccupants
} from "../../../../types";
import {
    DiscordTownCategory
} from "./index.ts";

function DiscordTown({townOccupancy}: {
    townOccupancy: TownOccupants
}) {
    return (
        <>
            {
                townOccupancy?.channelCategories.map(category =>
                    <DiscordTownCategory
                        key={category.id}
                        category={category}/>
                )
            }
        </>
    );
}

export default DiscordTown;