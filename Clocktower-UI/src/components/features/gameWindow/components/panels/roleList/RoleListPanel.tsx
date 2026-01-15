import {BasePanel} from "@/components/ui";
import {useServerHub} from "@/hooks";
import {RoleListRecord} from "./RoleListRecord";
import {useDiscordTown} from "@/components/features/discordTownPanel/hooks";

interface RoleListPanelProps {
    isOpen: boolean;
    onClose: () => void;
}

export const RoleListPanel = ({isOpen, onClose}: RoleListPanelProps) => {
        const {script} = useServerHub();
        const {discordTown} = useDiscordTown();
        const players = discordTown?.gameUsers || [];
        return (
            <BasePanel title="Role List" isOpen={isOpen} onClose={onClose}>
                <div className="role-list-container">
                    <div className="team townsfolk">
                        <aside className="role-aside"><h4>Townsfolk</h4></aside>
                        <ul className="role-list">
                            {script?.townsfolk.map((role) => (
                                <RoleListRecord key={role.id} role={role} players={players}/>
                            ))}
                        </ul>
                    </div>

                    <div className="team outsider">
                        <aside className="role-aside"><h4>Outsider</h4></aside>
                        <ul className="role-list">
                            {script?.outsiders.map((role) => (
                                <RoleListRecord key={role.id} role={role} players={players}/>
                            ))}
                        </ul>
                    </div>

                    <div className="team minion">
                        <aside className="role-aside"><h4>Minion</h4></aside>
                        <ul className="role-list">
                            {script?.minions.map((role) => (
                                <RoleListRecord key={role.id} role={role} players={players}/>
                            ))}
                        </ul>
                    </div>

                    <div className="team demon">
                        <aside className="role-aside"><h4>Demon</h4></aside>
                        <ul className="role-list">
                            {script?.demons.map((role) => (
                                <RoleListRecord key={role.id} role={role} players={players}/>
                            ))}
                        </ul>
                    </div>
                </div>
            </BasePanel>
        )
    }
;
