import type {ClocktowerServerDataTypesReminderToken} from "@/api";

export class Reminder {
    readonly id: string;
    readonly roleId: string;
    readonly reminderText: string;

    constructor(data: Partial<Reminder>) {
        this.id = data.id ?? '';
        this.roleId = data.roleId ?? '';
        this.reminderText = data.reminderText ?? '';
    }
}

export function mapToReminderToken(data: ClocktowerServerDataTypesReminderToken): Reminder {
    return new Reminder({
        id: data.id ?? '',
        roleId: data.roleId ?? '',
        reminderText: data.reminderText ?? ''
    });
}