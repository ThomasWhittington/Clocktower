import type {
    ChannelOccupants,
} from "@/types";
import {
    DiscordTownUser
} from "./index.ts";
import {
    discordService
} from "@/services";
import {
    useAppStore
} from "@/store";
import {
    ValidationUtils
} from "@/utils";
import {
    useUserVoiceStatus
} from "@/components/features/discordTownPanel/hooks";


function DiscordTownChannel({channel}: {
    channel: ChannelOccupants
}) {

    const guildId = useAppStore((state) => state.guildId);
    const currentUser = useAppStore((state) => state.currentUser);
    const {isInVoiceChannel} = useUserVoiceStatus();
    
    const channelId = `discord-channel-${channel.channel.id}`;
    
    const moveUserHere = async () => {
        if (!(ValidationUtils.isValidDiscordId(guildId) &&
            ValidationUtils.isValidDiscordId(channel.channel.id) &&
            currentUser != undefined &&
            ValidationUtils.isValidDiscordId(currentUser.id))
        ) {
            console.error('Ids were not valid');
            console.log(guildId);
            console.log(channel.channel.id);
            console.log(currentUser!.id);

            return;
        }
        
        await discordService.moveUserToChannel(guildId, currentUser.id, channel.channel.id)
            .then((data) => console.log(data))
            .catch((err) => console.error(err));

    }

    return (
        <div
            id={channelId}>
            <a onClick={isInVoiceChannel ? moveUserHere : undefined}
               className={isInVoiceChannel ? "cursor-pointer" : "cursor-not-allowed opacity-50"}>{channel.channel.name}</a>
            {channel.occupants.map(user =>
                <DiscordTownUser
                    key={user.id}
                    user={user}/>)}

        </div>
    );
}

export default DiscordTownChannel;