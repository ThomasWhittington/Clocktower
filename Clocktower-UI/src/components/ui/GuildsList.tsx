import type {
    MiniGuild
} from "@/types";
import {
    Spinner
} from "@/components/ui";

interface GuildsListProps {
    guilds: MiniGuild[];
    loading: boolean;
    error: string | null;
    onGuildClick: (guild: MiniGuild) => void;
}

export const GuildsList = ({ guilds, loading, error, onGuildClick }: GuildsListProps) => {
    return (
        <>
            {loading && <Spinner />}
            {error && <div>Error: {error}</div>}
            {guilds.length > 0 &&
                guilds.map(guild => (
                    <button
                        key={guild.id}
                        className="btn-primary"
                        onClick={() => onGuildClick(guild)}
                    >
                        {guild.name}
                    </button>
                ))
            }
        </>
    );
};