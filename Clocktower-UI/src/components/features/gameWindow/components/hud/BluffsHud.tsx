import {
    Role,
    type User
} from "@/types";
import {useState} from "react";
import {Token} from "@/components/tokens";
import {IconButton} from "@/components/ui";
import {Demon} from "@/components/ui/icons";

interface BluffsHudProps {
    user: User | undefined
}

export const BluffsHud = ({user}: BluffsHudProps) => {
    const [isOpen, setIsOpen] = useState(true);
    const [hoveredBluff, setHoveredBluff] = useState<Role | undefined>();
    const bluffs = user?.bluffs?.filter(r => r != undefined).map((bluff) => new Role(bluff)) ?? [];

    if (!bluffs.length) return null;
    return (
        <div className="bluffs">
            <IconButton
                className="bluffs-button"
                icon={<Demon/>}
                isActive={isOpen}
                onClick={() => setIsOpen((current) => !current)}
                tooltip={isOpen ? "Hide Bluffs" : "Show Bluffs"}
            />
            {isOpen && (
                <div className="bluffs-menu">
                    {hoveredBluff && (
                        <div className="role-ability">
                            {hoveredBluff.fullDescription}
                        </div>
                    )}  <h2>Bluffs</h2>

                    <ul>
                        {bluffs.map((bluff, index) => (
                            <li
                                key={bluff?.id ?? index}
                                onMouseEnter={() => setHoveredBluff(bluff)}
                                onMouseLeave={() => setHoveredBluff(undefined)}
                                onFocus={() => setHoveredBluff(bluff)}
                                onBlur={() => setHoveredBluff(undefined)}
                            >
                                <Token role={bluff} size='9vmin'/>
                            </li>
                        ))}
                    </ul>
                </div>
            )}
        </div>
    );
};