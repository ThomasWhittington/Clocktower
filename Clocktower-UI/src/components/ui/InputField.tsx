import type {InputHTMLAttributes} from "react";

interface InputFieldProps extends InputHTMLAttributes<HTMLInputElement> {
}

export const InputField = ({
                               className = "",
                               placeholder = "Enter text...",
                               ...props
                           }: InputFieldProps) => {
    return (
        <input
            className={`input-field ${className}`}
            placeholder={placeholder}
            {...props}
        />
    );
};