import {BasePanel} from "@/components/ui";
import {ScriptSelector} from "@/components/features/discordTownPanel/components";

interface ScriptManagerPanelProps {
    isOpen: boolean;
    onClose: () => void;
}

export const ScriptManagerPanel = ({isOpen, onClose}: ScriptManagerPanelProps) => {
    return (
        <BasePanel title="Script Manager" isOpen={isOpen} onClose={onClose} className="script-manager">
            <ScriptSelector/>
        </BasePanel>
    )
};
