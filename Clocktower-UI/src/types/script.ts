import {Role} from "@/types/role.ts";

export class Script {

    readonly name: string;
    readonly author: string;
    readonly roles: Role[];

    constructor(data: Partial<Script>) {
        this.name = data.name ?? '';
        this.author = data.author ?? '';
        this.roles = (data.roles ?? []).map(role => new Role(role));
    }
}

