import {
    BasePanel,
    CloseButton,
    InputField
} from "@/components/ui";
import {
    Reminder,
    type User
} from "@/types";
import {useReminders} from "@/components/features/gameWindow/hooks";
import {
    ReminderToken,
    Token
} from "@/components/tokens";
import {
    useState,
    type FormEvent
} from "react";

const CUSTOM_REMINDER_MAX_LENGTH = 40;
const REMINDER_TOKEN_SIZE = 100;
const CUSTOM_TRIGGER_REMINDER: Reminder = {id: 'custom-trigger', roleId: 'custom', reminderText: 'Custom note'};

interface ReminderPanelProps {
    isOpen: boolean,
    onClose: () => void,
    player: User
}

export const ReminderPanel = ({isOpen, onClose, player}: ReminderPanelProps) => {
    const {
        availableReminders,
        setReminder,
        addCustomReminder
    } = useReminders(player);
    const [isAddingCustom, setIsAddingCustom] = useState(false);
    const [customReminderText, setCustomReminderText] = useState("");

    const addReminder = async (reminder: Reminder) => {
        await setReminder(player.id, reminder.id);
        onClose();
    }

    const handleAddCustomReminder = async (e: FormEvent) => {
        e.preventDefault();
        const reminderText = customReminderText.trim();
        if (!reminderText) return;

        await addCustomReminder(player.id, reminderText);
        setCustomReminderText("");
        setIsAddingCustom(false);
        onClose();
    }

    const cancelCustomReminder = () => {
        setIsAddingCustom(false);
        setCustomReminderText("");
    }

    return (
        <BasePanel title={`Add reminder for ${player.name}`} isOpen={isOpen} onClose={onClose}>
            <div className="reminder-panel">
                {availableReminders.map(reminder =>

                    <ReminderToken
                        key={reminder.id}
                        reminder={reminder}
                        angleToCenter={0}
                        size={REMINDER_TOKEN_SIZE}
                        onClick={() => addReminder(reminder)}
                        disableXOverlay={true}
                        className=""
                    />
                )}

                <div className="custom-reminder-slot" style={{width: REMINDER_TOKEN_SIZE, height: REMINDER_TOKEN_SIZE}}>
                    <ReminderToken
                        key={CUSTOM_TRIGGER_REMINDER.id}
                        reminder={CUSTOM_TRIGGER_REMINDER}
                        angleToCenter={0}
                        size={REMINDER_TOKEN_SIZE}
                        onClick={() => setIsAddingCustom(true)}
                        disableXOverlay={true}
                        className={isAddingCustom ? 'invisible' : ''}
                    />

                    {isAddingCustom &&
                        <form className="custom-reminder-form" onSubmit={handleAddCustomReminder}>
                            <InputField
                                autoFocus
                                value={customReminderText}
                                onChange={(e) => setCustomReminderText(e.target.value)}
                                placeholder="Custom reminder text..."
                                maxLength={CUSTOM_REMINDER_MAX_LENGTH}
                            />
                            <button type="submit" className="btn-primary" disabled={!customReminderText.trim()}>
                                Add
                            </button>
                            <CloseButton type="button" onClick={cancelCustomReminder} aria-label="Cancel custom reminder"/>
                        </form>
                    }
                </div>

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