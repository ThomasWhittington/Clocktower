import {type User, UserType} from "@/types";

export const UserUtils = {
    isStoryTeller(user: User | undefined): boolean {
        if (!user) return false;
        return user.userType === UserType.StoryTeller;
    }
}