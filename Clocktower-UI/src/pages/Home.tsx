import {
    useAppStore
} from "@/store";
import {
    authService
} from "@/services";

function Home() {
    const loggedIn = useAppStore((state) => state.loggedIn);
    
    return (
        <>
            {loggedIn ?
                (
                    <>
                        <h1>Play on Discord</h1>

                        <h2>You are playing in these games</h2>

                        <h2>Games in your servers</h2>

                        <button
                            onClick={() => window.location.href = "/game"}>Go to game
                        </button>
                    </>
                ) : (
                    <>
                        <button
                            onClick={() => authService.initiateDiscordLogin()}
                        >
                            Login with Discord
                        </button>
                    </>
                )
            }
        </>
    );
}

export default Home;