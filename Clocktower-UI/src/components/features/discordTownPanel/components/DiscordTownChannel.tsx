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


function DiscordTownChannel({channel}: Readonly<{
    channel: ChannelOccupants
}>) {

    const guildId = useAppStore((state) => state.guildId);
    const currentUser = useAppStore((state) => state.currentUser);
    const {isInVoiceChannel} = useUserVoiceStatus();
    
    const moveUserHere = async () => {
        if (!(ValidationUtils.isValidDiscordId(guildId) &&
            ValidationUtils.isValidDiscordId(channel.channel.id) &&
            currentUser != undefined &&
            ValidationUtils.isValidDiscordId(currentUser.id))
        ) {
            return;
        }

        await discordService.moveUserToChannel(guildId, currentUser.id, channel.channel.id)
            .catch((err) => console.error(err));

    }

    return (
        <div
            id={`discord-channel-${channel.channel.id}`}
            className={`channel-container${channel.occupants.length > 0 ? '' : '-empty'}`}>
            <a onClick={isInVoiceChannel ? moveUserHere : undefined}
               className={`channel-button channel-button-${isInVoiceChannel ? "enabled" :
                   "disabled"}`}>{channel.channel.name}</a>
            <div
                className="channel-occupants">{channel.occupants.map(user => (
                <DiscordTownUser
                    key={user.id}
                    user={user}/>
            ))}</div>
        </div>
    );
}

export default DiscordTownChannel;