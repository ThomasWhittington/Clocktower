import './App.css'
import {BrowserRouter as Router, Route, Routes} from 'react-router-dom';
import {AuthCallback, BotCallback, ErrorPage, Game, Home, Join, LoginPage, Playground} from "./pages";
import {useAppStore} from "@/store";
import {DiscordUserStatus, ServerDisconnected} from "@/components/ui";
import {useServerHeartbeat} from "@/hooks";

function App() {
    const loggedIn = useAppStore((state) => state.loggedIn);
    const heartbeat = useServerHeartbeat();
    const playground = false;
    return (
        <>
            {playground ?
                <Playground/> :
                <>
                    {
                        heartbeat.status === 'Healthy' ? (
                            <>
                                <Router>
                                    <Routes>
                                        <Route path="/playground" element={<Playground/>}/>
                                        <Route path="/login" element={<LoginPage/>}/>
                                        <Route path="/auth/callback" element={<AuthCallback/>}/>
                                        <Route path="/auth/bot-callback" element={<BotCallback/>}/>
                                        <Route path="/" element={<Home/>}/>
                                        <Route path="/game" element={<Game/>}/>
                                        <Route path="/join" element={<Join/>}/>
                                        <Route path="/error" element={<ErrorPage/>}/>
                                    </Routes>
                                </Router>
                                {loggedIn &&
                                    <div
                                        className="fixed top-4 right-4">
                                        <DiscordUserStatus/>
                                    </div>
                                }
                            </>) : (
                            <ServerDisconnected {...heartbeat}/>
                        )
                    }
                </>
            }
        </>
    );
}

export default App
