import {ReminderToken} from "@/components/tokens";
import {Reminder} from "@/types";

interface ReminderBlockProps {
    reminderTokens: Reminder[],
    distanceToCenter: number,
    angleToCenter: number,
    size: number,
}

export const ReminderBlock = (
    {
        reminderTokens,
        distanceToCenter,
        angleToCenter,
        size
    }: ReminderBlockProps) => {
    return (
        <div
            className="reminders-block"
            style={{
                ['--reminders-height' as string]: `${size}px`,
                transform: `rotate(${angleToCenter}deg) translateX(${size / 2}px)`,
                width: `${distanceToCenter / 2}px`,
                height: `${size}px`,
            }}
        >
            {reminderTokens.map((reminder) =>
                <ReminderToken key={reminder.id} reminder={reminder} angleToCenter={angleToCenter} size={size / 2}/>
            )}
        </div>
    );
}