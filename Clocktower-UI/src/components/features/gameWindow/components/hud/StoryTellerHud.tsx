import {IconButton} from "@/components/ui";
import {
    AddUserIcon,
    BedIcon,
    BookIcon,
    DayIcon,
    DraftIcon,
    EveningIcon,
    NightIcon,
    ScriptIcon,
    TownsquareIcon,
    VoteIcon
} from "@/components/ui/icons";
import {
    useDiscordActions,
    useTimeOfDay
} from "@/components/features/discordTownPanel/hooks";
import {GameTime} from "@/types";
import {useNominationState} from "@/components/features/gameWindow/hooks";

interface StoryTellerHudProps {
    usersIsOpen: boolean;
    onUsersClick: () => void;
    scriptIsOpen: boolean;
    onScriptClick: () => void;
    rolePlannerIsOpen: boolean;
    onRolePlannerClick: () => void;
    showDraftRoles: boolean;
    onDraftToggle: () => void;
    hasScript: boolean;
}

export const StoryTellerHud = (
    {
        usersIsOpen,
        onUsersClick,
        scriptIsOpen,
        onScriptClick,
        rolePlannerIsOpen,
        onRolePlannerClick,
        showDraftRoles,
        onDraftToggle,
        hasScript
    }: StoryTellerHudProps) => {
    const setTime = useTimeOfDay();

    const {nominationsEnabled, toggleNominations} = useNominationState();

    const nominationsButtonText = nominationsEnabled ? 'Close Nominations' : 'Open Nominations';
    const nominationsButtonVariant = nominationsEnabled ? 'danger' : 'secondary';
    const {
        sendToCottages,
        sendToTownSquare,
        isLoading: isDiscordLoading
    } = useDiscordActions();

    const scriptControls = hasScript && (
        <>
            <IconButton
                icon={<BookIcon/>}
                isActive={rolePlannerIsOpen}
                onClick={onRolePlannerClick}
                tooltip="Role Planner"
            />
            <IconButton
                icon={<DraftIcon/>}
                isActive={showDraftRoles}
                isActiveVariant="danger"
                onClick={onDraftToggle}
                tooltip="Toggle Draft Mode"
            />
        </>
    );

    const gameManagementControls = hasScript && (
        <>
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

            <div>
                <IconButton icon={<VoteIcon/>}
                            tooltip={nominationsButtonText}
                            variant={nominationsButtonVariant}
                            onClick={toggleNominations}
                />
            </div>
        </>
    );

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
                {scriptControls}
            </div>

            {gameManagementControls}
        </div>
    );
}