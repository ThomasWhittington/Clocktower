import {
    type User,
    UserType
} from "@/types";
import {
    IconButton,
    Spinner
} from "@/components/ui";
import {
    Eye,
    Person,
    Quill
} from "@/components/ui/icons";

interface UserTypeChangeProps {
    user: User,
    isLoading: boolean,
    canRun: boolean,
    changeUserType: (user: User, type: UserType) => void
}

export const UserTypeChange = ({
                                   user,
                                   isLoading,
                                   canRun,
                                   changeUserType
                               }: UserTypeChangeProps) => {
    return (
        <div className="user-type-change">
            {isLoading && <Spinner/>}
            {canRun && <>
                <IconButton
                    icon={<Quill/>}
                    variant="primary"
                    isEnabled={user.userType !== UserType.StoryTeller}
                    onClick={() => changeUserType(user, UserType.StoryTeller)}
                    tooltip="Change user type to Storyteller"
                />

                <IconButton
                    icon={<Person/>}
                    variant="primary"
                    isEnabled={user.userType !== UserType.Player}
                    onClick={() => changeUserType(user, UserType.Player)}
                    tooltip="Change user type to Player"
                />

                <IconButton
                    icon={<Eye/>}
                    variant="primary"
                    isEnabled={user.userType !== UserType.Spectator}
                    onClick={() => changeUserType(user, UserType.Spectator)}
                    tooltip="Change user type to Spectator"
                />
            </>
            }
        </div>
    );
}