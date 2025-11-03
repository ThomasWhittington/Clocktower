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
import {
    useAppStore
} from "../../../store.ts";
import {
    ValidationUtils
} from "../../../utils";

function DiscordTownPanel() {
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState<string>("");
    const guildId = useAppStore((state) => state.guildId);
    const [townOccupancy, setTownOccupancy] = useState<TownOccupants>();


    const discordMoveState = useDiscordHub();

    const getTownOccupancy = async () => {
        if (!ValidationUtils.isValidDiscordId(guildId)) {
            console.error('guildId was not valid');
            return;
        }
        setIsLoading(true);
        await discordService.getTownOccupancy(guildId)
            .then((data) => setTownOccupancy(data))
            .catch((err) => setError(err.message))
            .finally(() => setIsLoading(false));
    }

    useEffect(() => {
        getTownOccupancy().catch(console.error);
    }, [guildId]);

    useEffect(() => {
        if (discordMoveState.townOccupancy) {
            setTownOccupancy(discordMoveState.townOccupancy);
        }
    }, [discordMoveState.townOccupancy]);

    return (
        <div
            className="bg-[#121214] h-full">
            <DiscordAdminPanel/>

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