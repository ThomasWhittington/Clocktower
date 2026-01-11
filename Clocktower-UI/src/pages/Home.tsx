import {useAppStore} from "@/store";
import {authService} from "@/services";
import {GameManager} from "@/components/features";
import {GuildsList} from "@/components/ui";
import {useGuildsWithUser} from "@/pages/hooks";
import type {MiniGuild} from "@/types";

function Home() {
    const {loggedIn, setGuildId} = useAppStore();
    const {
        guilds,
        loading,
        error
    } = useGuildsWithUser(loggedIn);

    const handleGuildClick = (guild: MiniGuild) => {
        setGuildId(guild.id);
        globalThis.location.href = '/game';
    };

    return (
        <div className="p-8">
            {loggedIn ?
                (<>
                        <GuildsList
                            guilds={guilds}
                            loading={loading}
                            error={error}
                            onGuildClick={handleGuildClick}
                        />
                        <GameManager/>
                    </>
                ) : (
                    <button onClick={() => authService.initiateDiscordLogin()}>
                        Login with Discord
                    </button>
                )
            }
        </div>
    );
}

export default Home;