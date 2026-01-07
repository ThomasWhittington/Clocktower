import type {ClocktowerServerDataTypesRoleRole} from "@/api";
import {Edition, RoleType} from "@/types";

export type Role = {
    name: string;
    description: string;
    type: RoleType,
    edition: Edition
};

export function mapToRole(roleDto: ClocktowerServerDataTypesRoleRole): Role {
    return {
        name: roleDto.name!,
        description: roleDto.description!,
        type: (RoleType[roleDto.type as keyof typeof RoleType]) ?? RoleType.Unknown,
        edition: (Edition[roleDto.edition as keyof typeof Edition]) ?? Edition.Unknown
    };
}