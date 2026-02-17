import {BasePanel} from "@/components/ui";
import {
    Reminder,
    type User
} from "@/types";
import {useReminders} from "@/components/features/gameWindow/hooks";
import {
    ReminderToken,
    Token
} from "@/components/tokens";
import {useEffect} from "react";

interface ReminderPanelProps {
    isOpen: boolean,
    onClose: () => void,
    player: User
}

export const ReminderPanel = ({isOpen, onClose, player}: ReminderPanelProps) => {
    const {
        availableReminders,
        setReminder
    } = useReminders(player);
    useEffect(() => {
        if (isOpen && availableReminders.length <= 0) {
            onClose();
        }
    }, [isOpen, availableReminders.length, onClose]);

    const addReminder = async (reminder: Reminder) => {
        await setReminder(player.id, reminder.id);
        onClose();
    }

    return (
        <BasePanel title={`Add reminder for ${player.name}`} isOpen={isOpen} onClose={onClose}>
            <div className="reminder-panel">
                {availableReminders.map(reminder =>

                    <ReminderToken
                        key={reminder.id}
                        reminder={reminder}
                        angleToCenter={0}
                        size={100}
                        onClick={() => addReminder(reminder)}
                        disableXOverlay={true}
                    />
                )}

                <div className="current-role">
                    <Token
                        size={200}
                        key="current-role"
                        role={player.role}
                        className="pointer-events-none"
                    />
                </div>
            </div>
        </BasePanel>
    );
};