import './App.css'
import {
    BrowserRouter as Router,
    Route,
    Routes
} from 'react-router-dom';
import {
    AuthCallback,
    Home,
    LoginPage
} from "./pages";

function App() {
    return (         
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
                        path="/"
                        element={
                            <Home/>}/>
                </Routes>
            </Router>
    );
}

export default App
