export function StatusIndicator({color = "yellow"}: Readonly<{ color?: "yellow" | "red" | "green" }>) {
    const colorClass = {
        yellow: "bg-yellow-400",
        red: "bg-red-500",
        green: "bg-green-500"
    }[color];

    return <div className={`w-2 h-2 ${colorClass} rounded-full animate-pulse`}/>;
}