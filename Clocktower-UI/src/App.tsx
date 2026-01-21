import './App.css'
import {
    BrowserRouter as Router,
    Route,
    Routes
} from 'react-router-dom';
import {
    AuthCallback,
    BotCallback,
    ErrorPage,
    Game,
    Home,
    Join,
    LoginPage,
    Playground
} from "./pages";
import {ServerDisconnected} from "@/components/ui";
import {
    useButtonTracking,
    useErrorTracking,
    useLinkTracking,
    usePageTracking,
    useServerHeartbeat,
    useUserTracking
} from "@/hooks";
import ReactGA from 'react-ga4';
import {useEffect} from "react";

function AppRoutes() {
    usePageTracking();
    useButtonTracking();
    useLinkTracking();

    return (
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
    );
}

function App() {
    const gaMeasurement = import.meta.env.VITE_GA_MEASUREMENT_ID;
    useErrorTracking();
    useUserTracking();

    useEffect(() => {
        ReactGA.initialize(gaMeasurement);
    }, [gaMeasurement]);

    const heartbeat = useServerHeartbeat();
    const playground = import.meta.env.VITE_PLAYGROUND_MODE === 'true';
    return (
        <>
            {playground ?
                <Playground/> :
                <>
                    {
                        heartbeat.status === 'Healthy' ? (
                            <Router>
                                <AppRoutes/>
                            </Router>
                        ) : (
                            <ServerDisconnected {...heartbeat}/>
                        )
                    }
                </>
            }
        </>
    );
}

export default App