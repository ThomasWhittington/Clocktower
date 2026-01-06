import {type ReactNode} from "react";
import {StatusIndicator} from "@/components/ui";

interface ActionBannerProps {
    message: ReactNode;
    onCancel: () => void;
    cancelText?: string;
}

export function ActionBanner({message, onCancel, cancelText = "Cancel"}: Readonly<ActionBannerProps>) {
    return (
        <div className="action-banner">
            <div className="action-banner-content">
                <StatusIndicator/>
                {message}
            </div>
            <button onClick={onCancel} className="btn-danger">
                {cancelText}
            </button>
        </div>
    );
}