import './App.css'
import {
    BrowserRouter as Router,
    Route,
    Routes
} from 'react-router-dom';
import {
    AuthCallback,
    BotCallback,
    Game,
    Home,
    LoginPage
} from "./pages";
import {
    useAppStore
} from "@/store";
import {
    DiscordUserStatus
} from "@/components/ui";

function App() {
    const loggedIn = useAppStore((state) => state.loggedIn);
    return (
        <>
            <Router>
                <Routes>
                    <Route
                        path="/login"
                        element={
                            <LoginPage/>}/>
                    <Route
                        path="/auth/callback"
                        element={
                            <AuthCallback/>}
                    />
                    <Route
                        path="/auth/bot-callback"
                        element={
                            <BotCallback/>}
                    />
                    <Route
                        path="/"
                        element={
                            <Home/>}/>
                    <Route
                        path="/game"
                        element={
                            <Game/>}/>
                </Routes>
            </Router>
            {loggedIn &&
                <div
                    className="fixed top-4 right-4">
                    <DiscordUserStatus/>
                </div>
            }
        </>
    );
}

export default App
