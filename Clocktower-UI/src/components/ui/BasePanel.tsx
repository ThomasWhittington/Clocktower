import type {ReactNode} from "react";
import {CloseButton} from "@/components/ui";

interface BasePanelProps {
    isOpen: boolean;
    onClose: () => void;
    title: string;
    children: ReactNode;
    className?: string;
}

export const BasePanel = ({isOpen, onClose, title, className, children}: BasePanelProps) => {
    if (!isOpen) return null;

    return (
        <div className={className}>
            <div className="panel-backdrop">
                <div className="panel-content">
                    <div className="panel-header">
                        <h2 className="panel-title">{title}</h2>
                        <CloseButton onClick={onClose}/>
                    </div>
                    <div className="panel-body">
                        {children}
                    </div>
                </div>
            </div>
        </div>
    );
};