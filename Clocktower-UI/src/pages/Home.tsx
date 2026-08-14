import {useAppStore} from "@/store";
import {authService} from "@/services";
import {GameManager} from "@/components/features";
import {GuildsList} from "@/components/ui";
import {useGuildsWithUser} from "@/pages/hooks";

function Home() {
    const {loggedIn, setGuildId, guildId} = useAppStore();
    const {
        guilds,
        loading,
        error
    } = useGuildsWithUser(loggedIn);

    return (
        <div className="p-8">
            {loggedIn ?
                (<>
                        <GuildsList
                            guilds={guilds}
                            loading={loading}
                            error={error}
                            selectedGuildId={guildId}
                            onGuildClick={(guild) => setGuildId(guild.id)}
                        />
                        {guildId &&
                            <GameManager/>
                        }
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