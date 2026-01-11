import type {ClocktowerServerDataTypesRoleRole} from "@/api";
import {Edition, RoleType} from "@/types";

export class Role {
    readonly id: string;
    readonly name: string;
    readonly description: string;
    readonly type: RoleType;
    readonly edition: Edition;
    readonly firstNight: number;
    readonly firstNightReminder: string;
    readonly otherNight: number;
    readonly otherNightReminder: string;
    readonly setup: boolean;
    readonly reminders: string[];
    readonly remindersGlobal: string[];

    constructor(data: Partial<Role>) {
        console.log(data)
        this.id = data.id ?? '';
        this.name = data.name ?? '';
        this.description = data.description ?? '';
        this.type = data.type ?? RoleType.Unknown;
        this.edition = data.edition ?? Edition.Unknown;
        this.firstNight = data.firstNight ?? 0;
        this.firstNightReminder = data.firstNightReminder ?? '';
        this.otherNight = data.otherNight ?? 0;
        this.otherNightReminder = data.otherNightReminder ?? '';
        this.setup = data.setup ?? false;
        this.reminders = data.reminders ?? [];
        this.remindersGlobal = data.remindersGlobal ?? [];
    }
}

export function mapToRole(roleDto: ClocktowerServerDataTypesRoleRole): Role {
    return new Role({
        id: roleDto.id ?? '',
        name: roleDto.name ?? '',
        description: roleDto.description ?? '',
        type: RoleType[roleDto.type as keyof typeof RoleType],
        edition: Edition[roleDto.edition as keyof typeof Edition],
        firstNight: roleDto.metadata?.firstNight ?? 0,
        firstNightReminder: roleDto.metadata?.firstNightReminder ?? undefined,
        otherNight: roleDto.metadata?.otherNight ?? 0,
        otherNightReminder: roleDto.metadata?.otherNightReminder ?? undefined,
        setup: roleDto.metadata?.setup ?? false,
        reminders: roleDto.metadata?.reminders ?? undefined,
        remindersGlobal: roleDto.metadata?.remindersGlobal ?? undefined
    });
}