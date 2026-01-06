import {type ReactNode} from "react";
import {StatusIndicator} from "@/components/ui";

interface ActionBannerProps {
    message: ReactNode;
    onCancel: () => void;
    cancelText?: string;
    statusColor?: "yellow" | "red" | "green";
}

export function ActionBanner({message, onCancel, cancelText = "Cancel", statusColor = "yellow"}: Readonly<ActionBannerProps>) {
    return (
        <div className="action-banner">
            <div className="action-banner-content">
                <StatusIndicator color={statusColor}/>
                {message}
            </div>
            <button onClick={onCancel} className="btn-danger" aria-label="Cancel">
                {cancelText}
            </button>
        </div>
    );
}