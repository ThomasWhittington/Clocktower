import {useKeyboardShortcut} from "@/hooks";
import {UserUtils} from "@/utils";
import {User} from "@/types";
import type {
    PanelDataMap,
    PanelType
} from "@/components/features/gameWindow/hooks/useActivePanel.ts";

interface UseGameWindowShortcutsProps {
    thisUser: User | undefined;
    script: any;
    togglePanel: <T extends PanelType>(panel: T, data?: PanelDataMap[T]) => void;
    setIsDraftMode: (callback: (prev: boolean) => boolean) => void;
}

export function useGameWindowShortcuts({
                                           thisUser,
                                           script,
                                           togglePanel,
                                           setIsDraftMode
                                       }: UseGameWindowShortcutsProps) {
    const isStoryTeller = UserUtils.isStoryTeller(thisUser);
    const hasScript = script !== undefined;

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
        onKeyPress: () => togglePanel('rolePlanner'),
        enabled: isStoryTeller && hasScript
    });
}