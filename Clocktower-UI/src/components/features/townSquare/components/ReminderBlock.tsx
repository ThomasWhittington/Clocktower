import {ReminderToken} from "@/components/tokens";
import {Reminder,} from "@/types";
import {useState} from "react";

interface ReminderBlockProps {
    reminderTokens: Reminder[],
    distanceToCenter: number,
    angleToCenter: number,
    size: number,
    isParentHovered: boolean,
    onManageRemindersClicked?: () => void,
    removeReminderClicked?: ((reminderId: string) => void)
}

export const ReminderBlock = (
    {
        reminderTokens,
        distanceToCenter,
        angleToCenter,
        size,
        isParentHovered,
        onManageRemindersClicked,
        removeReminderClicked
    }: ReminderBlockProps) => {
    const [isHovered, setIsHovered] = useState(false);
    const showAddButton = isHovered || isParentHovered;
    const add: Reminder = {
        id: 'add',
        roleId: 'plus',
        reminderText: ''
    }
    return (
        <div
            className="reminders-block"
            onMouseEnter={() => setIsHovered(true)}
            onMouseLeave={() => setIsHovered(false)}
            style={{
                ['--reminders-height' as string]: `${size}px`,
                transform: `rotate(${angleToCenter}deg) translateX(${size / 2}px)`,
                width: `${distanceToCenter / 2}px`,
                height: `${size}px`,
            }}
        >
            {reminderTokens.map((reminder) =>
                <ReminderToken key={reminder.id} reminder={reminder} angleToCenter={angleToCenter} size={size / 2} onClick={() => {
                    removeReminderClicked?.(reminder.id);
                }}/>
            )}
            <ReminderToken
                key={add.id}
                reminder={add}
                angleToCenter={angleToCenter}
                size={size / 2}
                className={`add-reminder ${showAddButton ? 'opacity-100' : 'opacity-0'}`}
                onClick={onManageRemindersClicked}
            />
        </div>
    );
}