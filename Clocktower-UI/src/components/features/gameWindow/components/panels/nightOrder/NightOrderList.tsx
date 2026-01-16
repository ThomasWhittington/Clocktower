import {Role, User} from "@/types";
import {NightOrderRecord} from "./NightOrderRecord";

interface NightOrderListProps {
    night: "first" | "other",
    roles: Role[],
    players: User[]
}

export const NightOrderList = ({night, roles, players}: NightOrderListProps) => {
    const title = night === "first" ? "First Night" : "Other Nights";
    return (
        <div className={`night-order-list ${night}`}>
            <h2>{title}</h2>
            {roles.map((role) => (
                <NightOrderRecord key={role.id} night={night} role={role} players={players}/>
            ))}
        </div>
    )
};
