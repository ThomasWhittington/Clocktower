import type {
    ChannelOccupants,
} from "../../../../types";
import {
    DiscordTownUser
} from "./index.ts";
import {
    discordService
} from "../../../../services";
import {
    useAppStore
} from "../../../../store.ts";
import {
    ValidationUtils
} from "../../../../utils/validation.ts";

function DiscordTownChannel({channel}: {
    channel: ChannelOccupants
}) {

    const guildId = useAppStore((state) => state.guildId);
    const currentUserId = useAppStore((state) => state.currentUserId);

    const moveUserHere = async () => {
        if (!(ValidationUtils.isValidDiscordId(guildId) &&
            ValidationUtils.isValidDiscordId(channel.channel.id) &&
            ValidationUtils.isValidDiscordId(currentUserId))
        ) {
            console.error('Ids were not valid');
            console.log(guildId);
            console.log(channel.channel.id);
            console.log(currentUserId);

            return;
        }

        console.log('guild: ' + guildId);
        console.log('channel: ' + channel.channel.id);

        await discordService.moveUserToChannel(guildId, currentUserId, channel.channel.id)
            .then((data) => console.log(data))
            .catch((err) => console.error(err));

    }

    return (
        <div>
            <a onClick={moveUserHere}
               className="cursor-pointer">{channel.channel.name}</a>
            {channel.occupants.map(user =>
                <DiscordTownUser
                    key={user.id}
                    user={user}/>)}

        </div>
    );
}

export default DiscordTownChannel;