import type {MiniGuild} from "@/types";
import {Spinner} from "@/components/ui";

interface GuildsListProps {
    guilds: MiniGuild[];
    loading: boolean;
    error: string | null;
    selectedGuildId: string;
    onGuildClick: (guild: MiniGuild) => void;
}

export const GuildsList = ({guilds, loading, error, selectedGuildId, onGuildClick}: GuildsListProps) => {
    return (
        <>
            {loading && <Spinner />}
            {error && <div>Error: {error}</div>}
            {guilds.length > 0 &&
                guilds.map(guild => {
                    const selected = guild.id === selectedGuildId;

                    return (
                        <button
                            key={guild.id}
                            className={selected ? "btn-secondary" : "btn-primary"}
                            onClick={() => onGuildClick(guild)}
                            aria-pressed={selected}
                        >
                            {selected ? `✓ ${guild.name}` : guild.name}
                        </button>
                    );
                })
            }
        </>
    );
};