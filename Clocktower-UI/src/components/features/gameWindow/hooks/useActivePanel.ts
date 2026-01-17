import {useCallback, useState} from 'react';
import {User} from "@/types";

type PanelType = "script" | "user" | "role" | "night" | "token";

type PanelDataMap = {
    script: undefined;
    user: undefined;
    role: undefined;
    night: undefined;
    token: { player: User };
};

interface UseActivePanelReturn {
    activePanel: PanelType | null;
    panelData: unknown;
    openPanel: <T extends PanelType>(panel: T, data?: PanelDataMap[T]) => void;
    closePanel: () => void;
    togglePanel: <T extends PanelType>(panel: T, data?: PanelDataMap[T]) => void;
    isPanelOpen: (panel: PanelType) => boolean;
    getPanelData: <T extends PanelType>(panel: T) => PanelDataMap[T] | null;
}

export const useActivePanel = (): UseActivePanelReturn => {
    const [activePanel, setActivePanel] = useState<PanelType | null>(null);
    const [panelData, setPanelData] = useState<unknown>(null);

    const openPanel = useCallback(<T extends PanelType>(panel: T, data?: PanelDataMap[T]) => {
        setActivePanel(panel);
        setPanelData(data ?? null);
    }, []);

    const closePanel = useCallback(() => {
        setActivePanel(null);
        setPanelData(null);
    }, []);

    const togglePanel = useCallback(<T extends PanelType>(panel: T, data?: PanelDataMap[T]) => {
        setActivePanel(prev => {
            if (prev === panel) {
                setPanelData(null);
                return null;
            }
            setPanelData(data ?? null);
            return panel;
        });
    }, []);


    const isPanelOpen = useCallback((panel: PanelType) => {
        return activePanel === panel;
    }, [activePanel]);
    const getPanelData = useCallback(<T extends PanelType>(panel: T): PanelDataMap[T] | null => {
        return activePanel === panel ? (panelData as PanelDataMap[T]) : null;
    }, [activePanel, panelData]);

    return {
        activePanel,
        panelData,
        openPanel,
        closePanel,
        togglePanel,
        isPanelOpen,
        getPanelData

    };
};