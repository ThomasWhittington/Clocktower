import {BasePanel} from "@/components/ui";
import {useServerHub} from "@/hooks";
import {NightOrderList} from "./NightOrderList";
import {useDiscordTown} from "@/components/features/discordTownPanel/hooks";

interface NightOrderPanelProps {
    isOpen: boolean;
    onClose: () => void;
}

export const NightOrderPanel = ({isOpen, onClose}: NightOrderPanelProps) => {
    const {script} = useServerHub();
    const {discordTown} = useDiscordTown();
    const players = discordTown?.players || [];
    const firstNightOrder = script?.getFirstNightOrder(players.length) ?? [];
    const otherNightOrder = script?.otherNightOrder ?? [];
    return (
        <BasePanel title="Night Order" isOpen={isOpen} onClose={onClose}>
            <div className="night-order-container">
                <NightOrderList night="first" roles={firstNightOrder} players={players}/>
                <NightOrderList night="other" roles={otherNightOrder} players={players}/>
            </div>
        </BasePanel>
    )
};