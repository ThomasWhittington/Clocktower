import type {
    ChannelOccupants,
} from "../../../../types";
import {
    DiscordTownUser
} from "./index.ts";

function DiscordTownChannel({channel}: {
    channel: ChannelOccupants
}) {
    return (
        <>
            <p>{channel.channel.name}</p>
            {channel.occupants.map(user =>
                <DiscordTownUser
                    key={user.id}
                    user={user}/>)}

        </>
    );
}

export default DiscordTownChannel;