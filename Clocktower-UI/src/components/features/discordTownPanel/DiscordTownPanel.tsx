import {
    useEffect,
    useState
} from "react";
import {
    discordService
} from "../../../services";
import type {
    TownOccupants
} from "../../../types";
import {
    DiscordAdminPanel,
    DiscordTown
} from "./components";
import {
    Spinner
} from "../../ui";
import {
    useDiscordHub
} from "./hooks/useDiscordHub.ts";

function DiscordTownPanel() {
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState<string>("");
    const [guildId, setGuildId] = useState<bigint>();
    const [townOccupancy, setTownOccupancy] = useState<TownOccupants>();


    const discordMoveState = useDiscordHub();

    const getTownOccupancy = async () => {
        if (guildId === undefined) return;
        setIsLoading(true);
        await discordService.getTownOccupancy(guildId!)
            .then((data) => setTownOccupancy(data))
            .catch((err) => setError(err.message))
            .finally(() => setIsLoading(false));
    }

    useEffect(() => {
        getTownOccupancy().catch(console.error);
    }, [guildId,]);

    useEffect(() => {
        if (discordMoveState.townOccupancy) {
            setTownOccupancy(discordMoveState.townOccupancy);
        }
    }, [discordMoveState.townOccupancy]);

    return (
        <div
            className="bg-[#121214] h-full">
            <DiscordAdminPanel
                guildIdChange={(id: bigint) => setGuildId(id)}/>

            {isLoading &&
                <Spinner/>}
            {error &&
                <p className="text-red-500 text-sm">{error}</p>}
            {townOccupancy &&
                <>
                    <DiscordTown
                        townOccupancy={townOccupancy}/>
                </>}
        </div>
    );
}

export default DiscordTownPanel;