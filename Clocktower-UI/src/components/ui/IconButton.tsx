import {type ButtonHTMLAttributes, Children, isValidElement, type ReactNode} from "react";

interface IconButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
    icon: ReactNode | ReactNode[];
    text?: string;
    variant?: "primary" | "secondary" | "outline" | "danger";
    isActiveVariant?: "primary" | "secondary" | "outline" | "danger";
    isActive?: boolean;
    isEnabled?: boolean;
}

export const IconButton = ({
                               icon,
                               text,
                               variant = 'secondary',
                               isActive,
                               isActiveVariant,
                               isEnabled = true,
                               className = "",
                               ...props
                           }: IconButtonProps) => {
    const activeClass = isActiveVariant ? `btn-${isActiveVariant}` : "btn-outline";
    let baseClass = isActive ? activeClass : `btn-${variant}`;

    return (
        <button
            disabled={!isEnabled}
            className={`${baseClass} btn-icon ${className}`}
            {...props}
        >
            {Children.toArray(icon).map((item) => (
                <span key={isValidElement(item) ? item.key : undefined} className="icon">
                {item}
            </span>
            ))}
            {text && <span className="text">{text}</span>}
        </button>
    );
};