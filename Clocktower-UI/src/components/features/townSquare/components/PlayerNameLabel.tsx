export function PlayerNameLabel({name}: Readonly<{
    name: string;
}>) {
    return (
        <p className="px-2 py-1.5 rounded-md text-center text-3xl">{name}</p>
    );
}