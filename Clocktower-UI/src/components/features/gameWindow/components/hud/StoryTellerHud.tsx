import {IconButton} from "@/components/ui";
import {
    AddUserIcon,
    BedIcon,
    DayIcon,
    DraftIcon,
    EveningIcon,
    NightIcon,
    ScriptIcon,
    TownsquareIcon
} from "@/components/ui/icons";
import {
    useDiscordActions,
    useTimeOfDay
} from "@/components/features/discordTownPanel/hooks";
import {GameTime} from "@/types";

interface StoryTellerHudProps {
    usersIsOpen: boolean;
    onUsersClick: () => void;
    scriptIsOpen: boolean;
    onScriptClick: () => void;
    showDraftRoles: boolean;
    onDraftToggle: () => void;
}

export const StoryTellerHud = (
    {
        usersIsOpen,
        onUsersClick,
        scriptIsOpen,
        onScriptClick,
        showDraftRoles,
        onDraftToggle
    }: StoryTellerHudProps) => {
    const setTime = useTimeOfDay();
    const {
        sendToCottages,
        sendToTownSquare,
        isLoading: isDiscordLoading
    } = useDiscordActions();
    return (
        <div className="controls-storyteller">
            <div>
                <IconButton
                    icon={<AddUserIcon/>}
                    isActive={usersIsOpen}
                    onClick={onUsersClick}
                    tooltip="User Manager"
                />
                <IconButton
                    icon={<ScriptIcon/>}
                    isActive={scriptIsOpen}
                    onClick={onScriptClick}
                    tooltip="Script Manager"
                />
                <IconButton
                    icon={<DraftIcon/>}
                    isActive={showDraftRoles}
                    isActiveVariant="danger"
                    onClick={onDraftToggle}
                    tooltip="Toggle Draft Mode"
                />
            </div>

            <div>
                <IconButton
                    icon={<TownsquareIcon/>}
                    onClick={sendToTownSquare}
                    isEnabled={!isDiscordLoading}
                    tooltip="Send to Town Square"
                />
                <IconButton
                    icon={<BedIcon/>}
                    onClick={sendToCottages}
                    isEnabled={!isDiscordLoading}
                    tooltip="Send to Cottages"
                />
            </div>

            <div>
                <IconButton
                    icon={<DayIcon/>}
                    onClick={() => setTime(GameTime.Day)}
                    tooltip="Set Time to Day"
                />
                <IconButton
                    icon={<EveningIcon/>}
                    onClick={() => setTime(GameTime.Evening)}
                    tooltip="Set Time to Evening"
                />
                <IconButton
                    icon={<NightIcon/>}
                    onClick={() => setTime(GameTime.Night)}
                    tooltip="Set Time to Night"
                />
            </div>
        </div>
    );
}