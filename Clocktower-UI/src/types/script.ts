import {Role} from "@/types/role.ts";
import {RoleType} from "@/types/roleType.ts";

export class Script {

    readonly name: string;
    readonly author: string;
    readonly roles: Role[];

    constructor(data: Partial<Script>) {
        this.name = data.name ?? '';
        this.author = data.author ?? '';
        this.roles = (data.roles ?? []).map(role => new Role(role));
    }

    get townsfolk(): Role[] {
        return this.roles.filter(r => r.type === RoleType.Townsfolk);
    }

    get outsiders(): Role[] {
        return this.roles.filter(r => r.type === RoleType.Outsider);
    }

    get minions(): Role[] {
        return this.roles.filter(r => r.type === RoleType.Minion);
    }

    get demons(): Role[] {
        return this.roles.filter(r => r.type === RoleType.Demon);
    }

    get travellers(): Role[] {
        return this.roles.filter(r => r.type === RoleType.Traveller);
    }

    get firstNightOrder(): Role[] {
        return this.roles?.filter(r => r.firstNight !== 0 && r.type !== RoleType.Traveller)
            .sort((a, b) => a.firstNight - b.firstNight);
    }

    get otherNightOrder(): Role[] {
        return this.roles?.filter(r => r.otherNight !== 0 && r.type !== RoleType.Traveller)
            .sort((a, b) => a.otherNight - b.otherNight);
    }

}

