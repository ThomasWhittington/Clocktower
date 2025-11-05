import {
    useState
} from "react";
import {
    discordService
} from "@/services";
import {
    Spinner,
    StatusIcon
} from "@/components/ui";
import type {
    GetTownStatusApiResponse
} from "@/generated";
import {
    useAppStore
} from "@/store";
import {
    ValidationUtils
} from "@/utils";

function DiscordAdminPanel() {
    const [isLoading, setIsLoading] = useState(false);
    const guildId = useAppStore((state) => state.guildId);
    const [error, setError] = useState<string>("");
    const [message, setMessage] = useState<string>("");
    const [townStatus, setTownStatus] = useState<GetTownStatusApiResponse>();

    const handleGetStatus = async () => {
        if (!ValidationUtils.isValidDiscordId(guildId)) {
            console.error('guildId was not valid');
            return;
        }
        setIsLoading(true);
        await discordService.getTownStatus(guildId)
            .then((data) => setTownStatus(data))
            .catch((err) => setError(err.message))
            .finally(() => setIsLoading(false));
    }


    const handleRebuildTown = async () => {
        if (!ValidationUtils.isValidDiscordId(guildId)) {
            console.error('guildId was not valid');
            return;
        }

        setIsLoading(true);
        await discordService.rebuildTown(guildId)
            .then((data) => setMessage(data))
            .catch((err) => setError(err.message))
            .finally(() => setIsLoading(false));
    }

    return (

        <div
            className="flex flex-col space-y-2">
            {isLoading &&
                <Spinner/>}
            {error &&
                <p className="text-red-500 text-sm">{error}</p>}
            {message &&
                <p className="text-purple-700 text-sm">{message}</p>}
            {(guildId != '') &&
                <div>
                    <h2 className="text-2xl">{guildId}</h2>
                    {townStatus &&
                        <StatusIcon
                            status={townStatus.exists ?? false}/>
                    }
                    <button
                        className="rounded-2xl"
                        onClick={handleGetStatus}>
                        Get Status
                    </button>

                    <button
                        className="rounded-2xl"
                        onClick={handleRebuildTown}>
                        Rebuild Town
                    </button>
                </div>
            }
        </div>

    )
}

export default DiscordAdminPanel;