import {
    useEffect,
    useState
} from 'react';

function WeatherWidget() {
    const [weather, setWeather] = useState(null);

    useEffect(() => {
        console.log("fetch")
        fetch('/api/weatherforecast')
            .then((res) => res.json())
            .then((data) => setWeather(data))
            .catch((err) => console.error('API error:', err));
    }, []);

    return (
        <div>
            <h2>Weather</h2>
            {weather ? (
                <pre>{JSON.stringify(weather, null, 2)}</pre>
            ) : (
                <p>Loading...</p>
            )}
        </div>
    );
}

export default WeatherWidget;