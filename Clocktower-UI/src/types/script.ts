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

    get otherNightOrder(): Role[] {
        return this.roles.filter(r => r.otherNight !== 0 && r.type !== RoleType.Traveller)
            .sort((a, b) => a.otherNight - b.otherNight);
    }

    getFirstNightOrder(playerCount: number): Role[] {
        const baseOrder = this.roles.filter(r => r.firstNight !== 0 && r.type !== RoleType.Traveller)
            .sort((a, b) => a.firstNight - b.firstNight);

        if (playerCount >= 7) {
            const demonInfoRole = createDemonInfoRole();
            const minionInfoRole = createMinionInfoRole();
            return [...baseOrder, minionInfoRole, demonInfoRole].sort((a, b) => a.firstNight - b.firstNight);
        }

        return baseOrder;
    }
}

function createMinionInfoRole(): Role {
    return new Role({
        id: "evil-minion",
        name: "Minion info",
        description: "",
        type: RoleType.Minion,
        firstNight: 5,
        firstNightReminder: "If more than 1 minion, tell them who their fellow minions are. Tell the minions who the demon is.",
        otherNight: 0,
        otherNightReminder: "",
        setup: false,
        reminders: [],
        remindersGlobal: []
    });
}

function createDemonInfoRole(): Role {
    return new Role({
        id: "evil-demon",
        name: "Demon info & bluffs",
        description: "",
        type: RoleType.Demon,
        firstNight: 8,
        firstNightReminder: "Give the demon 3 not in play characters. Tell the demon who the minions are.",
        otherNight: 0,
        otherNightReminder: "",
        setup: false,
        reminders: [],
        remindersGlobal: []
    });
}