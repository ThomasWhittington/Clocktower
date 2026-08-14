import {Reminder} from "@/types";
import {TokenRoleIcon} from "@/components/tokens/TokenRoleIcon.tsx";

interface ReminderTokenProps {
    reminder: Reminder;
    angleToCenter: number;
    size: number;
    className?: string;
    onClick?: () => void;
    disableXOverlay?: boolean;
}

export const ReminderToken = ({reminder, angleToCenter, size, className, onClick, disableXOverlay}: ReminderTokenProps) => {
    const reminderBase = `${import.meta.env.BASE_URL}tokenParts/base/reminderBase.png`;
    return (
        <span
            key={reminder.id}
            className={`reminder-token ${className ?? ''} ${disableXOverlay ? 'no-x-overlay' : ''}`.trim()}
            style={{
                transform: `rotate(${-angleToCenter}deg)`,
                width: `${size}px`,
                height: `${size}px`,
                backgroundImage: `url(${reminderBase})`,
                fontSize: `${size * 0.18}px`
            }}
            onClick={onClick}
        >
            <TokenRoleIcon roleId={reminder.roleId} className="reminder-token-icon"/>
            <span className="reminder-text">{reminder.reminderText}</span>
        </span>
    );
}