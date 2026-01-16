import {useCallback, useState} from 'react';

type PanelType = "script" | "user" | "role" | "night";

interface UseActivePanelReturn {
    activePanel: PanelType | null;
    openPanel: (panel: PanelType) => void;
    closePanel: () => void;
    togglePanel: (panel: PanelType) => void;
    isPanelOpen: (panel: PanelType) => boolean;
}

export const useActivePanel = (): UseActivePanelReturn => {
    const [activePanel, setActivePanel] = useState<PanelType | null>(null);

    const openPanel = useCallback((panel: PanelType) => {
        setActivePanel(panel);
    }, []);

    const closePanel = useCallback(() => {
        setActivePanel(null);
    }, []);

    const togglePanel = useCallback((panel: PanelType) => {
        setActivePanel(prev => prev === panel ? null : panel);
    }, []);

    const isPanelOpen = useCallback((panel: PanelType) => {
        return activePanel === panel;
    }, [activePanel]);

    return {
        activePanel,
        openPanel,
        closePanel,
        togglePanel,
        isPanelOpen
    };
};