import {Reminder} from "@/types";
import reminderBase from "#/tokenParts/base/reminderBase.png";
import {TokenRoleIcon} from "@/components/tokens/TokenRoleIcon.tsx";

interface ReminderTokenProps {
    reminder: Reminder;
    angleToCenter: number;
    size: number;
    className?: string;
}

export const ReminderToken = ({reminder, angleToCenter, size, className}: ReminderTokenProps) => {
    return (
        <span
            key={reminder.id}
            className={`reminder-token ${className}`}
            style={{
                transform: `rotate(${-angleToCenter}deg)`,
                width: `${size}px`,
                height: `${size}px`,
                backgroundImage: `url(${reminderBase})`,
                fontSize: `${size * 0.18}px`
            }}
        >
            <TokenRoleIcon roleId={reminder.roleId} className="reminder-token-icon"/>
            <span className="reminder-text">{reminder.reminderText}</span>
        </span>
    );
}