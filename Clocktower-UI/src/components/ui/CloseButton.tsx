import type {ButtonHTMLAttributes} from "react";

interface CloseButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
}

export const CloseButton = ({
                                className = "",
                                ...props
                            }: CloseButtonProps) => {

    return (
        <button className="btn-close"
                {...props}
        >✕</button>
    );
};