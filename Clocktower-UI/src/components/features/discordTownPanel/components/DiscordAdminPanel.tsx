import {
    useEffect,
    useState
} from "react";
import {
    discordService
} from "../../../../services";
import {
    Spinner,
    StatusIcon
} from "../../../ui";
import type {
    CheckGuildApiResponse,
    GetTownStatusApiResponse
} from "../../../../openApi";
import {
    useAppStore
} from "../../../../store.ts";
import {
    ValidationUtils
} from "../../../../utils/validation.ts";

function DiscordAdminPanel() {
    const [isLoading, setIsLoading] = useState(false);
    const [inputValue, setInputValue] = useState<string>('');
    const guildId = useAppStore((state) => state.guildId);
    const setGuildId = useAppStore((state) => state.setGuildId);
    const setCurrentUserId = useAppStore((state) => state.setCurrentUserId);
    const [error, setError] = useState<string>("");
    const [message, setMessage] = useState<string>("");
    const [buttonEnabled, setButtonEnabled] = useState<boolean>(false);
    const [guildData, setGuildData] = useState<CheckGuildApiResponse>();
    const [townStatus, setTownStatus] = useState<GetTownStatusApiResponse>();


    useEffect(() => {
        inputChanged("1318686543363178666");
        setCurrentUserId(285398267854848000n);
    }, []);


    const inputChanged = (value: string) => {
        setButtonEnabled(false);

        setError("");
        const numericValue = BigInt(value);
        if (ValidationUtils.isValidDiscordId(numericValue)) {
            setButtonEnabled(true);
        }
        setInputValue(value)
    }

    const checkGuildId = async (id: bigint): Promise<boolean> => {
        setError('');
        setGuildData({});
        setIsLoading(true);

        await discordService.checkGuild(id)
            .then((data) => setGuildData(data))
            .catch((err) => setError(err.message))
            .finally(() => setIsLoading(false));
        return guildData?.valid ?? false;
    };


    const handleSetGuildId = async () => {
        const numericValue = BigInt(inputValue);
        if (await checkGuildId(numericValue)) {
            //guildIdChange(numericValue);
            setGuildId(numericValue);
        }
    };

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
            <div
                className="flex items-center space-x-2">
                <label
                    htmlFor="serverId"
                    className="text-white">ServerID:</label>
                <input
                    id="serverId"
                    type="number"
                    className="bg-[#222327] w-2/3 h-10 flex-3 rounded-2xl"
                    value={inputValue}
                    onChange={(e) => inputChanged(e.target.value)}
                />
                <button
                    className="bg-blue-500 text-white px-4 py-2 rounded-2xl hover:bg-blue-600"
                    disabled={!buttonEnabled}
                    onClick={handleSetGuildId}
                >
                    Set
                </button>
            </div>
            {isLoading &&
                <Spinner/>}
            {error &&
                <p className="text-red-500 text-sm">{error}</p>}
            {message &&
                <p className="text-purple-700 text-sm">{message}</p>}
            {(guildData?.valid && guildId > 0) &&
                <div>
                    {townStatus &&
                        <StatusIcon
                            status={townStatus.exists ?? false}/>
                    }
                    <h2 className="text-2xl">{guildData.name}</h2>
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