import {BasePanel, InputField} from "@/components/ui";

interface InviteUserPanelProps {
    isOpen: boolean;
    onClose: () => void;
}

export const InviteUserPanel = ({isOpen, onClose}: InviteUserPanelProps) => (
    <BasePanel title="Invite Player" isOpen={isOpen} onClose={onClose}>
        <InputField placeholder="User ID or Name..."/>
    </BasePanel>
);