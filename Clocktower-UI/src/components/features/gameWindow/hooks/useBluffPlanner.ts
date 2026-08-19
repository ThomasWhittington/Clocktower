import {
    useCallback,
    useMemo,
    useState
} from "react";
import {
    Role,
    type User
} from "@/types";
import {useAction} from "@/hooks";
import {useAppStore} from "@/store";
import {gamesService} from "@/services";
import {useDiscordTown} from "@/components/features/discordTownPanel/hooks";

interface UseBluffPlannerProps {
    player: User;
}

const BLUFF_SLOT_COUNT = 3;

const normalizeBluffs = (bluffs: (Role | undefined)[] | undefined): (Role | undefined)[] =>
    Array.from({length: BLUFF_SLOT_COUNT}, (_, i) => bluffs?.[i] ?? undefined);

export const useBluffPlanner = ({player}: UseBluffPlannerProps) => {
    const {runAction} = useAction();
    const {gameId} = useAppStore();
    const {discordTown} = useDiscordTown();
    const [pendingRole, setPendingRole] = useState<Role | undefined>(undefined);

    const livePlayer = useMemo(
        () => discordTown?.players?.find(p => p.id === player.id) ?? player,
        [discordTown, player]
    );

    const slots = useMemo(() => normalizeBluffs(livePlayer.draftBluffs), [livePlayer.draftBluffs]);

    const selectedRoleIds = useMemo(
        () => new Set(slots.filter((role): role is Role => role !== undefined).map(role => role.id)),
        [slots]
    );

    const setSlot = useCallback((slotIndex: number, role: Role | undefined) => {
        if (!gameId) return;
        void runAction(async () => {
            return await gamesService.setDraftBluff(gameId, player.id, slotIndex + 1, role?.id);
        });
    }, [gameId, player.id, runAction]);

    const cancelPending = useCallback(() => {
        setPendingRole(undefined);
    }, []);

    const handleRoleClick = useCallback((role: Role) => {
        const existingIndex = slots.findIndex(slot => slot?.id === role.id);
        if (existingIndex !== -1) {
            setSlot(existingIndex, undefined);
            if (pendingRole?.id === role.id) setPendingRole(undefined);
            return;
        }

        if (pendingRole?.id === role.id) {
            setPendingRole(undefined);
            return;
        }

        const emptyIndex = slots.findIndex(slot => slot === undefined);
        if (emptyIndex !== -1) {
            setSlot(emptyIndex, role);
            return;
        }

        setPendingRole(role);
    }, [slots, pendingRole, setSlot]);

    const handleSlotClick = useCallback((slotIndex: number) => {
        if (pendingRole) {
            setSlot(slotIndex, pendingRole);
            setPendingRole(undefined);
            return;
        }

        if (slots[slotIndex]) {
            setSlot(slotIndex, undefined);
        }
    }, [pendingRole, slots, setSlot]);

    return {
        slots,
        selectedRoleIds,
        pendingRole,
        handleRoleClick,
        handleSlotClick,
        cancelPending
    };
};
