import {
    useCallback,
    useState
} from 'react';
import {User} from "@/types";

export type PanelType = "script" | "user" | "role" | "night" | "token" | "rolePlanner";

export type PanelDataMap = {
    script: undefined;
    user: undefined;
    role: undefined;
    night: undefined;
    token: { player: User };
    rolePlanner: undefined;
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
            const isClosing = prev === panel;
            setPanelData(isClosing ? null : (data ?? null));
            return isClosing ? null : panel;
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