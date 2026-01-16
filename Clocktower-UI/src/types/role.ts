import type {ClocktowerServerDataTypesRoleRole} from "@/api";
import {Edition, RoleType} from "@/types";

export type RoleDto = {
    id?: string;
    name?: string;
    description?: string;
    type?: string;
    edition?: string;
    firstNight?: number;
    firstNightReminder?: string;
    otherNight?: number;
    otherNightReminder?: string;
    setup?: boolean;
    reminders?: string[];
    remindersGlobal?: string[];
};

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

    constructor(data: Partial<Role> | RoleDto) {
        this.id = data.id ?? '';
        this.name = data.name ?? '';
        this.description = data.description ?? '';
        this.type = typeof data.type === 'string'
            ? (RoleType[data.type as keyof typeof RoleType] ?? RoleType.Unknown)
            : (data.type ?? RoleType.Unknown);
        this.edition = typeof data.edition === 'string'
            ? (Edition[data.edition as keyof typeof Edition] ?? Edition.Unknown)
            : (data.edition ?? Edition.Unknown);
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
        type: (RoleType[roleDto.type as keyof typeof RoleType]) ?? RoleType.Unknown,
        edition: (Edition[roleDto.edition as keyof typeof Edition]) ?? Edition.Unknown,
        firstNight: roleDto.firstNight,
        firstNightReminder: roleDto.firstNightReminder ?? undefined,
        otherNight: roleDto.otherNight,
        otherNightReminder: roleDto.otherNightReminder ?? undefined,
        setup: roleDto.setup ?? false,
        reminders: roleDto.reminders ?? undefined,
        remindersGlobal: roleDto.remindersGlobal ?? undefined
    });
}