import {
    useServerHub
} from "@/hooks";
import {
    findGameUserById
} from "@/types";

export const useUser = (userId?: string) => {
    const {discordTown} = useServerHub();
    
    const thisUser = findGameUserById(discordTown, userId);
    return {thisUser};
};