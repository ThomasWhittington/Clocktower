import {useAppStore} from "@/store";
import {authService} from "@/services";

import {useAddBot, useGuildsWithUser} from "@/pages/hooks";
import type {MiniGuild} from "@/types";
import {GuildsList} from "@/components/ui";
import {GameManager} from "@/components/features";

function Home() {
    const loggedIn = useAppStore((state) => state.loggedIn);
    const {
        guilds,
        loading,
        error
    } = useGuildsWithUser(loggedIn);

    const setGuildId = useAppStore((state) => state.setGuildId);
    const {addBot} = useAddBot();

    const handleGuildClick = (guild: MiniGuild) => {
        setGuildId(guild.id);
        globalThis.location.href = '/game';
    };

    return (
        <div className="m-8">
            {loggedIn ?
                (
                    <>
                        <h1>Play on Discord</h1>
                        <GuildsList
                            guilds={guilds}
                            loading={loading}
                            error={error}
                            onGuildClick={handleGuildClick}
                        />
                        <button onClick={addBot} className="btn-outline">
                            Add Bot To Server
                        </button>

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