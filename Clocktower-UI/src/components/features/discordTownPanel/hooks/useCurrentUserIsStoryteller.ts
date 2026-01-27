import {useAppStore} from "@/store";
import {useUser} from "@/components/features/discordTownPanel/hooks/useUser.ts";
import {UserUtils} from "@/utils";

export function useCurrentUserIsStoryteller(): boolean {
    const {currentUser} = useAppStore();
    const {thisUser} = useUser(currentUser?.id);
    return UserUtils.isStoryTeller(thisUser);
}