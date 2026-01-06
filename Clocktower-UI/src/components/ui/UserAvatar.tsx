import type {User} from "@/types";

interface UserAvatarProps {
    user: User;
    size?: number;
    className?: string;
}

export function UserAvatar({user, size = 40, className = ""}: Readonly<UserAvatarProps>) {
    return (
        <div className={`user-avatar ${className}`} style={{width: size, height: size}}>
            {user.avatarUrl ? (
                <img
                    src={user.avatarUrl}
                    alt={user.name}
                    draggable="false"
                />
            ) : (
                <div className="w-full h-full bg-gray-600 flex items-center justify-center text-white font-medium select-none">
                    {user.name?.charAt(0)?.toUpperCase() || "?"}
                </div>
            )}
        </div>
    );
}