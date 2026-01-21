import type {PanelType} from "@/components/features/gameWindow/hooks";
import {Script} from "@/types";
import {
    BottomHud,
    CenterHud,
    RightHud,
    StoryTellerHud,
    TopHud
} from "@/components/features/gameWindow/components";
import {useDiscordTown} from "@/components/features/discordTownPanel/hooks";

interface GameHudProps {
    script: Script | undefined;
    isStoryteller: boolean;
    isPanelOpen: (panel: PanelType) => boolean;
    togglePanel: (panel: PanelType) => void;
    setIsDraftMode: (callback: (prev: boolean) => boolean) => void;
    isDraftMode: boolean;
}

export function GameHud({
                            script,
                            isStoryteller,
                            isPanelOpen,
                            togglePanel,
                            setIsDraftMode,
                            isDraftMode
                        }: Readonly<GameHudProps>) {
    const {discordTown} = useDiscordTown();

    return (
        <>
            {isStoryteller && (
                <StoryTellerHud
                    usersIsOpen={isPanelOpen('user')}
                    onUsersClick={() => togglePanel('user')}
                    scriptIsOpen={isPanelOpen('script')}
                    onScriptClick={() => togglePanel('script')}
                    rolePlannerIsOpen={isPanelOpen('rolePlanner')}
                    onRolePlannerClick={() => togglePanel('rolePlanner')}
                    showDraftRoles={isDraftMode}
                    onDraftToggle={() => setIsDraftMode(prev => !prev)}
                />
            )}
            <CenterHud/>
            <TopHud scriptName={script?.name}/>
            <RightHud
                onRoleListClick={() => script && togglePanel('role')}
                onNightOrderClick={() => script && togglePanel('night')}
            />
            <BottomHud storyTellers={discordTown?.storyTellers ?? []}/>
        </>
    );
}