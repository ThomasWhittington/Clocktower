import type {ButtonHTMLAttributes, ReactNode} from "react";

interface IconButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
    icon: ReactNode;
    variant?: "primary" | "secondary" | "outline";
    isActive?: boolean;
}

export const IconButton = ({
                               icon,
                               variant = 'secondary',
                               isActive,
                               className = "",
                               ...props
                           }: IconButtonProps) => {
    const baseClass = isActive ? "btn-outline" : `btn-${variant}`;

    return (
        <button
            className={`${baseClass} btn-icon ${className}`}
            {...props}
        >
            <span className="w-6 h-6">
                {icon}
            </span>
        </button>
    );
};