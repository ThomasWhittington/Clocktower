import {type User, UserType} from "@/types";
import {IconButton, Spinner} from "@/components/ui";
import {useUserControls} from "@/components/features/gameWindow/hooks";
import {Eye, Person, Quill} from "@/components/ui/icons";

export const UserTypeChange = ({user}: { user: User }) => {
    const {isLoading, canRun, changeUserType} = useUserControls();
    return (
        <div className="user-type-change">
            {isLoading && <Spinner/>}
            {canRun && <>
                <IconButton
                    icon={<Quill/>}
                    variant="primary"
                    isEnabled={user.userType !== UserType.StoryTeller}
                    onClick={() => changeUserType(user, UserType.StoryTeller)}
                />

                <IconButton
                    icon={<Person/>}
                    variant="primary"
                    isEnabled={user.userType !== UserType.Player}
                    onClick={() => changeUserType(user, UserType.Player)}
                />

                <IconButton
                    icon={<Eye/>}
                    variant="primary"
                    isEnabled={user.userType !== UserType.Spectator}
                    onClick={() => changeUserType(user, UserType.Spectator)}
                />
            </>
            }
        </div>
    );
}