import {useKeyboardShortcut} from "@/hooks";
import {
    Role,
    Script,
} from "@/types";
import type {
    PanelDataMap,
    PanelType
} from "@/components/features/gameWindow/hooks/useActivePanel.ts";
import {useCurrentUserIsStoryteller} from "@/components/features/discordTownPanel/hooks";

interface UseGameWindowShortcutsProps {
    script: Script | undefined;
    togglePanel: <T extends PanelType>(panel: T, data?: PanelDataMap[T]) => void;
    setIsDraftMode: (callback: (prev: boolean) => boolean) => void;
    selectedDraftRoles: Role[];
    assignToDraft: () => Promise<void>;

}

export function useGameWindowShortcuts({
                                           script,
                                           togglePanel,
                                           setIsDraftMode,
                                           selectedDraftRoles,
                                           assignToDraft
                                       }: UseGameWindowShortcutsProps) {
    const isStoryTeller = useCurrentUserIsStoryteller();
    const hasScript = script !== undefined;
    const canAssignToDraft = selectedDraftRoles.length > 0;

    useKeyboardShortcut({
        key: 'a',
        onKeyPress: () => togglePanel('user'),
        enabled: isStoryTeller
    });

    useKeyboardShortcut({
        key: 's',
        onKeyPress: () => togglePanel('script'),
        enabled: isStoryTeller
    });

    useKeyboardShortcut({
        key: 'r',
        onKeyPress: () => togglePanel('role'),
        enabled: hasScript
    });

    useKeyboardShortcut({
        key: 'n',
        onKeyPress: () => togglePanel('night'),
        enabled: hasScript
    });

    useKeyboardShortcut({
        key: 'd',
        onKeyPress: () => setIsDraftMode(prev => !prev),
        enabled: isStoryTeller
    });

    useKeyboardShortcut({
        key: 'p',
        onKeyPress: () => togglePanel('paper')
    });

    useKeyboardShortcut({
        key: 'x',
        onKeyPress: () => {
            void assignToDraft();
        },
        enabled: isStoryTeller && hasScript && canAssignToDraft
    });
}
