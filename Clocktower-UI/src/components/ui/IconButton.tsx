import {type ButtonHTMLAttributes, Children, isValidElement, type ReactNode} from "react";

interface IconButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
    icon: ReactNode | ReactNode[];
    text?: string;
    variant?: "primary" | "secondary" | "outline" | "danger";
    isActive?: boolean;
    isEnabled?: boolean;
}

export const IconButton = ({
                               icon,
                               text,
                               variant = 'secondary',
                               isActive,
                               isEnabled = true,
                               className = "",
                               ...props
                           }: IconButtonProps) => {
    let baseClass = isActive ? "btn-outline" : `btn-${variant}`;
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