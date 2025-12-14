import {
    Spinner
} from "@/components/ui";
import {
    useDiscordActions
} from "@/components/features/discordTownPanel/hooks";

function DiscordAdminPanel() {
    const { sendToCottages, sendToTownSquare, error, result, isLoading, canRun } = useDiscordActions();

    return (
        <div
            className="flex flex-col space-y-2">
            {isLoading &&
                <Spinner/>}
            {result &&
                <p className="text-green-500 text-sm">{result}</p>}
            {error &&
                <p className="text-red-500 text-sm">{error}</p>}
            {canRun &&
                <div>
                    <button
                        className="btn-primary"
                        onClick={sendToTownSquare}>⛲
                    </button>
                    <button
                        className="btn-secondary"
                        onClick={sendToCottages}>🛌
                    </button>
                </div>
            }
        </div>

    )
}

export default DiscordAdminPanel;