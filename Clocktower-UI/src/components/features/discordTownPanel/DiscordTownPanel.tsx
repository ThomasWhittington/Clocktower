import {
    DiscordAdminPanel,
    DiscordTown
} from "./components";
import {
    Spinner
} from "@/components/ui";
import {
    useTownOccupancy
} from "./hooks";

function DiscordTownPanel() {
    const {
        townOccupancy,
        isLoading,
        error
    } = useTownOccupancy();

    return (
        <div
            id="discord-town-panel"
            className="bg-[#121214] h-full flex flex-col justify-between">
            <DiscordAdminPanel/>

            {isLoading &&
                <Spinner/>}
            {error &&
                <p className="text-red-500 text-sm">{error}</p>}
            {townOccupancy &&
                <div
                    className="mb-auto">
                    <DiscordTown
                        townOccupancy={townOccupancy}/>
                </div>
            }
        </div>
    );
}

export default DiscordTownPanel;