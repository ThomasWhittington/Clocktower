import {
    useState
} from "react";
import {
    discordService
} from "../../../../services";
import {
    Spinner
} from "../../../ui";

function DiscordAdminPanel({guildIdChange}: {
    guildIdChange: (value: bigint) => void
}) {
    const [isLoading, setIsLoading] = useState(false);
    const [inputValue, setInputValue] = useState<bigint>(0n);
    const [_, setGuildId] = useState<bigint>(0n);
    const [error, setError] = useState<string>("");
    const [buttonEnabled, setButtonEnabled] = useState<boolean>(false);
    const [guildData, setGuildData] = useState<{
        valid?: boolean,
        name?: string | null,
        message?: string | null
    }>();

    const inputChanged = async (e: any) => {
        setButtonEnabled(false);

        setError("");
        const numericValue = BigInt(e.target.value);
        if (numericValue > 41943040000) {
            setButtonEnabled(true);
        }
        setInputValue(numericValue)
    }

    const checkGuildId = async (id: bigint): Promise<boolean> => {
        setError('');
        setGuildData({});
        setIsLoading(true);
        let valid = false;

        discordService.checkGuild(id)
            .then((data) => setGuildData(data))
            .catch((err) => setError(err.message))
            .finally(() => setIsLoading(false));
        return valid;
    };


    const handleSetGuildId = async () => {
        if (await checkGuildId(inputValue)) {
            guildIdChange(inputValue);
            setGuildId(inputValue);
        }
    };

    const handleGetStatus = async () => {

    }


    const handleRebuildTown = async () => {

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
                    onChange={inputChanged}
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
            {guildData?.valid &&
                <div>
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